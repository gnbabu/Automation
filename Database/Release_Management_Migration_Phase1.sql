/*============================================================================
  OHPNM Release Management - Phase 1: Database Migration
  Target database: MES_AUT_AI   Schema: aut

  Purpose:
    Make aut.Release the ROOT BUSINESS CONTEXT while preserving all existing
    automation functionality (discovery, assignment, execution, logs,
    screenshots, results).

  Principles:
    - NON-DESTRUCTIVE: no DROP TABLE, no column drops, no data loss.
    - IDEMPOTENT: guarded DDL, safe to re-run.
    - New FK columns are added NULLable and back-filled.
    - Existing text columns (ReleaseName / Environment) are RETAINED for
      backward compatibility with current code / stored procedures.

  Run order is top-to-bottom. See Release_Management_Migration_Notes.md.
============================================================================*/

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

PRINT '== Phase 1 migration starting ==';
GO

/*============================================================================
  1. ALTER aut.Release  (add columns, keep existing)
============================================================================*/

IF COL_LENGTH('aut.Release', 'Version') IS NULL
BEGIN
    ALTER TABLE aut.[Release] ADD [Version] NVARCHAR(50) NULL;
    PRINT 'Added aut.Release.Version';
END
GO

IF COL_LENGTH('aut.Release', 'EnvironmentId') IS NULL
BEGIN
    ALTER TABLE aut.[Release] ADD [EnvironmentId] INT NULL;
    PRINT 'Added aut.Release.EnvironmentId';
END
GO

IF COL_LENGTH('aut.Release', 'ReleaseFolderPath') IS NULL
BEGIN
    ALTER TABLE aut.[Release] ADD [ReleaseFolderPath] NVARCHAR(500) NULL;
    PRINT 'Added aut.Release.ReleaseFolderPath';
END
GO

IF COL_LENGTH('aut.Release', 'ModifiedBy') IS NULL
BEGIN
    ALTER TABLE aut.[Release] ADD [ModifiedBy] NVARCHAR(100) NULL;
    PRINT 'Added aut.Release.ModifiedBy';
END
GO

IF COL_LENGTH('aut.Release', 'ActivatedBy') IS NULL
BEGIN
    ALTER TABLE aut.[Release] ADD [ActivatedBy] NVARCHAR(100) NULL;
    PRINT 'Added aut.Release.ActivatedBy';
END
GO

IF COL_LENGTH('aut.Release', 'ActivatedOn') IS NULL
BEGIN
    ALTER TABLE aut.[Release] ADD [ActivatedOn] DATETIME2(7) NULL;
    PRINT 'Added aut.Release.ActivatedOn';
END
GO

-- Widen ReleaseLifecycle to fit states like 'ReadyForSignOff' (was NVARCHAR(20))
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('aut.Release')
      AND name = 'ReleaseLifecycle'
      AND max_length < 60   -- nvarchar(30) => 60 bytes
)
BEGIN
    ALTER TABLE aut.[Release] ALTER COLUMN [ReleaseLifecycle] NVARCHAR(30) NOT NULL;
    PRINT 'Widened aut.Release.ReleaseLifecycle to NVARCHAR(30)';
END
GO

-- FK: Release.EnvironmentId -> Environment.EnvironmentId
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Release_Environment')
BEGIN
    ALTER TABLE aut.[Release] WITH CHECK
        ADD CONSTRAINT [FK_Release_Environment]
        FOREIGN KEY ([EnvironmentId]) REFERENCES aut.[Environment] ([EnvironmentId]);
    PRINT 'Added FK_Release_Environment';
END
GO

-- Index on EnvironmentId
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Release_EnvironmentId' AND object_id = OBJECT_ID('aut.Release'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Release_EnvironmentId] ON aut.[Release] ([EnvironmentId]);
    PRINT 'Added IX_Release_EnvironmentId';
END
GO

-- Business uniqueness: ReleaseName + Version + EnvironmentId (only when all present)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Release_Name_Version_Env' AND object_id = OBJECT_ID('aut.Release'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Release_Name_Version_Env]
        ON aut.[Release] ([ReleaseName], [Version], [EnvironmentId])
        WHERE [Version] IS NOT NULL AND [EnvironmentId] IS NOT NULL;
    PRINT 'Added UX_Release_Name_Version_Env (filtered unique)';
