-- TestCaseAssignment_Reset_FK_Fix_Migration.sql
-- Idempotent (CREATE OR ALTER). Fixes "Reset Assignments" (and any Save that deselects a
-- previously-selected test case) failing with:
--   "The DELETE statement conflicted with the REFERENCE constraint
--    'FK_TestCaseExecutionQueue_AssignedTestCase'."
--
-- Root cause: usp_CreateOrUpdateAssignmentWithTestCases's "delete removed test cases" step
-- deletes aut.AssignedTestCases rows no longer present in @TestCases, but
-- aut.TestCaseExecutionQueue and aut.TestScreenshots both have FKs (AssignmentTestCaseId)
-- referencing AssignedTestCases with NO cascade delete configured - so removing any test
-- case that was ever queued/executed (has a Queue row) or has a screenshot fails outright
-- with a FK violation. This was pre-existing (not introduced by any recent Release/Manager/
-- Viewer work) - just never exercised on a test case with real execution history until now.
--
-- Fix: before deleting a to-be-removed AssignedTestCases row, also delete its dependent
-- TestCaseExecutionQueue/TestScreenshots rows (the actual FK-enforced blockers) and
-- TestCaseExecutionLogs rows (no FK forces this, but leaving them orphaned - referencing a
-- since-deleted AssignmentTestCaseId - makes no sense either). Un-assigning/resetting a
-- test case is a deliberate action that should also clear its execution history, since
-- re-assigning it later starts fresh.
--
-- Verified none of AssignedTestCases/TestCaseExecutionQueue/TestScreenshots/
-- TestCaseExecutionLogs/TestCaseAssignment have a filtered index, so QUOTED_IDENTIFIER
-- isn't actually load-bearing here - set explicitly anyway per this project's established
-- convention (see AGENTS.md's aut.Release/aut.[User] notes) so this proc's baked-in
-- setting is never a foot-gun if a filtered index is ever added to any of these tables.
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

        IF @ExistingAssignmentId IS NULL
        BEGIN
            SELECT @TestCaseCount = COUNT(*) FROM @TestCases;
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
