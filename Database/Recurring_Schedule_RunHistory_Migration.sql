-- Recurring_Schedule_RunHistory_Migration.sql
-- Idempotent (safe to re-run). Adds run-count tracking + a per-firing history (with
-- resolved Passed/Failed/Skipped outcomes) on top of aut.RecurringSchedule.
--
-- Outcome attribution note: aut.AssignedTestCases.TestCaseStatus is a *live*, mutable
-- column that gets overwritten by whichever run (manual or recurring) most recently
-- touched a given test case - it cannot tell you the outcome of one specific historical
-- firing versus another. aut.RecurringScheduleRunHistoryTestCase exists purely to snapshot
-- exactly which AssignmentTestCaseIds belonged to one firing, so that firing's outcome can
-- still be correctly attributed later even if a newer firing has since re-run (and
-- overwritten the status of) the same test case.

SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('aut.RecurringSchedule') AND name = 'RunCount')
BEGIN
    ALTER TABLE aut.[RecurringSchedule] ADD [RunCount] INT NOT NULL CONSTRAINT [DF_RecurringSchedule_RunCount] DEFAULT (0);
    PRINT 'Added aut.RecurringSchedule.RunCount';
END
GO

IF OBJECT_ID('aut.RecurringScheduleRunHistory', 'U') IS NULL
BEGIN
    CREATE TABLE aut.[RecurringScheduleRunHistory](
        [RunHistoryId]          BIGINT IDENTITY(1,1) NOT NULL,
        [RecurringScheduleId]   INT NOT NULL,
        [RunDate]               DATETIME2(7) NOT NULL CONSTRAINT [DF_RecurringScheduleRunHistory_RunDate] DEFAULT (SYSDATETIME()),
        -- 'Queued' | 'NoEligibleTestCases' | 'Paused'
        [Result]                NVARCHAR(30) NOT NULL,
        [Detail]                NVARCHAR(300) NULL,
        [TestCasesQueuedCount]  INT NOT NULL CONSTRAINT [DF_RecurringScheduleRunHistory_QueuedCount] DEFAULT (0),
        -- 'Pending' (queued, outcome not yet known) | 'Resolved' | 'NotApplicable' (nothing queued this firing)
        [ResolutionStatus]      NVARCHAR(20) NOT NULL CONSTRAINT [DF_RecurringScheduleRunHistory_ResolutionStatus] DEFAULT ('NotApplicable'),
        [PassedCount]           INT NULL,
        [FailedCount]           INT NULL,
        [SkippedCount]          INT NULL,
        [ResolvedOn]            DATETIME2(7) NULL,
        CONSTRAINT [PK_RecurringScheduleRunHistory] PRIMARY KEY CLUSTERED ([RunHistoryId] ASC),
        -- ON DELETE CASCADE - usp_RecurringSchedule_Delete is a plain DELETE with no
        -- explicit cleanup of dependent history rows; without cascade, deleting any
        -- schedule that has ever fired (i.e. has RunHistory rows) would fail with an FK
        -- violation. Matches this codebase's existing cascade-delete convention (see
        -- Flow/Section Management's own cascade deletion).
        CONSTRAINT [FK_RecurringScheduleRunHistory_Schedule] FOREIGN KEY ([RecurringScheduleId])
            REFERENCES aut.[RecurringSchedule] ([RecurringScheduleId]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_RecurringScheduleRunHistory_Schedule] ON aut.[RecurringScheduleRunHistory] ([RecurringScheduleId], [RunDate] DESC);
    CREATE NONCLUSTERED INDEX [IX_RecurringScheduleRunHistory_Pending] ON aut.[RecurringScheduleRunHistory] ([ResolutionStatus]) WHERE [ResolutionStatus] = 'Pending';
    PRINT 'Created aut.RecurringScheduleRunHistory';
END
GO

IF OBJECT_ID('aut.RecurringScheduleRunHistoryTestCase', 'U') IS NULL
BEGIN
    CREATE TABLE aut.[RecurringScheduleRunHistoryTestCase](
        [RunHistoryId]          BIGINT NOT NULL,
        [AssignmentTestCaseId]  INT NOT NULL,
        CONSTRAINT [PK_RecurringScheduleRunHistoryTestCase] PRIMARY KEY CLUSTERED ([RunHistoryId] ASC, [AssignmentTestCaseId] ASC),
        CONSTRAINT [FK_RecurringScheduleRunHistoryTestCase_History] FOREIGN KEY ([RunHistoryId])
            REFERENCES aut.[RecurringScheduleRunHistory] ([RunHistoryId]) ON DELETE CASCADE
    );
    PRINT 'Created aut.RecurringScheduleRunHistoryTestCase';
END
GO

-- Called by RecurringScheduleWorker right after each firing attempt (Queued/
-- NoEligibleTestCases/Paused). Only increments RunCount for an actual 'Queued' firing -
-- matches the plain-English "how many times has it run".
CREATE OR ALTER PROCEDURE aut.usp_RecurringSchedule_AddRunHistory
(
    @RecurringScheduleId    INT,
    @Result                 NVARCHAR(30),
    @Detail                 NVARCHAR(300) = NULL,
    @AssignmentTestCaseIds  aut.AssignmentTestCaseIdList READONLY
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @QueuedCount INT = (SELECT COUNT(*) FROM @AssignmentTestCaseIds);
    DECLARE @ResolutionStatus NVARCHAR(20) = CASE WHEN @Result = 'Queued' AND @QueuedCount > 0 THEN 'Pending' ELSE 'NotApplicable' END;
    DECLARE @RunHistoryId BIGINT;

    INSERT INTO aut.[RecurringScheduleRunHistory]
        (RecurringScheduleId, Result, Detail, TestCasesQueuedCount, ResolutionStatus)
    VALUES
        (@RecurringScheduleId, @Result, @Detail, @QueuedCount, @ResolutionStatus);

    SET @RunHistoryId = SCOPE_IDENTITY();

    IF @QueuedCount > 0
    BEGIN
        INSERT INTO aut.[RecurringScheduleRunHistoryTestCase] (RunHistoryId, AssignmentTestCaseId)
        SELECT @RunHistoryId, AssignmentTestCaseId FROM @AssignmentTestCaseIds;
    END

    IF @Result = 'Queued' AND @QueuedCount > 0
    BEGIN
        UPDATE aut.[RecurringSchedule] SET RunCount = RunCount + 1 WHERE RecurringScheduleId = @RecurringScheduleId;
    END

    SELECT @RunHistoryId AS RunHistoryId;
END
GO

-- Set-based sweep, called once per RecurringScheduleWorker poll cycle regardless of
-- whether any schedules were due that cycle. For every 'Pending' history row where none of
-- its snapshotted test cases are still in-flight (Queued/Scheduled/InProgress), computes
-- Passed/Failed/Skipped counts from their *current* status and marks the row Resolved.
CREATE OR ALTER PROCEDURE aut.usp_RecurringSchedule_ResolvePendingRunHistory
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH ReadyToResolve AS (
        SELECT h.RunHistoryId
        FROM aut.[RecurringScheduleRunHistory] h
        WHERE h.ResolutionStatus = 'Pending'
          AND NOT EXISTS (
              SELECT 1
              FROM aut.[RecurringScheduleRunHistoryTestCase] rt
              JOIN aut.[AssignedTestCases] atc ON atc.AssignmentTestCaseId = rt.AssignmentTestCaseId
              WHERE rt.RunHistoryId = h.RunHistoryId
                AND atc.TestCaseStatus IN ('Queued', 'Scheduled', 'InProgress')
          )
    ),
    Outcomes AS (
        SELECT
            rt.RunHistoryId,
            SUM(CASE WHEN atc.TestCaseStatus = 'Passed' THEN 1 ELSE 0 END) AS PassedCount,
            SUM(CASE WHEN atc.TestCaseStatus = 'Failed' THEN 1 ELSE 0 END) AS FailedCount,
            SUM(CASE WHEN atc.TestCaseStatus = 'Skipped' THEN 1 ELSE 0 END) AS SkippedCount
        FROM aut.[RecurringScheduleRunHistoryTestCase] rt
        JOIN aut.[AssignedTestCases] atc ON atc.AssignmentTestCaseId = rt.AssignmentTestCaseId
        WHERE rt.RunHistoryId IN (SELECT RunHistoryId FROM ReadyToResolve)
        GROUP BY rt.RunHistoryId
    )
    UPDATE h
    SET h.ResolutionStatus = 'Resolved',
        h.PassedCount = o.PassedCount,
        h.FailedCount = o.FailedCount,
        h.SkippedCount = o.SkippedCount,
        h.ResolvedOn = SYSDATETIME()
    FROM aut.[RecurringScheduleRunHistory] h
    JOIN Outcomes o ON o.RunHistoryId = h.RunHistoryId;
END
GO

-- Used by the new /recurring-schedules/:id/history page.
CREATE OR ALTER PROCEDURE aut.usp_RecurringSchedule_GetRunHistory
(
    @RecurringScheduleId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        RunHistoryId, RecurringScheduleId, RunDate, Result, Detail, TestCasesQueuedCount,
        ResolutionStatus, PassedCount, FailedCount, SkippedCount, ResolvedOn
    FROM aut.[RecurringScheduleRunHistory]
    WHERE RecurringScheduleId = @RecurringScheduleId
    ORDER BY RunDate DESC;
END
GO

-- Adds RunCount + Login User role/name (mirrors the "${userRole} - ${userName}" display
-- convention already used by the create form's loginUserLabel) to the management list.
CREATE OR ALTER PROCEDURE aut.usp_RecurringSchedule_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        rs.RecurringScheduleId, rs.AssignmentId, ta.AssignmentName, ta.Environment, ta.EnvironmentId,
        r.ReleaseName, r.ReleaseLifecycle,
        rs.RecurrenceType, rs.DaysOfWeek, rs.DayOfMonth, rs.TimeOfDay, rs.Browser,
        rs.LoginUserId, lu.UserRole AS LoginUserRole, lu.UserName AS LoginUserName,
        rs.IsActive, rs.PausedReason, rs.EndDate, rs.NextRunDate,
        rs.LastRunDate, rs.RunCount, rs.CreatedBy, rs.CreatedOn
    FROM aut.[RecurringSchedule] rs
    JOIN aut.[TestCaseAssignment] ta ON ta.AssignmentId = rs.AssignmentId
    LEFT JOIN aut.[Release] r ON r.ReleaseId = ta.ReleaseId
    LEFT JOIN aut.[LoginUser] lu ON lu.LoginUserId = rs.LoginUserId
    ORDER BY rs.CreatedOn DESC;
END
GO