END
GO

/*============================================================================
  2. CREATE aut.ReleaseDLL  (required DLLs defined manually per release)
============================================================================*/

IF OBJECT_ID('aut.ReleaseDLL', 'U') IS NULL
BEGIN
    CREATE TABLE aut.[ReleaseDLL](
        [ReleaseDLLId]      INT IDENTITY(1,1) NOT NULL,
        [ReleaseId]         INT NOT NULL,
        [DLLName]           NVARCHAR(255) NOT NULL,
        [Required]          BIT NOT NULL CONSTRAINT [DF_ReleaseDLL_Required] DEFAULT (1),
        [ExpectedVersion]   NVARCHAR(50) NULL,
        [UploadedVersion]   NVARCHAR(50) NULL,
        [DLLPath]           NVARCHAR(500) NULL,
        [FileSize]          BIGINT NULL,
        [UploadedOn]        DATETIME2(7) NULL,
        [UploadedBy]        NVARCHAR(100) NULL,
        [ValidationStatus]  NVARCHAR(30) NOT NULL CONSTRAINT [DF_ReleaseDLL_ValidationStatus] DEFAULT ('Missing'),
        [ValidationMessage] NVARCHAR(500) NULL,
        [ValidatedOn]       DATETIME2(7) NULL,
        [IsActive]          BIT NOT NULL CONSTRAINT [DF_ReleaseDLL_IsActive] DEFAULT (1),
        CONSTRAINT [PK_ReleaseDLL] PRIMARY KEY CLUSTERED ([ReleaseDLLId] ASC),
        CONSTRAINT [FK_ReleaseDLL_Release] FOREIGN KEY ([ReleaseId])
            REFERENCES aut.[Release] ([ReleaseId]) ON DELETE CASCADE
    );
    CREATE UNIQUE NONCLUSTERED INDEX [UX_ReleaseDLL_Release_Name]
        ON aut.[ReleaseDLL] ([ReleaseId], [DLLName]);
    PRINT 'Created aut.ReleaseDLL';
END
GO

/*============================================================================
  3. CREATE aut.ReleaseNotification
============================================================================*/

IF OBJECT_ID('aut.ReleaseNotification', 'U') IS NULL
BEGIN
    CREATE TABLE aut.[ReleaseNotification](
        [ReleaseNotificationId] INT IDENTITY(1,1) NOT NULL,
        [ReleaseId]             INT NOT NULL,
        [NotificationType]      NVARCHAR(50) NOT NULL,
        [RecipientUserId]       INT NULL,
        [RecipientEmail]        NVARCHAR(255) NULL,
        [Status]                NVARCHAR(30) NOT NULL CONSTRAINT [DF_ReleaseNotification_Status] DEFAULT ('Pending'),
        [Message]               NVARCHAR(500) NULL,
        [CreatedOn]             DATETIME2(7) NOT NULL CONSTRAINT [DF_ReleaseNotification_CreatedOn] DEFAULT (SYSDATETIME()),
        [SentOn]                DATETIME2(7) NULL,
        CONSTRAINT [PK_ReleaseNotification] PRIMARY KEY CLUSTERED ([ReleaseNotificationId] ASC),
        CONSTRAINT [FK_ReleaseNotification_Release] FOREIGN KEY ([ReleaseId])
            REFERENCES aut.[Release] ([ReleaseId]) ON DELETE CASCADE,
        CONSTRAINT [FK_ReleaseNotification_User] FOREIGN KEY ([RecipientUserId])
            REFERENCES aut.[User] ([UserID])
    );
    CREATE NONCLUSTERED INDEX [IX_ReleaseNotification_ReleaseId]
        ON aut.[ReleaseNotification] ([ReleaseId]);
    PRINT 'Created aut.ReleaseNotification';
END
GO

/*============================================================================
  4. CREATE aut.ReleaseSignOff  (approve / reject / rework history)
============================================================================*/

