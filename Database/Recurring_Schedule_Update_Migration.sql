-- Recurring_Schedule_Update_Migration.sql
-- Idempotent (safe to re-run). Adds Edit support for an existing recurring schedule.
--
-- The Assignment itself is deliberately NOT editable (matches ReleaseFormComponent's own
-- existing "lock identity fields once created" convention, e.g. Release Name/Version/
-- Environment locking once a release leaves Draft) - only cadence/execution settings
-- (RecurrenceType, DaysOfWeek, DayOfMonth, TimeOfDay, Browser, LoginUserId, EndDate) can be
-- changed. Changing which Assignment a schedule targets is a rarer, more consequential
-- action better done via delete + recreate.

SET QUOTED_IDENTIFIER ON;
GO

-- EnvironmentId wasn't previously returned by GetAll (only the display-only Environment
-- name string was) - needed here so the Edit form can resolve Login Users for the
-- (read-only, in edit mode) assignment's environment the same self-service way the
-- create form already does.
CREATE OR ALTER PROCEDURE aut.usp_RecurringSchedule_GetById
(
    @RecurringScheduleId INT
)
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
    WHERE rs.RecurringScheduleId = @RecurringScheduleId;
END
GO

CREATE OR ALTER PROCEDURE aut.usp_RecurringSchedule_Update
(
    @RecurringScheduleId INT,
    @RecurrenceType      NVARCHAR(20),
    @DaysOfWeek          NVARCHAR(20) = NULL,
    @DayOfMonth          INT = NULL,
    @TimeOfDay           TIME(0),
    @Browser             NVARCHAR(100),
    @LoginUserId         INT = NULL,
    @EndDate             DATETIME2(7) = NULL,
    @NextRunDate         DATETIME2(7)
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE aut.[RecurringSchedule]
    SET RecurrenceType = @RecurrenceType,
        DaysOfWeek = @DaysOfWeek,
        DayOfMonth = @DayOfMonth,
        TimeOfDay = @TimeOfDay,
        Browser = @Browser,
        LoginUserId = @LoginUserId,
        EndDate = @EndDate,
        NextRunDate = @NextRunDate
    WHERE RecurringScheduleId = @RecurringScheduleId;
END
GO
