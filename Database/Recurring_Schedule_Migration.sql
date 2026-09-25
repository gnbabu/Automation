-- Recurring_Schedule_Migration.sql
-- Idempotent (safe to re-run). Adds a repeating (Daily/Weekly/Monthly) schedule on top
-- of the existing one-time Schedule/Bulk Schedule pipeline (usp_ScheduleSingleTestCase/
-- usp_BulkScheduleTestCases -> aut.TestCaseExecutionQueue -> TestQueueWorker). A recurring
-- schedule does not execute anything itself - RecurringScheduleWorker periodically calls
-- the *existing* BulkScheduleAsync repository method to insert fresh
-- aut.TestCaseExecutionQueue rows, so actual test execution is unchanged.
--
-- Scope is dynamic (AssignmentId, not a snapshot of AssignmentTestCaseIds) - the current
-- test cases in the assignment are looked up fresh every time the schedule fires, so
-- test cases added to the assignment later are automatically included.

SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('aut.RecurringSchedule', 'U') IS NULL
BEGIN
    CREATE TABLE aut.[RecurringSchedule](
        [RecurringScheduleId] INT IDENTITY(1,1) NOT NULL,
        [AssignmentId]        INT NOT NULL,
        [RecurrenceType]      NVARCHAR(20) NOT NULL,
        [DaysOfWeek]          NVARCHAR(20) NULL,
        [DayOfMonth]          INT NULL,
        [TimeOfDay]           TIME(0) NOT NULL,
        [Browser]             NVARCHAR(100) NOT NULL,
        [LoginUserId]         INT NULL,
        [IsActive]            BIT NOT NULL CONSTRAINT [DF_RecurringSchedule_IsActive] DEFAULT (1),
        [PausedReason]        NVARCHAR(300) NULL,
        [EndDate]             DATETIME2(7) NULL,
        [NextRunDate]         DATETIME2(7) NOT NULL,
        [LastRunDate]         DATETIME2(7) NULL,
        [CreatedBy]           NVARCHAR(100) NULL,
        [CreatedOn]           DATETIME2(7) NOT NULL CONSTRAINT [DF_RecurringSchedule_CreatedOn] DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_RecurringSchedule] PRIMARY KEY CLUSTERED ([RecurringScheduleId] ASC),
        CONSTRAINT [FK_RecurringSchedule_Assignment] FOREIGN KEY ([AssignmentId])
            REFERENCES aut.[TestCaseAssignment] ([AssignmentId])
    );
    CREATE NONCLUSTERED INDEX [IX_RecurringSchedule_NextRunDate] ON aut.[RecurringSchedule] ([IsActive], [NextRunDate]);
    PRINT 'Created aut.RecurringSchedule';
END
GO

