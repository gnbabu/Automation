/*============================================================================
  Automation Test Data - scope by Environment
  Target database: MES_AUT_AI   Schema: aut

  Purpose:
    aut.AutomationData (per-user Flow/Section test content) previously had no
    Environment dimension - one value shared across every environment. Adds
    EnvironmentId so a tester can maintain different content per environment
    (e.g. different DEV/QA/UAT values) for the same Flow/Section.

  Principles:
    - IDEMPOTENT: guarded DDL + CREATE OR ALTER procedures, safe to re-run.
    - The one-time backfill (existing rows -> QA) and NOT NULL tightening are
      guarded so re-running this script is a no-op once already applied.

  Note: This is unrelated to Release Management (no Release/Library/TestCase
  concept here) - Environment is the only new scoping dimension.
============================================================================*/

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/*----------------------------------------------------------------------------
  1. Schema: EnvironmentId column + FK
----------------------------------------------------------------------------*/
IF COL_LENGTH('aut.AutomationData', 'EnvironmentId') IS NULL
BEGIN
    ALTER TABLE aut.[AutomationData] ADD [EnvironmentId] INT NULL;
    PRINT 'Added aut.AutomationData.EnvironmentId';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AutomationData_Environment')
BEGIN
    ALTER TABLE aut.[AutomationData] WITH CHECK
        ADD CONSTRAINT [FK_AutomationData_Environment]
        FOREIGN KEY ([EnvironmentId]) REFERENCES aut.[Environment] ([EnvironmentId]);
    PRINT 'Added FK_AutomationData_Environment';
END
GO

/*----------------------------------------------------------------------------
  2. One-time backfill: existing (pre-Environment) rows -> QA, then tighten
     to NOT NULL. Guarded so this is a safe no-op once already applied.
----------------------------------------------------------------------------*/
IF EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'aut' AND TABLE_NAME = 'AutomationData'
      AND COLUMN_NAME = 'EnvironmentId' AND IS_NULLABLE = 'YES'
)
BEGIN
    UPDATE aut.AutomationData
    SET EnvironmentId = (SELECT EnvironmentId FROM aut.[Environment] WHERE EnvironmentName = 'QA')
    WHERE EnvironmentId IS NULL;
    PRINT 'Backfilled aut.AutomationData.EnvironmentId (legacy rows -> QA)';

    IF NOT EXISTS (SELECT 1 FROM aut.AutomationData WHERE EnvironmentId IS NULL)
    BEGIN
        ALTER TABLE aut.[AutomationData] ALTER COLUMN [EnvironmentId] INT NOT NULL;
        PRINT 'AutomationData.EnvironmentId altered to NOT NULL';
    END
    ELSE
        PRINT 'Skipped EnvironmentId NOT NULL: NULL rows still present';
END
GO

/*----------------------------------------------------------------------------
  3. Stored procedures
----------------------------------------------------------------------------*/

-- usp_GetAutomationData - now also filters by EnvironmentId
CREATE OR ALTER PROCEDURE [aut].[usp_GetAutomationData]
    @SectionID INT,
    @UserId INT,
    @EnvironmentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [ID],
        [SectionID],
        [TestContent],
        [UserID],
        [EnvironmentId]
    FROM [aut].[AutomationData] WITH (NOLOCK)
    WHERE SectionID = @SectionID
      AND UserID = @UserId
      AND EnvironmentId = @EnvironmentId;
END
GO

-- usp_InsertAutomationData - now stores EnvironmentId
CREATE OR ALTER PROCEDURE [aut].[usp_InsertAutomationData]
(
    @SectionID INT = NULL,
    @TestContent NVARCHAR(MAX) = NULL,
    @UserID INT = NULL,
    @EnvironmentId INT = NULL
)
AS
BEGIN TRANSACTION

BEGIN TRY
    INSERT INTO [aut].[AutomationData] (
        [SectionID],
        [TestContent],
        [UserID],
        [EnvironmentId]
    )
    VALUES (
        @SectionID,
        @TestContent,
        @UserID,
        @EnvironmentId
    );

    SELECT SCOPE_IDENTITY();

    COMMIT TRANSACTION
END TRY

BEGIN CATCH
    DECLARE @ErrorMessage NVARCHAR(4000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    SELECT
        @ErrorMessage = ERROR_MESSAGE(),
        @ErrorSeverity = ERROR_SEVERITY(),
        @ErrorState = ERROR_STATE();

    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);

    ROLLBACK TRANSACTION
END CATCH;
GO

PRINT '== AutomationData Environment migration completed ==';
GO
