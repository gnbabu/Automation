-- TestScreenshot_ReturnGeneratedId_Migration.sql
-- Idempotent (safe to re-run). usp_InsertTestScreenshot previously returned nothing
-- (caller only got rows-affected via ExecuteNonQueryAsync) - now returns the generated
-- ScreenshotId via SCOPE_IDENTITY(), so a caller can link a specific screenshot back to
-- a specific TestCaseExecutionLog row via TestCaseExecutionLog.ScreenshotId (used by
-- BaseFeatureFixture.LogFailureIfAny's failure screenshot - see AGENTS.md). The bulk
-- insert path (usp_InsertBulkTestScreenshots / BulkInsertScreenshotsAsync, used by every
-- test class's own success-path TearDown) is unchanged - it still doesn't return IDs,
-- since success-path screenshots were never linked to a specific log row and don't need
-- to be.

SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE [aut].[usp_InsertTestScreenshot]
    @AssignmentTestCaseId INT,
    @Caption NVARCHAR(MAX),
    @Screenshot VARBINARY(MAX),
    @TakenAt DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO aut.TestScreenshots
        (AssignmentTestCaseId, Caption, Screenshot, TakenAt)
    VALUES
        (@AssignmentTestCaseId, @Caption, @Screenshot, ISNULL(@TakenAt, GETUTCDATE()));

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS ScreenshotId;
END;
GO
