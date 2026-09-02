-- TestCaseAssignment_Lock_Executed_Migration.sql
-- Idempotent (CREATE OR ALTER). Once a test case's assignment has moved past 'Assigned'
-- (Queued/Scheduled/InProgress/Passed/Failed/Cancelled - i.e. it has entered the execution
-- pipeline at all, not just finished), it becomes locked: this proc will no longer let a
-- resend of the same TestCaseId overwrite its status, remove it via a Save that omits it,
-- or remove it via Reset. Enforced here (DB-level) rather than only in the frontend, so a
-- stale/buggy client can't silently corrupt an executed result - this is deliberately the
-- single source of truth, with the frontend's disabled checkbox being a UX nicety on top,
-- not the actual enforcement.
--
-- Returns (via a final SELECT, read as a scalar by the caller) the number of test cases
-- that were locked and therefore left untouched by this call - 0 when nothing was skipped.
-- Callers that don't care about this (e.g. plain ExecuteNonQuery) simply ignore it.

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE aut.usp_CreateOrUpdateAssignmentWithTestCases
(
    @AssignmentStatus NVARCHAR(100),
    @AssignedUser     INT,
    @ReleaseName      NVARCHAR(255),
    @Environment      NVARCHAR(100),
    @AssignedDate     DATETIME,
    @AssignedBy       INT,
    @LastUpdatedDate  DATETIME,
    @TestCases        aut.TestCaseType READONLY,
    @ReleaseId        INT = NULL,
    @EnvironmentId    INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ExistingAssignmentId INT;
    DECLARE @TestCaseCount INT;
    DECLARE @TesterName NVARCHAR(255);
    DECLARE @AssignmentName NVARCHAR(255);
    DECLARE @ResolvedEnvironmentId INT = @EnvironmentId;
    DECLARE @LockedCount INT = 0;

    BEGIN TRANSACTION;

    DECLARE @RealReleaseName NVARCHAR(255) = NULL;

    BEGIN TRY
        SELECT @TesterName = UserName FROM aut.[User] WHERE UserID = @AssignedUser;

        IF @ReleaseId IS NOT NULL
            SELECT @RealReleaseName = ReleaseName FROM aut.Release WHERE ReleaseId = @ReleaseId;

        -- AssignmentName formula is unchanged; when a real Release is linked (@ReleaseId
        -- provided), its actual name is appended as an extra segment so assignments stay
        -- distinct per Release even when Library/Environment match. When @ReleaseId is
        -- NULL, output is byte-for-byte identical to the pre-Release-Management behavior.
        SET @AssignmentName = @TesterName + '-' + @ReleaseName + '-' + @Environment
            + CASE WHEN @RealReleaseName IS NOT NULL THEN '-' + @RealReleaseName ELSE '' END;

        -- Resolve EnvironmentId from text if not supplied
        IF @ResolvedEnvironmentId IS NULL AND @Environment IS NOT NULL
        BEGIN
            SELECT TOP 1 @ResolvedEnvironmentId = EnvironmentId
            FROM aut.[Environment]
            WHERE LTRIM(RTRIM(EnvironmentName)) = LTRIM(RTRIM(@Environment));
        END

        SELECT @ExistingAssignmentId = AssignmentId
        FROM aut.TestCaseAssignment
        WHERE AssignmentName = @AssignmentName
          AND AssignedUser = @AssignedUser;

        SELECT @TestCaseCount = COUNT(*) FROM @TestCases;

        -- Resetting an existing assignment down to zero test cases: only remove test cases
        -- still in the freely-modifiable 'Assigned' state. Locked ones (anything that has
        -- ever entered the execution pipeline) are left in place - and so, since the
        -- assignment then still has test cases, the TestCaseAssignment row itself is left
        -- in place too (not permanently deleted).
        IF @ExistingAssignmentId IS NOT NULL AND @TestCaseCount = 0
        BEGIN
            SELECT @LockedCount = COUNT(*)
            FROM aut.AssignedTestCases
            WHERE AssignmentId = @ExistingAssignmentId
              AND TestCaseStatus <> 'Assigned';

            -- Clean up dependents only for the test cases we're actually about to delete
            -- (the still-'Assigned', unlocked ones).
            DELETE Q
            FROM aut.TestCaseExecutionQueue Q
            INNER JOIN aut.AssignedTestCases ATC ON ATC.AssignmentTestCaseId = Q.AssignmentTestCaseId
            WHERE ATC.AssignmentId = @ExistingAssignmentId
              AND ATC.TestCaseStatus = 'Assigned';

            DELETE S
            FROM aut.TestScreenshots S
            INNER JOIN aut.AssignedTestCases ATC ON ATC.AssignmentTestCaseId = S.AssignmentTestCaseId
            WHERE ATC.AssignmentId = @ExistingAssignmentId
              AND ATC.TestCaseStatus = 'Assigned';

            DELETE L
            FROM aut.TestCaseExecutionLogs L
            INNER JOIN aut.AssignedTestCases ATC ON ATC.AssignmentTestCaseId = L.AssignmentTestCaseId
            WHERE ATC.AssignmentId = @ExistingAssignmentId
              AND ATC.TestCaseStatus = 'Assigned';

            DELETE FROM aut.AssignedTestCases
            WHERE AssignmentId = @ExistingAssignmentId
              AND TestCaseStatus = 'Assigned';

            IF @LockedCount = 0
            BEGIN
                -- Nothing left locked behind - genuinely empty now, so the assignment
                -- itself is gone too (unchanged from the prior permanent-delete behavior).
                DELETE FROM aut.TestCaseExecutionQueue WHERE AssignmentId = @ExistingAssignmentId;
                DELETE FROM aut.TestCaseAssignment WHERE AssignmentId = @ExistingAssignmentId;
            END
            ELSE
            BEGIN
                UPDATE aut.TestCaseAssignment
                SET LastUpdatedDate = @LastUpdatedDate
                WHERE AssignmentId = @ExistingAssignmentId;
            END

            COMMIT TRANSACTION;
            SELECT @LockedCount AS LockedCount;
            RETURN;
        END

        IF @ExistingAssignmentId IS NULL
        BEGIN
            IF @TestCaseCount = 0
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 0 AS LockedCount;
                RETURN;
            END

            INSERT INTO aut.TestCaseAssignment
            (
                AssignmentName, AssignmentStatus, AssignedUser,
                ReleaseName, Environment, ReleaseId, EnvironmentId,
                AssignedDate, AssignedBy, LastUpdatedDate
            )
            VALUES
            (
                @AssignmentName, @AssignmentStatus, @AssignedUser,
                @ReleaseName, @Environment, @ReleaseId, @ResolvedEnvironmentId,
                @AssignedDate, @AssignedBy, @LastUpdatedDate
            );

            SET @ExistingAssignmentId = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            -- Keep FK columns current on existing assignments
            UPDATE aut.TestCaseAssignment
            SET ReleaseId       = COALESCE(@ReleaseId, ReleaseId),
                EnvironmentId   = COALESCE(@ResolvedEnvironmentId, EnvironmentId),
                AssignmentStatus = @AssignmentStatus,
                LastUpdatedDate  = @LastUpdatedDate
            WHERE AssignmentId = @ExistingAssignmentId;
        END

        -- Count (for the caller's info) how many locked test cases are being left alone
        -- because the incoming @TestCases omitted them (e.g. a stale client that doesn't
        -- know about a status change) - these are NOT deleted below.
        SELECT @LockedCount = COUNT(*)
        FROM aut.AssignedTestCases ATC
        WHERE ATC.AssignmentId = @ExistingAssignmentId
          AND ATC.TestCaseStatus <> 'Assigned'
          AND NOT EXISTS (SELECT 1 FROM @TestCases TC WHERE ATC.TestCaseId = TC.TestCaseId);

        -- Clean up dependent rows for test cases about to be removed, BEFORE deleting
        -- AssignedTestCases itself - TestCaseExecutionQueue/TestScreenshots both have FKs
        -- on AssignmentTestCaseId with no cascade, so the delete below would otherwise fail
        -- for any test case that was ever queued/executed or has a screenshot. Only ever
        -- targets still-'Assigned' (unlocked) rows - a locked row is never removed here.
        DELETE Q
        FROM aut.TestCaseExecutionQueue Q
        INNER JOIN aut.AssignedTestCases ATC ON ATC.AssignmentTestCaseId = Q.AssignmentTestCaseId
        WHERE ATC.AssignmentId = @ExistingAssignmentId
          AND ATC.TestCaseStatus = 'Assigned'
          AND NOT EXISTS (
                SELECT 1 FROM @TestCases TC WHERE ATC.TestCaseId = TC.TestCaseId
            );

        DELETE S
        FROM aut.TestScreenshots S
        INNER JOIN aut.AssignedTestCases ATC ON ATC.AssignmentTestCaseId = S.AssignmentTestCaseId
        WHERE ATC.AssignmentId = @ExistingAssignmentId
          AND ATC.TestCaseStatus = 'Assigned'
          AND NOT EXISTS (
                SELECT 1 FROM @TestCases TC WHERE ATC.TestCaseId = TC.TestCaseId
            );

        DELETE L
        FROM aut.TestCaseExecutionLogs L
        INNER JOIN aut.AssignedTestCases ATC ON ATC.AssignmentTestCaseId = L.AssignmentTestCaseId
        WHERE ATC.AssignmentId = @ExistingAssignmentId
          AND ATC.TestCaseStatus = 'Assigned'
          AND NOT EXISTS (
                SELECT 1 FROM @TestCases TC WHERE ATC.TestCaseId = TC.TestCaseId
            );

        -- Delete removed test cases (only ones still 'Assigned' - locked ones are kept
        -- even if the caller's @TestCases list omitted them)
        DELETE ATC
        FROM aut.AssignedTestCases ATC
        WHERE ATC.AssignmentId = @ExistingAssignmentId
          AND ATC.TestCaseStatus = 'Assigned'
          AND NOT EXISTS (
                SELECT 1 FROM @TestCases TC
                WHERE ATC.TestCaseId = TC.TestCaseId
           );

        -- Insert / update test cases. WHEN MATCHED only fires while the existing row is
        -- still 'Assigned' - once a test case is Queued/Scheduled/InProgress/Passed/
        -- Failed/Cancelled, resending it (e.g. because it's still checked in the UI) no
        -- longer touches it at all.
        MERGE aut.AssignedTestCases AS Target
        USING @TestCases AS Source
        ON Target.AssignmentId = @ExistingAssignmentId
           AND Target.TestCaseId = Source.TestCaseId
        WHEN MATCHED AND Target.TestCaseStatus = 'Assigned' THEN
            UPDATE SET
                TestCaseDescription = Source.TestCaseDescription,
                TestCaseStatus      = Source.TestCaseStatus,
                ClassName           = Source.ClassName,
                LibraryName         = Source.LibraryName,
                MethodName          = Source.MethodName,
                Priority            = Source.Priority
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (AssignmentId, TestCaseId, TestCaseDescription, TestCaseStatus,
                    ClassName, LibraryName, MethodName, Priority)
            VALUES (@ExistingAssignmentId, Source.TestCaseId, Source.TestCaseDescription,
                    Source.TestCaseStatus, Source.ClassName, Source.LibraryName,
                    Source.MethodName, Source.Priority);

        COMMIT TRANSACTION;
        SELECT @LockedCount AS LockedCount;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
