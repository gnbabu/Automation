/*============================================================================
  Test data from code models, saved per test case
  Target database: MES_AUT_AI   Schema: aut

  Purpose:
    A test declares its test-data model with [TestDataFlow(typeof(Model))]. The
    Automation API reads that model from the Release folder by reflection: the
    class is the Flow, its properties are the Sections (List<T> = multi-row), and
    the section classes' properties are the Fields. Sections created this way are
    stored in aut.AutomationDataSections with IsFromModel = 1 (read-only in Flow &
    Section Management).

    Values are saved per test case (LibraryName + ClassName + MethodName - the
    same identity execution filters by, stable across releases) x User x
    Environment as one JSON document - see TestDataJson_Migration.sql, which must
    be run after this script.

    aut.AutomationData (manual, per-flow data for test cases without a model) is
    not touched.

  Principles: IDEMPOTENT - guarded DDL + CREATE OR ALTER procedures.
============================================================================*/

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/*----------------------------------------------------------------------------
  1. Schema
----------------------------------------------------------------------------*/
IF COL_LENGTH('aut.AutomationDataSections', 'IsFromModel') IS NULL
BEGIN
    ALTER TABLE aut.AutomationDataSections
        ADD IsFromModel BIT NOT NULL CONSTRAINT DF_AutomationDataSections_IsFromModel DEFAULT 0;
    PRINT 'Added aut.AutomationDataSections.IsFromModel';
END
GO

/*----------------------------------------------------------------------------
  2. Model section sync
----------------------------------------------------------------------------*/

-- Inserts any section of the model that doesn't exist yet for the flow (flagged
-- IsFromModel = 1) and flags existing same-named ones. Returns every section of
-- the flow so the caller can map names to SectionIDs.
-- @SectionNames: JSON array of strings, e.g. ["Recipient","Diagnoses"].
--
-- Called on every Test Data Management load, often several times in parallel (the
-- Portal loads data and copy sources at once). Fixed: it used UPDLOCK/HOLDLOCK on
-- aut.AutomationDataSections, which has no index, so concurrent calls took
-- table-wide range locks and deadlocked (error 1205 -> HTTP 500). Now:
--   - fast path: nothing missing/unflagged -> no transaction, no locks, just read;
--   - otherwise one caller per flow at a time via an application lock, re-checking
--     inside it, so parallel first loads can't deadlock or insert duplicates.
CREATE OR ALTER PROCEDURE aut.usp_UpsertModelSections
    @FlowName NVARCHAR(250),
    @SectionNames NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Names TABLE (SectionName NVARCHAR(250) PRIMARY KEY);
    INSERT INTO @Names (SectionName)
    SELECT DISTINCT value FROM OPENJSON(@SectionNames);

    IF EXISTS (
        SELECT 1 FROM @Names N
        WHERE NOT EXISTS (
            SELECT 1 FROM aut.AutomationDataSections S
            WHERE S.FlowName = @FlowName AND S.SectionName = N.SectionName AND S.IsFromModel = 1)
    )
    BEGIN
        BEGIN TRANSACTION;

        DECLARE @Resource NVARCHAR(255) = N'aut.ModelSections:' + LEFT(@FlowName, 200);
        EXEC sp_getapplock @Resource = @Resource, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 15000;

        UPDATE S SET IsFromModel = 1
        FROM aut.AutomationDataSections S
        JOIN @Names N ON N.SectionName = S.SectionName
        WHERE S.FlowName = @FlowName AND S.IsFromModel = 0;

        INSERT INTO aut.AutomationDataSections (SectionName, FlowName, IsFromModel)
        SELECT N.SectionName, @FlowName, 1
        FROM @Names N
        WHERE NOT EXISTS (
            SELECT 1 FROM aut.AutomationDataSections S
            WHERE S.FlowName = @FlowName AND S.SectionName = N.SectionName
        );

        COMMIT TRANSACTION;
    END

    SELECT SectionID, SectionName, FlowName, IsFromModel
    FROM aut.AutomationDataSections
    WHERE FlowName = @FlowName;
END
GO

/*----------------------------------------------------------------------------
  3. Assignment lookups
----------------------------------------------------------------------------*/

-- Test cases assigned to a user in an environment (Test Data Management's Test
-- Case dropdown), one entry per LibraryName/ClassName/MethodName - the same test
-- case can be in several assignments; the most recent assignment wins.
CREATE OR ALTER PROCEDURE aut.usp_TestDataGetMyTestCases
    @UserId INT,
    @EnvironmentId INT
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Ranked AS (
        SELECT
            ATC.AssignmentTestCaseId, ATC.LibraryName, ATC.ClassName, ATC.MethodName,
            ATC.TestCaseId, ATC.TestCaseDescription, TCA.ReleaseId,
            ROW_NUMBER() OVER (PARTITION BY ATC.LibraryName, ATC.ClassName, ATC.MethodName
                               ORDER BY ATC.AssignmentTestCaseId DESC) AS rn
        FROM aut.AssignedTestCases ATC
        JOIN aut.TestCaseAssignment TCA ON TCA.AssignmentId = ATC.AssignmentId
        WHERE TCA.AssignedUser = @UserId
          AND TCA.EnvironmentId = @EnvironmentId
          AND ATC.LibraryName IS NOT NULL AND ATC.ClassName IS NOT NULL AND ATC.MethodName IS NOT NULL
    )
    SELECT AssignmentTestCaseId, LibraryName, ClassName, MethodName, TestCaseId, TestCaseDescription, ReleaseId
    FROM Ranked
    WHERE rn = 1
    ORDER BY TestCaseId, ClassName, MethodName;
END
GO

-- Everything the API needs to know about one assigned test case: its identity, who
-- it's assigned to, its environment and the Release folder holding its DLLs.
CREATE OR ALTER PROCEDURE aut.usp_TestDataResolveAssignment
    @AssignmentTestCaseId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ATC.AssignmentTestCaseId, ATC.LibraryName, ATC.ClassName, ATC.MethodName,
        ATC.TestCaseId, TCA.AssignedUser, TCA.EnvironmentId, TCA.ReleaseId, R.ReleaseFolderPath
    FROM aut.AssignedTestCases ATC
    JOIN aut.TestCaseAssignment TCA ON TCA.AssignmentId = ATC.AssignmentId
    LEFT JOIN aut.[Release] R ON R.ReleaseId = TCA.ReleaseId
    WHERE ATC.AssignmentTestCaseId = @AssignmentTestCaseId;
END
GO

PRINT '== TestDataModel migration completed ==';
GO
