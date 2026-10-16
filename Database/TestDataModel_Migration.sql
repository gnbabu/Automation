SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF COL_LENGTH('aut.AutomationDataSections', 'IsFromModel') IS NULL
BEGIN
    ALTER TABLE aut.AutomationDataSections
        ADD IsFromModel BIT NOT NULL CONSTRAINT DF_AutomationDataSections_IsFromModel DEFAULT 0;
    PRINT 'Added aut.AutomationDataSections.IsFromModel';
END
GO

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
