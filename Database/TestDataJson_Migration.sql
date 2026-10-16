/*============================================================================
  Test case data as ONE JSON document per test case x user x environment
  Target database: MES_AUT_AI   Schema: aut
  Run after: TestDataModel_Migration.sql

  Purpose:
    TestDataModel_Migration.sql stored model-based test data as one row per
    section (test case x section x user x environment). This stores the whole
    test case's data as a single JSON document shaped exactly like the C# model,
    so a test run deserializes it straight onto the model:

      { "ProviderHeaderInfomration": { "ProviderMedicaidId": "00059871", ... },
        "Diagnoses": [ { "Sequence": 1, "DiagnosisCode": "Z00.00" }, ... ],
        "ProviderNotes": "..." }

    - class-typed properties  -> JSON objects
    - List<T> properties      -> JSON arrays
    - the model's own scalars -> top-level values
    A section that isn't in the document hasn't been saved for the test case;
    test runs keep the model's default (property-initializer) values for it.

    Version (rowversion) gives optimistic concurrency: a save carries the version
    it was based on and is rejected if the document changed in between.

    aut.TestCaseKey gives each test case (LibraryName + ClassName + MethodName) a
    narrow integer id, so the document table's unique key is three ints.

  Existing per-section rows are converted, and the old table is kept as
  aut.TestCaseData_SectionRows_Backup (safe to drop once verified).

  Principles: IDEMPOTENT - guarded DDL + CREATE OR ALTER procedures.
============================================================================*/

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/*----------------------------------------------------------------------------
  1. aut.TestCaseKey
----------------------------------------------------------------------------*/
IF OBJECT_ID('aut.TestCaseKey', 'U') IS NULL
BEGIN
    CREATE TABLE aut.TestCaseKey
    (
        TestCaseKeyId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TestCaseKey PRIMARY KEY,
        LibraryName   NVARCHAR(255) NOT NULL,
        ClassName     NVARCHAR(255) NOT NULL,
        MethodName    NVARCHAR(255) NOT NULL,
        CreatedOn     DATETIME2 NOT NULL CONSTRAINT DF_TestCaseKey_CreatedOn DEFAULT SYSDATETIME(),
        CONSTRAINT UX_TestCaseKey UNIQUE (LibraryName, ClassName, MethodName)
    );
    PRINT 'Created aut.TestCaseKey';
END
GO

/*----------------------------------------------------------------------------
  2. Move the per-section table aside (first run only)
----------------------------------------------------------------------------*/
IF COL_LENGTH('aut.TestCaseData', 'SectionID') IS NOT NULL
   AND OBJECT_ID('aut.TestCaseData_SectionRows_Backup', 'U') IS NULL
BEGIN
    EXEC sp_rename 'aut.TestCaseData', 'TestCaseData_SectionRows_Backup';
    EXEC sp_rename 'aut.PK_TestCaseData', 'PK_TestCaseData_SectionRows_Backup', 'OBJECT';
    EXEC sp_rename 'aut.FK_TestCaseData_User', 'FK_TestCaseData_SectionRows_Backup_User', 'OBJECT';
    EXEC sp_rename 'aut.FK_TestCaseData_Environment', 'FK_TestCaseData_SectionRows_Backup_Environment', 'OBJECT';
    EXEC sp_rename 'aut.DF_TestCaseData_CreatedOn', 'DF_TestCaseData_SectionRows_Backup_CreatedOn', 'OBJECT';
    PRINT 'Renamed per-section aut.TestCaseData to aut.TestCaseData_SectionRows_Backup';
END
GO

/*----------------------------------------------------------------------------
  3. aut.TestCaseData (one JSON document per test case x user x environment)
----------------------------------------------------------------------------*/
IF OBJECT_ID('aut.TestCaseData', 'U') IS NULL
BEGIN
    CREATE TABLE aut.TestCaseData
    (
        TestCaseDataId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TestCaseData PRIMARY KEY,
        TestCaseKeyId  INT NOT NULL CONSTRAINT FK_TestCaseData_TestCaseKey REFERENCES aut.TestCaseKey(TestCaseKeyId),
        UserID         INT NOT NULL CONSTRAINT FK_TestCaseData_User REFERENCES aut.[User](UserID),
        EnvironmentId  INT NOT NULL CONSTRAINT FK_TestCaseData_Environment REFERENCES aut.[Environment](EnvironmentId),
        -- Model class name (the flow) the document was saved for - used to offer
        -- "copy from another test case" only between test cases of the same model.
        FlowName       NVARCHAR(250) NOT NULL,
        DataJson       NVARCHAR(MAX) NOT NULL CONSTRAINT CK_TestCaseData_DataJson CHECK (ISJSON(DataJson) = 1),
        [Version]      ROWVERSION NOT NULL,
        CreatedOn      DATETIME2 NOT NULL CONSTRAINT DF_TestCaseData_CreatedOn DEFAULT SYSDATETIME(),
        ModifiedOn     DATETIME2 NULL,
        CONSTRAINT UX_TestCaseData UNIQUE (TestCaseKeyId, UserID, EnvironmentId)
    );
    PRINT 'Created aut.TestCaseData (JSON document per test case)';