IF OBJECT_ID('aut.ReleaseSignOff', 'U') IS NULL
BEGIN
    CREATE TABLE aut.[ReleaseSignOff](
        [ReleaseSignOffId] INT IDENTITY(1,1) NOT NULL,
        [ReleaseId]        INT NOT NULL,
        [SignOffStatus]    NVARCHAR(20) NOT NULL,   -- Pending / Approved / Rejected
        [SignOffBy]        NVARCHAR(100) NULL,
        [SignOffOn]        DATETIME2(7) NULL,
        [Comments]         NVARCHAR(1000) NULL,
        [CreatedOn]        DATETIME2(7) NOT NULL CONSTRAINT [DF_ReleaseSignOff_CreatedOn] DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_ReleaseSignOff] PRIMARY KEY CLUSTERED ([ReleaseSignOffId] ASC),
        CONSTRAINT [FK_ReleaseSignOff_Release] FOREIGN KEY ([ReleaseId])
            REFERENCES aut.[Release] ([ReleaseId]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_ReleaseSignOff_ReleaseId]
        ON aut.[ReleaseSignOff] ([ReleaseId]);
    PRINT 'Created aut.ReleaseSignOff';
END
GO

/*============================================================================
  5. ALTER aut.TestCaseAssignment  (Release-aware; keep text columns)
============================================================================*/

IF COL_LENGTH('aut.TestCaseAssignment', 'ReleaseId') IS NULL
BEGIN
    ALTER TABLE aut.[TestCaseAssignment] ADD [ReleaseId] INT NULL;
    PRINT 'Added aut.TestCaseAssignment.ReleaseId';
END
GO

IF COL_LENGTH('aut.TestCaseAssignment', 'EnvironmentId') IS NULL
BEGIN
    ALTER TABLE aut.[TestCaseAssignment] ADD [EnvironmentId] INT NULL;
    PRINT 'Added aut.TestCaseAssignment.EnvironmentId';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TestCaseAssignment_Release')
BEGIN
    ALTER TABLE aut.[TestCaseAssignment] WITH CHECK
        ADD CONSTRAINT [FK_TestCaseAssignment_Release]
        FOREIGN KEY ([ReleaseId]) REFERENCES aut.[Release] ([ReleaseId]);
    PRINT 'Added FK_TestCaseAssignment_Release';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TestCaseAssignment_Environment')
BEGIN
    ALTER TABLE aut.[TestCaseAssignment] WITH CHECK
        ADD CONSTRAINT [FK_TestCaseAssignment_Environment]
        FOREIGN KEY ([EnvironmentId]) REFERENCES aut.[Environment] ([EnvironmentId]);
    PRINT 'Added FK_TestCaseAssignment_Environment';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TestCaseAssignment_ReleaseId' AND object_id = OBJECT_ID('aut.TestCaseAssignment'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_TestCaseAssignment_ReleaseId]
        ON aut.[TestCaseAssignment] ([ReleaseId]);
    PRINT 'Added IX_TestCaseAssignment_ReleaseId';
END
GO

/*============================================================================
  6. DATA MIGRATION (non-destructive back-fill)
============================================================================*/

-- 6.1 Back-fill TestCaseAssignment.EnvironmentId by exact (trim/case-insensitive) name match.
UPDATE a
SET a.EnvironmentId = e.EnvironmentId
FROM aut.[TestCaseAssignment] a
JOIN aut.[Environment] e
     ON LTRIM(RTRIM(e.EnvironmentName)) = LTRIM(RTRIM(a.Environment))
WHERE a.EnvironmentId IS NULL
  AND a.Environment IS NOT NULL;
PRINT 'Back-filled TestCaseAssignment.EnvironmentId where a name match existed';
GO

-- 6.2 Existing Release rows: Version/EnvironmentId intentionally left NULL (no fabrication).
-- 6.3 Assignment -> Release: existing ReleaseName values are library names, NOT real
--     releases. ReleaseId is intentionally left NULL (no fabrication). See report below.

/*============================================================================
  6.4 MAPPING REPORT  (review these result sets after running)
============================================================================*/
PRINT '== Mapping report ==';

-- (a) Assignments whose Environment text has NO matching aut.Environment row
SELECT 'UnmatchedEnvironment' AS Issue,
       a.AssignmentId, a.AssignmentName, a.Environment AS EnvironmentText
FROM aut.[TestCaseAssignment] a
WHERE a.EnvironmentId IS NULL
  AND a.Environment IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM aut.[Environment] e
      WHERE LTRIM(RTRIM(e.EnvironmentName)) = LTRIM(RTRIM(a.Environment))
  );

