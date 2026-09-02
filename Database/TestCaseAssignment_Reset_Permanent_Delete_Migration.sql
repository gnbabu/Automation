-- TestCaseAssignment_Reset_Permanent_Delete_Migration.sql
-- Idempotent (CREATE OR ALTER). Changes "Reset Assignments" from a soft-delete (kept the
-- aut.TestCaseAssignment row, just flipped AssignmentStatus to 'Removed') to a real,
-- permanent delete of the assignment row itself, per explicit request.
--
-- Previously: Reset (assignmentStatus='Removed', empty @TestCases) removed all
-- AssignedTestCases rows (and, since the prior fix in
-- TestCaseAssignment_Reset_FK_Fix_Migration.sql, their dependent Queue/Screenshots/Logs
-- rows too) but left the TestCaseAssignment row itself in place with
-- AssignmentStatus = 'Removed' - a soft delete. Confirmed via a real Reset call that this
-- is exactly what happened (AssignmentId 28 stayed, just with Status='Removed').
--
-- Now: an existing assignment being reset to zero test cases is treated as a full,
-- permanent delete of the TestCaseAssignment row itself (and everything under it), not an
-- update. The next time that same Tester+Library+Release combination is assigned test
-- cases again, a brand-new AssignmentId is created - there's no lingering 'Removed' row.
--
-- A brand-new assignment created with zero test cases (@ExistingAssignmentId IS NULL AND
-- @TestCaseCount = 0) still just no-ops (rolls back), unchanged from before - there's
-- nothing to create or delete in that case.

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

        -- Resetting an existing assignment down to zero test cases = permanent delete of
        -- the whole assignment, not a soft 'Removed' status update. Handle this up front
        -- and exit early; everything below is for the "still has test cases" path.
        IF @ExistingAssignmentId IS NOT NULL AND @TestCaseCount = 0
        BEGIN
            -- TestCaseExecutionQueue has two separate FKs into this data - one via
            -- AssignmentTestCaseId (AssignedTestCases), one directly via AssignmentId
            -- (TestCaseAssignment, FK_TestCaseExecutionQueue_Assignment) - delete by both
            -- to be safe, though in practice every Queue row for this assignment will be
            -- caught by the AssignmentId delete alone.
            DELETE FROM aut.TestCaseExecutionQueue WHERE AssignmentId = @ExistingAssignmentId;

            DELETE S
            FROM aut.TestScreenshots S
            INNER JOIN aut.AssignedTestCases ATC ON ATC.AssignmentTestCaseId = S.AssignmentTestCaseId
            WHERE ATC.AssignmentId = @ExistingAssignmentId;

            DELETE L
            FROM aut.TestCaseExecutionLogs L
            INNER JOIN aut.AssignedTestCases ATC ON ATC.AssignmentTestCaseId = L.AssignmentTestCaseId
            WHERE ATC.AssignmentId = @ExistingAssignmentId;

            DELETE FROM aut.AssignedTestCases WHERE AssignmentId = @ExistingAssignmentId;

            DELETE FROM aut.TestCaseAssignment WHERE AssignmentId = @ExistingAssignmentId;

            COMMIT TRANSACTION;
            RETURN;
        END

        IF @ExistingAssignmentId IS NULL
        BEGIN
            IF @TestCaseCount = 0
            BEGIN
                ROLLBACK TRANSACTION;
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

        -- Clean up dependent rows for test cases about to be removed, BEFORE deleting
        -- AssignedTestCases itself - TestCaseExecutionQueue/TestScreenshots both have FKs
        -- on AssignmentTestCaseId with no cascade, so the delete below would otherwise fail
        -- for any test case that was ever queued/executed or has a screenshot.
        DELETE Q
        FROM aut.TestCaseExecutionQueue Q
        INNER JOIN aut.AssignedTestCases ATC ON ATC.AssignmentTestCaseId = Q.AssignmentTestCaseId
        WHERE ATC.AssignmentId = @ExistingAssignmentId
          AND NOT EXISTS (
                SELECT 1 FROM @TestCases TC WHERE ATC.TestCaseId = TC.TestCaseId
            );

        DELETE S
        FROM aut.TestScreenshots S
        INNER JOIN aut.AssignedTestCases ATC ON ATC.AssignmentTestCaseId = S.AssignmentTestCaseId
        WHERE ATC.AssignmentId = @ExistingAssignmentId
          AND NOT EXISTS (
                SELECT 1 FROM @TestCases TC WHERE ATC.TestCaseId = TC.TestCaseId
            );

        DELETE L
        FROM aut.TestCaseExecutionLogs L
        INNER JOIN aut.AssignedTestCases ATC ON ATC.AssignmentTestCaseId = L.AssignmentTestCaseId
        WHERE ATC.AssignmentId = @ExistingAssignmentId
          AND NOT EXISTS (
                SELECT 1 FROM @TestCases TC WHERE ATC.TestCaseId = TC.TestCaseId
            );

        -- Delete removed test cases
        DELETE ATC
        FROM aut.AssignedTestCases ATC
        WHERE ATC.AssignmentId = @ExistingAssignmentId
          AND NOT EXISTS (
                SELECT 1 FROM @TestCases TC
                WHERE ATC.TestCaseId = TC.TestCaseId
           );

        -- Insert / update test cases
        MERGE aut.AssignedTestCases AS Target
        USING @TestCases AS Source
        ON Target.AssignmentId = @ExistingAssignmentId
           AND Target.TestCaseId = Source.TestCaseId
        WHEN MATCHED THEN
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
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