CREATE OR ALTER PROCEDURE aut.usp_RecurringSchedule_Create
(
    @AssignmentId    INT,
    @RecurrenceType  NVARCHAR(20),
    @DaysOfWeek      NVARCHAR(20) = NULL,
    @DayOfMonth      INT = NULL,
    @TimeOfDay       TIME(0),
    @Browser         NVARCHAR(100),
    @LoginUserId     INT = NULL,
    @EndDate         DATETIME2(7) = NULL,
    @NextRunDate     DATETIME2(7),
    @CreatedBy       NVARCHAR(100) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO aut.[RecurringSchedule]
        (AssignmentId, RecurrenceType, DaysOfWeek, DayOfMonth, TimeOfDay, Browser, LoginUserId, EndDate, NextRunDate, CreatedBy)
    VALUES
        (@AssignmentId, @RecurrenceType, @DaysOfWeek, @DayOfMonth, @TimeOfDay, @Browser, @LoginUserId, @EndDate, @NextRunDate, @CreatedBy);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS RecurringScheduleId;
END
GO

CREATE OR ALTER PROCEDURE aut.usp_RecurringSchedule_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        rs.RecurringScheduleId, rs.AssignmentId, ta.AssignmentName, ta.Environment,
        r.ReleaseName, r.ReleaseLifecycle,
        rs.RecurrenceType, rs.DaysOfWeek, rs.DayOfMonth, rs.TimeOfDay, rs.Browser,
        rs.LoginUserId, rs.IsActive, rs.PausedReason, rs.EndDate, rs.NextRunDate,
        rs.LastRunDate, rs.CreatedBy, rs.CreatedOn
    FROM aut.[RecurringSchedule] rs
    JOIN aut.[TestCaseAssignment] ta ON ta.AssignmentId = rs.AssignmentId
    LEFT JOIN aut.[Release] r ON r.ReleaseId = ta.ReleaseId
    ORDER BY rs.CreatedOn DESC;
END
GO

CREATE OR ALTER PROCEDURE aut.usp_RecurringSchedule_SetActive
(
    @RecurringScheduleId INT,
    @IsActive            BIT,
    @PausedReason        NVARCHAR(300) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE aut.[RecurringSchedule]
    SET IsActive = @IsActive,
        PausedReason = CASE WHEN @IsActive = 1 THEN NULL ELSE @PausedReason END
    WHERE RecurringScheduleId = @RecurringScheduleId;
END
GO

CREATE OR ALTER PROCEDURE aut.usp_RecurringSchedule_Delete
(
    @RecurringScheduleId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM aut.[RecurringSchedule] WHERE RecurringScheduleId = @RecurringScheduleId;
END
GO

-- Used by RecurringScheduleWorker each poll cycle - joined to Release.ReleaseLifecycle so
-- the worker can detect a permanently-done (Completed/Rejected) release without a second
-- round-trip.
CREATE OR ALTER PROCEDURE aut.usp_RecurringSchedule_GetDue
(
    @Now DATETIME2(7)
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        rs.RecurringScheduleId, rs.AssignmentId, ta.AssignmentName, r.ReleaseName, r.ReleaseLifecycle,
        rs.RecurrenceType, rs.DaysOfWeek, rs.DayOfMonth, rs.TimeOfDay, rs.Browser,
        rs.LoginUserId, rs.EndDate
    FROM aut.[RecurringSchedule] rs
    JOIN aut.[TestCaseAssignment] ta ON ta.AssignmentId = rs.AssignmentId
    LEFT JOIN aut.[Release] r ON r.ReleaseId = ta.ReleaseId
    WHERE rs.IsActive = 1
      AND rs.NextRunDate <= @Now
      AND (rs.EndDate IS NULL OR rs.EndDate > @Now);
END
GO

CREATE OR ALTER PROCEDURE aut.usp_RecurringSchedule_MarkRun
(
    @RecurringScheduleId INT,
    @NextRunDate         DATETIME2(7) = NULL,
    @IsActive            BIT,
    @PausedReason        NVARCHAR(300) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE aut.[RecurringSchedule]
    SET LastRunDate = SYSDATETIME(),
        NextRunDate = COALESCE(@NextRunDate, NextRunDate),
        IsActive = @IsActive,
        PausedReason = CASE WHEN @IsActive = 1 THEN NULL ELSE @PausedReason END
    WHERE RecurringScheduleId = @RecurringScheduleId;
END
GO

-- Current, eligible (not already Queued/Scheduled/InProgress) test cases for an
-- assignment - deliberately does NOT exclude 'Passed' (unlike the one-time Schedule
-- dialog's isTestCaseSelectable), since re-running already-passed tests on a cadence is
-- the entire point of a regression schedule.
CREATE OR ALTER PROCEDURE aut.usp_RecurringSchedule_GetEligibleTestCaseIds
(
    @AssignmentId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AssignmentTestCaseId
    FROM aut.[AssignedTestCases]
    WHERE AssignmentId = @AssignmentId
      AND TestCaseStatus NOT IN ('Queued', 'Scheduled', 'InProgress');
END
GO

-- Lightweight assignment picker list for the Recurring Schedule creation form.
-- EnvironmentId is included so the frontend can resolve Login Users the same
-- self-service way ScheduleTestcasesDialogComponent already does (look up the
-- environment's RequiresAuthentication flag, then the current user's own credential).
-- Only Active-release assignments are offered - matches RecurringScheduleController.
-- Create's own server-side lifecycle check exactly, so the dropdown never lets someone
-- pick something guaranteed to be rejected on submit.
CREATE OR ALTER PROCEDURE aut.usp_RecurringSchedule_GetAssignmentOptions
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        ta.AssignmentId, ta.AssignmentName, ta.Environment, ta.EnvironmentId, ta.ReleaseId, r.ReleaseName, r.ReleaseLifecycle
    FROM aut.[TestCaseAssignment] ta
    JOIN aut.[Release] r ON r.ReleaseId = ta.ReleaseId
    WHERE r.ReleaseLifecycle = 'Active'
    ORDER BY ta.AssignmentId DESC;
END
GO
