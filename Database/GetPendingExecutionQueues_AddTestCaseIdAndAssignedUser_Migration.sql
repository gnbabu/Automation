-- GetPendingExecutionQueues_AddTestCaseIdAndAssignedUser_Migration.sql
-- Idempotent (safe to re-run). Adds ATC.TestCaseId and TCA.AssignedUser to
-- usp_GetPendingExecutionQueues's result - needed by TestQueueWorker to build a
-- scheduled-run failure notification (subject line's human-readable TestCaseId, and the
-- assigned user to notify) without a second round trip. Purely additive - existing
-- columns/behavior unchanged.

SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE aut.usp_GetPendingExecutionQueues
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Q.QueueId,
        Q.AssignmentTestCaseId,
        ATC.AssignmentId,
        ATC.TestCaseId,
        ATC.LibraryName,
        ATC.ClassName,
        ATC.MethodName,
        TCA.Environment,
        TCA.EnvironmentId,
        TCA.ReleaseId,
        TCA.AssignedUser,
        Q.Browser,
        Q.LoginUserId,
        Q.QueueStatus,
        Q.ExecutionDateTime

    FROM aut.TestCaseExecutionQueue Q
    INNER JOIN aut.AssignedTestCases ATC
        ON Q.AssignmentTestCaseId = ATC.AssignmentTestCaseId
    INNER JOIN aut.TestCaseAssignment TCA
        ON ATC.AssignmentId = TCA.AssignmentId

    WHERE
        Q.QueueStatus = 'Queued'
        OR (Q.QueueStatus = 'Scheduled' AND Q.ExecutionDateTime <= GETDATE())

    ORDER BY
        Q.CreatedDate ASC;
END
GO