-- (b) Assignments left with NULL ReleaseId (need manual linkage to a real Release)
SELECT 'UnlinkedAssignmentRelease' AS Issue,
       a.AssignmentId, a.AssignmentName, a.ReleaseName AS ReleaseNameText, a.Environment AS EnvironmentText
FROM aut.[TestCaseAssignment] a
WHERE a.ReleaseId IS NULL;

-- (c) Release rows with NULL EnvironmentId or NULL Version (need manual completion)
SELECT 'IncompleteRelease' AS Issue,
       r.ReleaseId, r.ReleaseName, r.[Version], r.EnvironmentId, r.ReleaseLifecycle
FROM aut.[Release] r
WHERE r.EnvironmentId IS NULL OR r.[Version] IS NULL;
GO

/*============================================================================
  7. STORED PROCEDURES  (create/alter to match the new schema)
============================================================================*/

-- 7.1 usp_CreateRelease  (adds Version, EnvironmentId, ReleaseFolderPath)
CREATE OR ALTER PROCEDURE aut.usp_CreateRelease
(
    @ReleaseName       NVARCHAR(100),
    @Version           NVARCHAR(50)  = NULL,
    @EnvironmentId     INT           = NULL,
    @Description       NVARCHAR(255) = NULL,
    @ReleaseFolderPath NVARCHAR(500) = NULL,
    @CreatedBy         NVARCHAR(100) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Uniqueness = ReleaseName + Version + EnvironmentId (business rule)
    IF EXISTS (
        SELECT 1 FROM aut.[Release]
        WHERE ReleaseName = @ReleaseName
          AND ISNULL([Version], N'') = ISNULL(@Version, N'')
          AND ISNULL(EnvironmentId, -1) = ISNULL(@EnvironmentId, -1)
    )
    BEGIN
        RAISERROR('A release with the same Name, Version and Environment already exists.', 16, 1);
        RETURN;
    END

    INSERT INTO aut.[Release]
    (
        ReleaseName, [Version], EnvironmentId, Description, ReleaseFolderPath,
        ReleaseLifecycle, IsActive, SignOffStatus, CreatedBy
    )
    VALUES
    (
        @ReleaseName, @Version, @EnvironmentId, @Description, @ReleaseFolderPath,
        'Draft', 1, 'Pending', @CreatedBy
    );

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS ReleaseId;
END
GO

-- 7.2 usp_UpdateRelease  (adds Version/EnvironmentId/ReleaseFolderPath/ModifiedBy)
CREATE OR ALTER PROCEDURE aut.usp_UpdateRelease
(
    @ReleaseId         INT,
    @ReleaseName       NVARCHAR(100),
    @Version           NVARCHAR(50)  = NULL,
    @EnvironmentId     INT           = NULL,
    @Description       NVARCHAR(255) = NULL,
    @ReleaseFolderPath NVARCHAR(500) = NULL,
    @ReleaseLifecycle  NVARCHAR(30)  = NULL,
    @IsActive          BIT           = NULL,
    @ModifiedBy        NVARCHAR(100) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE aut.[Release]
    SET
        ReleaseName       = @ReleaseName,
        [Version]         = @Version,
        EnvironmentId     = @EnvironmentId,
        Description       = @Description,
        ReleaseFolderPath = COALESCE(@ReleaseFolderPath, ReleaseFolderPath),
        ReleaseLifecycle  = COALESCE(@ReleaseLifecycle, ReleaseLifecycle),
        IsActive          = COALESCE(@IsActive, IsActive),
        ModifiedBy        = @ModifiedBy,
        ModifiedOn        = SYSDATETIME()
    WHERE ReleaseId = @ReleaseId;
END
GO

-- 7.3 usp_GetAllRelease  (join Environment, test summary)
-- NOTE: DLL readiness is filesystem state (DLLs are placed by the existing
-- controlled build/deploy process, not tracked in the database), so it is
-- computed by the application layer (IReleaseReadinessService), not here.
CREATE OR ALTER PROCEDURE aut.usp_GetAllRelease
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        r.ReleaseId,
        r.ReleaseName,
        r.[Version],
        r.EnvironmentId,
        e.EnvironmentName,
        r.Description,
        r.ReleaseFolderPath,
        r.ReleaseLifecycle,
        r.IsActive,
        r.SignOffStatus,
        r.SignedOffBy,
        r.SignedOffOn,
        r.CreatedBy,
        r.CreatedOn,
        r.ModifiedBy,
        r.ModifiedOn,
        r.ActivatedBy,
        r.ActivatedOn,
        -- Test summary
        ts.TotalTests,
        ts.PassedTests,
        ts.FailedTests,
        ts.SkippedTests,
        ts.RunningTests
    FROM aut.[Release] r
    LEFT JOIN aut.[Environment] e ON r.EnvironmentId = e.EnvironmentId
    OUTER APPLY (
        SELECT
            COUNT(*) AS TotalTests,
            SUM(CASE WHEN atc.TestCaseStatus = 'Passed'  THEN 1 ELSE 0 END) AS PassedTests,
            SUM(CASE WHEN atc.TestCaseStatus = 'Failed'  THEN 1 ELSE 0 END) AS FailedTests,
            SUM(CASE WHEN atc.TestCaseStatus = 'Skipped' THEN 1 ELSE 0 END) AS SkippedTests,
            SUM(CASE WHEN atc.TestCaseStatus NOT IN ('Passed','Failed','Skipped') THEN 1 ELSE 0 END) AS RunningTests
        FROM aut.[TestCaseAssignment] a
        JOIN aut.[AssignedTestCases] atc ON atc.AssignmentId = a.AssignmentId
        WHERE a.ReleaseId = r.ReleaseId
    ) ts
    ORDER BY r.CreatedOn DESC;
END
GO

-- 7.4 usp_GetReleaseById  (same shape as GetAll, single release)
CREATE OR ALTER PROCEDURE aut.usp_GetReleaseById
(
    @ReleaseId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        r.ReleaseId,
        r.ReleaseName,
        r.[Version],
        r.EnvironmentId,
        e.EnvironmentName,
        r.Description,
        r.ReleaseFolderPath,
        r.ReleaseLifecycle,
        r.IsActive,
        r.SignOffStatus,
        r.SignedOffBy,
        r.SignedOffOn,
        r.CreatedBy,
        r.CreatedOn,
        r.ModifiedBy,
        r.ModifiedOn,
        r.ActivatedBy,
        r.ActivatedOn,
        ts.TotalTests,
        ts.PassedTests,
        ts.FailedTests,
        ts.SkippedTests,
        ts.RunningTests
    FROM aut.[Release] r
    LEFT JOIN aut.[Environment] e ON r.EnvironmentId = e.EnvironmentId
    OUTER APPLY (
        SELECT
            COUNT(*) AS TotalTests,
            SUM(CASE WHEN atc.TestCaseStatus = 'Passed'  THEN 1 ELSE 0 END) AS PassedTests,
            SUM(CASE WHEN atc.TestCaseStatus = 'Failed'  THEN 1 ELSE 0 END) AS FailedTests,
            SUM(CASE WHEN atc.TestCaseStatus = 'Skipped' THEN 1 ELSE 0 END) AS SkippedTests,
            SUM(CASE WHEN atc.TestCaseStatus NOT IN ('Passed','Failed','Skipped') THEN 1 ELSE 0 END) AS RunningTests
        FROM aut.[TestCaseAssignment] a
        JOIN aut.[AssignedTestCases] atc ON atc.AssignmentId = a.AssignmentId
        WHERE a.ReleaseId = r.ReleaseId
    ) ts
    WHERE r.ReleaseId = @ReleaseId;
END
GO

-- 7.4b usp_Release_SetFolderPath  (store the resolved folder path after physical creation)
CREATE OR ALTER PROCEDURE aut.usp_Release_SetFolderPath
(
    @ReleaseId INT,
    @ReleaseFolderPath NVARCHAR(500)
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE aut.[Release]
    SET ReleaseFolderPath = @ReleaseFolderPath,
        ModifiedOn = SYSDATETIME()
    WHERE ReleaseId = @ReleaseId;
END
GO

-- 7.4c usp_DeleteRelease  (compensating delete; used ONLY to roll back a Release
-- row when the immediately-following physical folder creation fails, so a
-- failed Create never falsely reports success. Cascades to ReleaseDLL,
-- ReleaseNotification, ReleaseSignOff via existing FK ON DELETE CASCADE.)
CREATE OR ALTER PROCEDURE aut.usp_DeleteRelease
(
    @ReleaseId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM aut.[Release] WHERE ReleaseId = @ReleaseId;
END
GO

-- 7.5 usp_ActivateRelease  (guards: env active, folder present, required DLLs validated)
CREATE OR ALTER PROCEDURE aut.usp_ActivateRelease
(
    @ReleaseId    INT,
    @ActivatedBy  NVARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EnvironmentId INT, @FolderPath NVARCHAR(500), @EnvActive BIT;

    SELECT @EnvironmentId = EnvironmentId, @FolderPath = ReleaseFolderPath
    FROM aut.[Release] WHERE ReleaseId = @ReleaseId;

    IF @EnvironmentId IS NULL
    BEGIN
        RAISERROR('Cannot activate: release has no environment.', 16, 1); RETURN;
    END

    SELECT @EnvActive = IsActive FROM aut.[Environment] WHERE EnvironmentId = @EnvironmentId;
    IF ISNULL(@EnvActive, 0) = 0
    BEGIN
        RAISERROR('Cannot activate: environment is not active.', 16, 1); RETURN;
    END

    IF @FolderPath IS NULL OR LTRIM(RTRIM(@FolderPath)) = ''
    BEGIN
        RAISERROR('Cannot activate: release folder path is not set.', 16, 1); RETURN;
    END

    IF EXISTS (
        SELECT 1 FROM aut.[ReleaseDLL]
        WHERE ReleaseId = @ReleaseId AND IsActive = 1
          AND Required = 1 AND ValidationStatus <> 'Validated'
    )
    BEGIN
        RAISERROR('Cannot activate: one or more required DLLs are not validated.', 16, 1); RETURN;
    END

    UPDATE aut.[Release]
    SET ReleaseLifecycle = 'Active',
        IsActive         = 1,
        ActivatedBy      = @ActivatedBy,
        ActivatedOn      = SYSDATETIME(),
        ModifiedBy       = @ActivatedBy,
        ModifiedOn       = SYSDATETIME()
    WHERE ReleaseId = @ReleaseId;
END
GO

-- 7.6 usp_ReleaseSignOff  (only after all assigned tests are terminal; approve/reject + history)
CREATE OR ALTER PROCEDURE aut.usp_ReleaseSignOff
(
    @ReleaseId     INT,
    @SignOffStatus NVARCHAR(20),      -- 'Approved' or 'Rejected'
    @SignedOffBy   NVARCHAR(100),
    @Comments      NVARCHAR(1000) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @SignOffStatus NOT IN ('Approved', 'Rejected')
    BEGIN
        RAISERROR('SignOffStatus must be Approved or Rejected.', 16, 1); RETURN;
    END

    -- Testing must be complete: no non-terminal assigned test cases for this release
    IF EXISTS (
        SELECT 1
        FROM aut.[TestCaseAssignment] a
        JOIN aut.[AssignedTestCases] atc ON atc.AssignmentId = a.AssignmentId
        WHERE a.ReleaseId = @ReleaseId
          AND atc.TestCaseStatus NOT IN ('Passed','Failed','Skipped')
    )
    BEGIN
        RAISERROR('Cannot sign off: not all assigned tests have completed.', 16, 1); RETURN;
    END

    -- A release with zero assigned tests cannot be signed off either
    IF NOT EXISTS (
        SELECT 1
        FROM aut.[TestCaseAssignment] a
        JOIN aut.[AssignedTestCases] atc ON atc.AssignmentId = a.AssignmentId
        WHERE a.ReleaseId = @ReleaseId
    )
    BEGIN
        RAISERROR('Cannot sign off: the release has no completed tests to review.', 16, 1); RETURN;
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO aut.[ReleaseSignOff] (ReleaseId, SignOffStatus, SignOffBy, SignOffOn, Comments)
        VALUES (@ReleaseId, @SignOffStatus, @SignedOffBy, SYSDATETIME(), @Comments);

        UPDATE aut.[Release]
        SET SignOffStatus    = @SignOffStatus,
            SignedOffBy      = @SignedOffBy,
            SignedOffOn      = SYSDATETIME(),
            ReleaseLifecycle = CASE WHEN @SignOffStatus = 'Approved' THEN 'Completed' ELSE 'Rejected' END,
            ModifiedBy       = @SignedOffBy,
            ModifiedOn       = SYSDATETIME()
        WHERE ReleaseId = @ReleaseId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- 7.6b usp_ReleaseSignOff_GetByRelease  (sign-off history for a release)
CREATE OR ALTER PROCEDURE aut.usp_ReleaseSignOff_GetByRelease
(
    @ReleaseId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ReleaseSignOffId, ReleaseId, SignOffStatus, SignOffBy, SignOffOn, Comments, CreatedOn
    FROM aut.[ReleaseSignOff]
    WHERE ReleaseId = @ReleaseId
    ORDER BY CreatedOn DESC;
END
GO

-- 7.7 ReleaseDLL stored procedures
CREATE OR ALTER PROCEDURE aut.usp_ReleaseDLL_Add
(
    @ReleaseId       INT,
    @DLLName         NVARCHAR(255),
    @Required        BIT = 1,
    @ExpectedVersion NVARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM aut.[ReleaseDLL] WHERE ReleaseId = @ReleaseId AND DLLName = @DLLName)
    BEGIN
        RAISERROR('This DLL is already registered for the release.', 16, 1); RETURN;
    END

    INSERT INTO aut.[ReleaseDLL] (ReleaseId, DLLName, Required, ExpectedVersion, ValidationStatus)
    VALUES (@ReleaseId, @DLLName, @Required, @ExpectedVersion, 'Missing');

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS ReleaseDLLId;
END
GO

CREATE OR ALTER PROCEDURE aut.usp_ReleaseDLL_Update
(
    @ReleaseDLLId    INT,
    @DLLName         NVARCHAR(255),
    @Required        BIT,
    @ExpectedVersion NVARCHAR(50) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE aut.[ReleaseDLL]
    SET DLLName = @DLLName, Required = @Required, ExpectedVersion = @ExpectedVersion
    WHERE ReleaseDLLId = @ReleaseDLLId;
END
GO

CREATE OR ALTER PROCEDURE aut.usp_ReleaseDLL_GetByRelease
(
    @ReleaseId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ReleaseDLLId, ReleaseId, DLLName, Required, ExpectedVersion, UploadedVersion,
           DLLPath, FileSize, UploadedOn, UploadedBy, ValidationStatus, ValidationMessage,
           ValidatedOn, IsActive
    FROM aut.[ReleaseDLL]
    WHERE ReleaseId = @ReleaseId AND IsActive = 1
    ORDER BY DLLName;
END
GO

-- Called after a physical upload to record file info (and set status to Uploaded)
CREATE OR ALTER PROCEDURE aut.usp_ReleaseDLL_SetUpload
(
    @ReleaseDLLId    INT,
    @UploadedVersion NVARCHAR(50) = NULL,
    @DLLPath         NVARCHAR(500) = NULL,
    @FileSize        BIGINT = NULL,
    @UploadedBy      NVARCHAR(100) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE aut.[ReleaseDLL]
    SET UploadedVersion = @UploadedVersion,
        DLLPath         = @DLLPath,
        FileSize        = @FileSize,
        UploadedOn      = SYSDATETIME(),
        UploadedBy      = @UploadedBy,
        ValidationStatus = 'Uploaded'
    WHERE ReleaseDLLId = @ReleaseDLLId;
END
GO

CREATE OR ALTER PROCEDURE aut.usp_ReleaseDLL_SetValidation
(
    @ReleaseDLLId      INT,
    @ValidationStatus  NVARCHAR(30),      -- Validated / VersionMismatch / Invalid / Missing
    @ValidationMessage NVARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE aut.[ReleaseDLL]
    SET ValidationStatus  = @ValidationStatus,
        ValidationMessage = @ValidationMessage,
        ValidatedOn       = SYSDATETIME()
    WHERE ReleaseDLLId = @ReleaseDLLId;
END
GO

CREATE OR ALTER PROCEDURE aut.usp_ReleaseDLL_Delete
(
    @ReleaseDLLId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    -- Soft delete to preserve history
    UPDATE aut.[ReleaseDLL] SET IsActive = 0 WHERE ReleaseDLLId = @ReleaseDLLId;
END
GO

-- 7.8 ReleaseNotification stored procedures
CREATE OR ALTER PROCEDURE aut.usp_ReleaseNotification_Add
(
    @ReleaseId        INT,
    @NotificationType NVARCHAR(50),
    @RecipientUserId  INT = NULL,
    @RecipientEmail   NVARCHAR(255) = NULL,
    @Message          NVARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO aut.[ReleaseNotification]
        (ReleaseId, NotificationType, RecipientUserId, RecipientEmail, Status, Message)
    VALUES
        (@ReleaseId, @NotificationType, @RecipientUserId, @RecipientEmail, 'Pending', @Message);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS ReleaseNotificationId;
END
GO

CREATE OR ALTER PROCEDURE aut.usp_ReleaseNotification_GetByRelease
(
    @ReleaseId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ReleaseNotificationId, ReleaseId, NotificationType, RecipientUserId,
           RecipientEmail, Status, Message, CreatedOn, SentOn
    FROM aut.[ReleaseNotification]
    WHERE ReleaseId = @ReleaseId
    ORDER BY CreatedOn DESC;
END
GO

CREATE OR ALTER PROCEDURE aut.usp_ReleaseNotification_MarkSent
(
    @ReleaseNotificationId INT,
    @Status                NVARCHAR(30) = 'Sent'
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE aut.[ReleaseNotification]
    SET Status = @Status,
        SentOn = CASE WHEN @Status = 'Sent' THEN SYSDATETIME() ELSE SentOn END
    WHERE ReleaseNotificationId = @ReleaseNotificationId;
END
GO

-- 7.9 usp_CreateOrUpdateAssignmentWithTestCases  (add optional ReleaseId/EnvironmentId)
--     Signature keeps all existing params; adds two OPTIONAL trailing params so current
--     callers keep working. Persists new FK columns alongside existing text columns.
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

    BEGIN TRY
        SELECT @TesterName = UserName FROM aut.[User] WHERE UserID = @AssignedUser;

        SET @AssignmentName = @TesterName + '-' + @ReleaseName + '-' + @Environment;

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

-- 7.10 usp_GetTestCaseAssignmentsByUser  (additionally return ReleaseId/EnvironmentId)
CREATE OR ALTER PROCEDURE aut.usp_GetTestCaseAssignmentsByUser
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF (@UserId IS NULL OR @UserId <= 0)
    BEGIN
        RAISERROR('UserId is required and must be greater than zero.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        SELECT
            A.AssignmentId,
            A.AssignmentName,
            A.AssignmentStatus,
            A.AssignedUser,
            U.UserName AS AssignedUserName,
            A.ReleaseName,
            A.Environment,
            A.ReleaseId,
            A.EnvironmentId,
            A.AssignedDate,
            A.AssignedBy,
            UB.UserName AS AssignedByUserName,
            A.LastUpdatedDate
        FROM aut.TestCaseAssignment A
        INNER JOIN aut.[User] U  ON A.AssignedUser = U.UserID
        INNER JOIN aut.[User] UB ON A.AssignedBy   = UB.UserID
        WHERE A.AssignedUser = @UserId;
    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END
GO

-- 7.11 usp_GetReleaseExecutionLogs  (also expose ReleaseId; keep ReleaseName filter)
CREATE OR ALTER PROCEDURE aut.usp_GetReleaseExecutionLogs
    @ReleaseName NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.ReleaseName,
        a.ReleaseId,
        l.AssignmentId,
        l.AssignmentTestCaseId,
        tc.TestCaseId,
        tc.TestCaseDescription,
        l.StepName,
        l.LogMessage,
        l.LogLevel,
        l.ExecutionStatus,
        l.ErrorStackTrace,
        l.ScreenshotId,
        l.CreatedAt
    FROM aut.TestCaseExecutionLogs l
    JOIN aut.TestCaseAssignment a ON l.AssignmentId = a.AssignmentId
    JOIN aut.AssignedTestCases tc ON l.AssignmentTestCaseId = tc.AssignmentTestCaseId
    WHERE a.ReleaseName = @ReleaseName
    ORDER BY l.CreatedAt;
END
GO

PRINT '== Phase 1 migration completed ==';
GO