END
GO

/*----------------------------------------------------------------------------
  4. Convert the old per-section rows (only when the new table is still empty)
     Values come across as JSON strings - exactly what the Portal saved; empty
     values are left out. Multi-row sections (items carrying RowIndex) become
     arrays; a section named after the flow holds the model's own scalars and is
     flattened to the top level.
----------------------------------------------------------------------------*/
IF OBJECT_ID('aut.TestCaseData_SectionRows_Backup', 'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM aut.TestCaseData)
BEGIN
    INSERT INTO aut.TestCaseKey (LibraryName, ClassName, MethodName)
    SELECT DISTINCT B.LibraryName, B.ClassName, B.MethodName
    FROM aut.TestCaseData_SectionRows_Backup B
    WHERE NOT EXISTS (
        SELECT 1 FROM aut.TestCaseKey K
        WHERE K.LibraryName = B.LibraryName AND K.ClassName = B.ClassName AND K.MethodName = B.MethodName);

    ;WITH Items AS (
        SELECT B.LibraryName, B.ClassName, B.MethodName, B.UserID, B.EnvironmentId,
               S.FlowName, S.SectionName,
               J.RowIndex, J.FieldName, J.FieldValue
        FROM aut.TestCaseData_SectionRows_Backup B
        JOIN aut.AutomationDataSections S ON S.SectionID = B.SectionID
        CROSS APPLY OPENJSON(B.TestContent)
            WITH (RowIndex INT '$.RowIndex', FieldName NVARCHAR(255) '$.FieldName', FieldValue NVARCHAR(MAX) '$.FieldValue') J
        WHERE ISJSON(B.TestContent) = 1 AND ISNULL(J.FieldValue, '') <> ''
    ),
    Pairs AS (
        SELECT *, '"' + STRING_ESCAPE(FieldName, 'json') + '":"' + STRING_ESCAPE(FieldValue, 'json') + '"' AS Pair
        FROM Items
    ),
    RowObjects AS (       -- one JSON object per section row
        SELECT LibraryName, ClassName, MethodName, UserID, EnvironmentId, FlowName, SectionName, RowIndex,
               '{' + STRING_AGG(Pair, ',') + '}' AS RowJson
        FROM Pairs
        WHERE SectionName <> FlowName
        GROUP BY LibraryName, ClassName, MethodName, UserID, EnvironmentId, FlowName, SectionName, RowIndex
    ),
    SectionJson AS (      -- object for single-row sections, array for multi-row
        SELECT LibraryName, ClassName, MethodName, UserID, EnvironmentId, FlowName,
               '"' + STRING_ESCAPE(SectionName, 'json') + '":' +
               CASE WHEN MAX(RowIndex) IS NULL THEN MAX(RowJson)
                    ELSE '[' + STRING_AGG(RowJson, ',') WITHIN GROUP (ORDER BY RowIndex) + ']' END AS Pair
        FROM RowObjects
        GROUP BY LibraryName, ClassName, MethodName, UserID, EnvironmentId, FlowName, SectionName
    ),
    AllPairs AS (
        SELECT LibraryName, ClassName, MethodName, UserID, EnvironmentId, FlowName, Pair FROM SectionJson
        UNION ALL
        SELECT LibraryName, ClassName, MethodName, UserID, EnvironmentId, FlowName, Pair FROM Pairs WHERE SectionName = FlowName
    )
    INSERT INTO aut.TestCaseData (TestCaseKeyId, UserID, EnvironmentId, FlowName, DataJson)
    SELECT K.TestCaseKeyId, A.UserID, A.EnvironmentId, A.FlowName, '{' + STRING_AGG(A.Pair, ',') + '}'
    FROM AllPairs A
    JOIN aut.TestCaseKey K ON K.LibraryName = A.LibraryName AND K.ClassName = A.ClassName AND K.MethodName = A.MethodName
    GROUP BY K.TestCaseKeyId, A.UserID, A.EnvironmentId, A.FlowName;

    PRINT 'Converted per-section rows: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' document(s)';
END
GO

/*----------------------------------------------------------------------------
  5. Procedures
----------------------------------------------------------------------------*/
DROP PROCEDURE IF EXISTS aut.usp_TestCaseDataGetForFlow;
GO

CREATE OR ALTER PROCEDURE aut.usp_TestCaseDataGet
    @LibraryName NVARCHAR(255),
    @ClassName NVARCHAR(255),
    @MethodName NVARCHAR(255),
    @UserId INT,
    @EnvironmentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TD.TestCaseDataId, TD.FlowName, TD.DataJson,
           CAST(TD.[Version] AS BIGINT) AS [Version],
           COALESCE(TD.ModifiedOn, TD.CreatedOn) AS ModifiedOn
    FROM aut.TestCaseData TD
    JOIN aut.TestCaseKey K ON K.TestCaseKeyId = TD.TestCaseKeyId
    WHERE K.LibraryName = @LibraryName AND K.ClassName = @ClassName AND K.MethodName = @MethodName
      AND TD.UserID = @UserId AND TD.EnvironmentId = @EnvironmentId;
END
GO

-- Writes the whole document. @ExpectedVersion is the version the caller read
-- (NULL = the caller saw no document yet). Returns IsConflict = 1 without writing
-- when the stored document is not the one the caller based its change on.
CREATE OR ALTER PROCEDURE aut.usp_TestCaseDataSave
    @LibraryName NVARCHAR(255),
    @ClassName NVARCHAR(255),
    @MethodName NVARCHAR(255),
    @UserId INT,
    @EnvironmentId INT,
    @FlowName NVARCHAR(250),
    @DataJson NVARCHAR(MAX),
    @ExpectedVersion BIGINT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @KeyId INT;
    SELECT @KeyId = TestCaseKeyId FROM aut.TestCaseKey WITH (UPDLOCK, HOLDLOCK)
    WHERE LibraryName = @LibraryName AND ClassName = @ClassName AND MethodName = @MethodName;

    IF @KeyId IS NULL
    BEGIN
        INSERT INTO aut.TestCaseKey (LibraryName, ClassName, MethodName) VALUES (@LibraryName, @ClassName, @MethodName);
        SET @KeyId = SCOPE_IDENTITY();
    END

    DECLARE @Id INT, @CurrentVersion BIGINT;
    SELECT @Id = TestCaseDataId, @CurrentVersion = CAST([Version] AS BIGINT)
    FROM aut.TestCaseData WITH (UPDLOCK, HOLDLOCK)
    WHERE TestCaseKeyId = @KeyId AND UserID = @UserId AND EnvironmentId = @EnvironmentId;

    IF @Id IS NULL
    BEGIN
        IF @ExpectedVersion IS NOT NULL
        BEGIN
            COMMIT TRANSACTION;
            SELECT CAST(NULL AS INT) AS TestCaseDataId, CAST(0 AS BIT) AS IsNew, CAST(1 AS BIT) AS IsConflict, CAST(NULL AS BIGINT) AS [Version];
            RETURN;
        END

        INSERT INTO aut.TestCaseData (TestCaseKeyId, UserID, EnvironmentId, FlowName, DataJson)
        VALUES (@KeyId, @UserId, @EnvironmentId, @FlowName, @DataJson);
        SET @Id = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
        SELECT @Id AS TestCaseDataId, CAST(1 AS BIT) AS IsNew, CAST(0 AS BIT) AS IsConflict,
               (SELECT CAST([Version] AS BIGINT) FROM aut.TestCaseData WHERE TestCaseDataId = @Id) AS [Version];
        RETURN;
    END

    IF @ExpectedVersion IS NULL OR @ExpectedVersion <> @CurrentVersion
    BEGIN
        COMMIT TRANSACTION;
        SELECT @Id AS TestCaseDataId, CAST(0 AS BIT) AS IsNew, CAST(1 AS BIT) AS IsConflict, @CurrentVersion AS [Version];
        RETURN;
    END

    UPDATE aut.TestCaseData
    SET DataJson = @DataJson, FlowName = @FlowName, ModifiedOn = SYSDATETIME()
    WHERE TestCaseDataId = @Id;

    COMMIT TRANSACTION;
    SELECT @Id AS TestCaseDataId, CAST(0 AS BIT) AS IsNew, CAST(0 AS BIT) AS IsConflict,
           (SELECT CAST([Version] AS BIGINT) FROM aut.TestCaseData WHERE TestCaseDataId = @Id) AS [Version];
END
GO

-- Other test cases of the same model (same user + environment) that have data -
-- the "Copy from another test case" list.
CREATE OR ALTER PROCEDURE aut.usp_TestCaseDataGetCopySources
    @LibraryName NVARCHAR(255),
    @ClassName NVARCHAR(255),
    @MethodName NVARCHAR(255),
    @FlowName NVARCHAR(250),
    @UserId INT,
    @EnvironmentId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT K.LibraryName, K.ClassName, K.MethodName,
        (SELECT TOP 1 ATC.TestCaseId FROM aut.AssignedTestCases ATC
         WHERE ATC.LibraryName = K.LibraryName AND ATC.ClassName = K.ClassName AND ATC.MethodName = K.MethodName
         ORDER BY ATC.AssignmentTestCaseId DESC) AS TestCaseId
    FROM aut.TestCaseData TD
    JOIN aut.TestCaseKey K ON K.TestCaseKeyId = TD.TestCaseKeyId
    WHERE TD.FlowName = @FlowName AND TD.UserID = @UserId AND TD.EnvironmentId = @EnvironmentId
      AND NOT (K.LibraryName = @LibraryName AND K.ClassName = @ClassName AND K.MethodName = @MethodName);
END
GO

PRINT '== TestDataJson migration completed ==';
GO
