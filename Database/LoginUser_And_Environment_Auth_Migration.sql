-- LoginUser_And_Environment_Auth_Migration.sql
-- Idempotent (safe to re-run). Implements:
--   1. aut.Environment gains EnvironmentUrl (nullable) and RequiresAuthentication
--      (defaults to 1/true - matches today's actual behavior for every existing
--      environment, so nothing regresses).
--   2. New aut.LoginUser table - per-environment login credentials, selected explicitly
--      by the person running/scheduling a test (not automatically matched). UserRole and
--      PortalUserId are informational labels only, not matching keys - free text UserRole
--      matches whatever string a test's own [TestFixture("...")] declares, and
--      PortalUserId (optional) just records whose credential a row represents.
--   3. New stored procedures: usp_LoginUserCreate/Update/SoftDelete,
--      usp_LoginUserGetByEnvironment (list, never returns the password),
--      usp_LoginUserGetCredentials (by id - the only procedure that ever returns the
--      encrypted password).
--   4. aut.TestCaseExecutionQueue gains a nullable LoginUserId - the specific login user
--      selected at Run Now/Schedule time (single or bulk - one shared selection for the
--      whole batch). usp_SingleRunTestCaseNow/usp_BulkRunTestCasesNow/
--      usp_ScheduleSingleTestCase/usp_BulkScheduleTestCases/
--      usp_GetPendingExecutionQueues updated accordingly.
--   5. Seed data: aut.LoginUser rows only for environments that already exist AND whose
--      name matches one of Selenium.BaseComponents.Data.UserCredentials.cs's hard-coded
--      dictionary keys - concretely just E2EP3 and PROD (its other keys - DEV01,
--      DEV01P3, INT01, INT01P3, E2E01P3 - don't correspond to any real aut.Environment
--      row and are deliberately skipped, not force-mapped to something else). No new
--      aut.Environment rows are created.

SET QUOTED_IDENTIFIER ON;
GO

-- 1. aut.Environment: EnvironmentUrl + RequiresAuthentication
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'aut' AND TABLE_NAME = 'Environment' AND COLUMN_NAME = 'EnvironmentUrl'
)
BEGIN
    ALTER TABLE aut.[Environment] ADD EnvironmentUrl NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'aut' AND TABLE_NAME = 'Environment' AND COLUMN_NAME = 'RequiresAuthentication'
)
BEGIN
    ALTER TABLE aut.[Environment] ADD RequiresAuthentication BIT NOT NULL DEFAULT 1;
END
GO

-- 1b. usp_EnvironmentCreate/Update/GetAll/GetById - accept/return EnvironmentUrl +
-- RequiresAuthentication. Bodies otherwise unchanged from their current live definitions.

CREATE OR ALTER PROCEDURE [aut].[usp_EnvironmentCreate]
(
    @EnvironmentName NVARCHAR(50),
    @Description NVARCHAR(255),
    @CreatedBy INT,
    @EnvironmentUrl NVARCHAR(500) = NULL,
    @RequiresAuthentication BIT = 1
)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM aut.Environment
        WHERE EnvironmentName = @EnvironmentName
    )
    BEGIN
        RAISERROR ('Environment already exists', 16, 1);
        RETURN;
    END

    INSERT INTO aut.Environment
    (
        EnvironmentName,
        Description,
        IsActive,
        CreatedBy,
        EnvironmentUrl,
        RequiresAuthentication
    )
    VALUES
    (
        @EnvironmentName,
        @Description,
        1,
        @CreatedBy,
        @EnvironmentUrl,
        @RequiresAuthentication
    );

    SELECT SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE [aut].[usp_EnvironmentUpdate]
(
    @EnvironmentId INT,
    @EnvironmentName NVARCHAR(50),
    @Description NVARCHAR(255),
    @IsActive BIT,
    @ModifiedBy INT = NULL,
    @EnvironmentUrl NVARCHAR(500) = NULL,
    @RequiresAuthentication BIT = 1
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE aut.Environment
    SET
        EnvironmentName = @EnvironmentName,
        Description = @Description,
        IsActive = @IsActive,
        ModifiedBy = @ModifiedBy,
        ModifiedOn = SYSDATETIME(),
        EnvironmentUrl = @EnvironmentUrl,
        RequiresAuthentication = @RequiresAuthentication
    WHERE EnvironmentId = @EnvironmentId;
END
GO

CREATE OR ALTER PROCEDURE [aut].[usp_EnvironmentGetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.EnvironmentId,
        e.EnvironmentName,
        e.Description,
        e.IsActive,
        e.CreatedOn,
        e.EnvironmentUrl,
        e.RequiresAuthentication,

        u.UserID,
        u.UserName,
        u.Email,

        mu.UserName AS ModifiedByName,

        (SELECT COUNT(*) FROM aut.[Release] r WHERE r.EnvironmentId = e.EnvironmentId) AS ReleaseCount

    FROM aut.Environment e
    JOIN aut.[User] u ON e.CreatedBy = u.UserID
    LEFT JOIN aut.[User] mu ON e.ModifiedBy = mu.UserID
    ORDER BY e.CreatedOn DESC;
END
GO

CREATE OR ALTER PROCEDURE [aut].[usp_EnvironmentGetById]
(
    @EnvironmentId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.*,
        u.UserName,
        u.Email,
        mu.UserName AS ModifiedByName,
        (SELECT COUNT(*) FROM aut.[Release] r WHERE r.EnvironmentId = e.EnvironmentId) AS ReleaseCount
    FROM aut.Environment e
    JOIN aut.[User] u ON e.CreatedBy = u.UserID
    LEFT JOIN aut.[User] mu ON e.ModifiedBy = mu.UserID
    WHERE e.EnvironmentId = @EnvironmentId;
END
GO

-- 2. aut.LoginUser
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'aut.LoginUser') AND type = 'U')
BEGIN
    CREATE TABLE aut.LoginUser
    (
        LoginUserId INT IDENTITY PRIMARY KEY,
        EnvironmentId INT NOT NULL,
        PortalUserId INT NULL,        -- optional label: "whose credential is this" (aut.User)
        UserRole NVARCHAR(50) NOT NULL, -- free text label, e.g. "TechAdmin", "CredSpec" - matches [TestFixture("...")]
        UserName NVARCHAR(100) NOT NULL,
        EncryptedPassword NVARCHAR(500) NOT NULL, -- ciphertext, never plaintext at rest
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedBy INT NULL,
        CreatedOn DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        ModifiedBy INT NULL,
        ModifiedOn DATETIME2 NULL,
        CONSTRAINT FK_LoginUser_Environment FOREIGN KEY (EnvironmentId) REFERENCES aut.[Environment](EnvironmentId),
        CONSTRAINT FK_LoginUser_PortalUser FOREIGN KEY (PortalUserId) REFERENCES aut.[User](UserID),
        CONSTRAINT FK_LoginUser_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES aut.[User](UserID),
        CONSTRAINT FK_LoginUser_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES aut.[User](UserID)
    );
END
GO

-- 3. aut.TestCaseExecutionQueue: LoginUserId (nullable - the login user selected at
-- Run Now/Schedule time; NULL when the environment doesn't require authentication).
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'aut' AND TABLE_NAME = 'TestCaseExecutionQueue' AND COLUMN_NAME = 'LoginUserId'
)
BEGIN
    ALTER TABLE aut.TestCaseExecutionQueue ADD LoginUserId INT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TestCaseExecutionQueue_LoginUser')
BEGIN
    ALTER TABLE aut.TestCaseExecutionQueue
    ADD CONSTRAINT FK_TestCaseExecutionQueue_LoginUser FOREIGN KEY (LoginUserId) REFERENCES aut.LoginUser(LoginUserId);
END
GO

-- 4. Stored procedures

CREATE OR ALTER PROCEDURE [aut].[usp_LoginUserCreate]
(
    @EnvironmentId INT,
    @PortalUserId INT = NULL,
    @UserRole NVARCHAR(50),
    @UserName NVARCHAR(100),
    @EncryptedPassword NVARCHAR(500),
    @CreatedBy INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO aut.LoginUser
    (EnvironmentId, PortalUserId, UserRole, UserName, EncryptedPassword, CreatedBy)
    VALUES
    (@EnvironmentId, @PortalUserId, @UserRole, @UserName, @EncryptedPassword, @CreatedBy);

    SELECT SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE [aut].[usp_LoginUserUpdate]
(
    @LoginUserId INT,
    @PortalUserId INT = NULL,
    @UserRole NVARCHAR(50),
    @UserName NVARCHAR(100),
    @EncryptedPassword NVARCHAR(500) = NULL, -- NULL = keep existing password unchanged
    @IsActive BIT,
    @ModifiedBy INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE aut.LoginUser
    SET
        PortalUserId = @PortalUserId,
        UserRole = @UserRole,
        UserName = @UserName,
        EncryptedPassword = COALESCE(@EncryptedPassword, EncryptedPassword),
        IsActive = @IsActive,
        ModifiedBy = @ModifiedBy,
        ModifiedOn = SYSDATETIME()
    WHERE LoginUserId = @LoginUserId;
END
GO

CREATE OR ALTER PROCEDURE [aut].[usp_LoginUserSoftDelete]
(
    @LoginUserId INT,
    @ModifiedBy INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE aut.LoginUser
    SET
        IsActive = 0,
        ModifiedBy = @ModifiedBy,
        ModifiedOn = SYSDATETIME()
    WHERE LoginUserId = @LoginUserId;
END
GO

-- Resolves the shared "default" login user for a given Environment+Role, used only by
-- BaseFeatureFixture.LoginByProfile's mid-test role-switch (e.g. TC.Registration logging
-- in as a different role partway through a test) - a genuinely different use case from
-- the initial Run Now/Schedule login (an unattended in-test call, not a human picking
-- from a dropdown), so a role-keyed lookup is appropriate here specifically. Returns the
-- most-recently-created active match for that Environment+Role, or no rows if none
-- configured - caller falls back to a clear failure, never silently to hard-coded data.
CREATE OR ALTER PROCEDURE [aut].[usp_LoginUserResolveByRole]
(
    @EnvironmentId INT,
    @UserRole NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        LoginUserId,
        UserName,
        EncryptedPassword
    FROM aut.LoginUser
    WHERE EnvironmentId = @EnvironmentId AND UserRole = @UserRole AND IsActive = 1
    ORDER BY CreatedOn DESC;
END
GO

-- Guarded like usp_EnvironmentHardDelete - a LoginUserId that's already been used by a
-- real queued/scheduled/executed run (aut.TestCaseExecutionQueue.LoginUserId, FK'd to
-- this table) can't be hard-deleted without violating that FK; raise a clear error
-- instead of letting the caller hit a raw FK-violation exception.
CREATE OR ALTER PROCEDURE [aut].[usp_LoginUserHardDelete]
(
    @LoginUserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @QueueUsageCount INT;

    SELECT @QueueUsageCount = COUNT(*)
    FROM aut.TestCaseExecutionQueue
    WHERE LoginUserId = @LoginUserId;

    IF @QueueUsageCount > 0
    BEGIN
        RAISERROR('Cannot delete: this login user has already been used by %d queued/scheduled run(s). Disable it instead.', 16, 1, @QueueUsageCount);
        RETURN;
    END

    DELETE FROM aut.LoginUser
    WHERE LoginUserId = @LoginUserId;
END
GO

-- Used by both the Login Users management screen and the Run Now/Schedule dropdowns -
-- never returns EncryptedPassword.
CREATE OR ALTER PROCEDURE [aut].[usp_LoginUserGetByEnvironment]
(
    @EnvironmentId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        lu.LoginUserId,
        lu.EnvironmentId,
        lu.PortalUserId,
        pu.UserName AS PortalUserName,
        lu.UserRole,
        lu.UserName,
        lu.IsActive,
        lu.CreatedOn,
        lu.ModifiedOn
    FROM aut.LoginUser lu
    LEFT JOIN aut.[User] pu ON lu.PortalUserId = pu.UserID
    WHERE lu.EnvironmentId = @EnvironmentId
    ORDER BY lu.CreatedOn DESC;
END
GO

-- The only procedure that ever returns EncryptedPassword - a plain lookup by primary
-- key, called only by an isolated test process's service JWT (see LoginUserController).
CREATE OR ALTER PROCEDURE [aut].[usp_LoginUserGetCredentials]
(
    @LoginUserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        LoginUserId,
        UserName,
        EncryptedPassword
    FROM aut.LoginUser
    WHERE LoginUserId = @LoginUserId AND IsActive = 1;
END
GO

-- 5. Queue insert procs + pending-queue getter: thread LoginUserId (optional) through.

CREATE OR ALTER PROCEDURE [aut].[usp_SingleRunTestCaseNow]
(
    @AssignmentId INT,
    @AssignmentTestCaseId INT,
    @Browser VARCHAR(100),
    @LoginUserId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @QueueId UNIQUEIDENTIFIER = NEWID();
    DECLARE @QueueStatus VARCHAR(50) = 'Queued';
    DECLARE @TestCaseStatus VARCHAR(50) = 'Queued';

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO aut.TestCaseExecutionQueue
        (
            QueueId,
            AssignmentId,
            AssignmentTestCaseId,
            QueueStatus,
            CreatedDate,
            ExecutionDateTime,
            Browser,
            LoginUserId
        )
        VALUES
        (
            @QueueId,
            @AssignmentId,
            @AssignmentTestCaseId,
            @QueueStatus,
            GETDATE(),
            GETDATE(),
            @Browser,
            @LoginUserId
        );

        UPDATE aut.AssignedTestCases
        SET TestCaseStatus = @TestCaseStatus
        WHERE AssignmentTestCaseId = @AssignmentTestCaseId;

        SELECT
            CAST(SCOPE_IDENTITY() AS INT) AS Id,
            @QueueId AS QueueId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [aut].[usp_BulkRunTestCasesNow]
(
    @AssignmentId INT,
    @AssignmentTestCaseIds aut.AssignmentTestCaseIdList READONLY,
    @Browser VARCHAR(100),
    @LoginUserId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @QueueStatus VARCHAR(50) = 'Queued';
    DECLARE @TestCaseStatus VARCHAR(50) = 'Queued';

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO aut.TestCaseExecutionQueue
        (
            QueueId,
            AssignmentId,
            AssignmentTestCaseId,
            QueueStatus,
            CreatedDate,
            ExecutionDateTime,
            Browser,
            LoginUserId
        )
        SELECT
            NEWID() AS QueueId,
            @AssignmentId,
            AssignmentTestCaseId,
            @QueueStatus,
            GETDATE(),
            GETDATE(),
            @Browser,
            @LoginUserId
        FROM @AssignmentTestCaseIds;

        UPDATE ATC
        SET ATC.TestCaseStatus = @TestCaseStatus
        FROM aut.AssignedTestCases ATC
        INNER JOIN @AssignmentTestCaseIds T
            ON ATC.AssignmentTestCaseId = T.AssignmentTestCaseId;

        SELECT 1 AS Success;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE [aut].[usp_ScheduleSingleTestCase]
(
    @AssignmentId INT,
    @AssignmentTestCaseId INT,
    @ScheduleDate DATETIME,
    @Browser VARCHAR(100),
    @LoginUserId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @QueueId UNIQUEIDENTIFIER = NEWID();
    DECLARE @QueueStatus VARCHAR(50) = 'Scheduled';
    DECLARE @TestCaseStatus VARCHAR(50) = 'Scheduled';

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO aut.TestCaseExecutionQueue
        (
            QueueId,
            AssignmentId,
            AssignmentTestCaseId,
            QueueStatus,
            CreatedDate,
            ExecutionDateTime,
            Browser,
            LoginUserId
        )
        VALUES
        (
            @QueueId,
            @AssignmentId,
            @AssignmentTestCaseId,
            @QueueStatus,
            GETDATE(),
            @ScheduleDate,
            @Browser,
            @LoginUserId
        );

        UPDATE aut.AssignedTestCases
        SET TestCaseStatus = @TestCaseStatus
        WHERE AssignmentTestCaseId = @AssignmentTestCaseId;

        SELECT
            CAST(SCOPE_IDENTITY() AS INT) AS Id,
            @QueueId AS QueueId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE [aut].[usp_BulkScheduleTestCases]
(
    @AssignmentId INT,
    @AssignmentTestCaseIds aut.AssignmentTestCaseIdList READONLY,
    @ScheduleDate DATETIME,
    @Browser VARCHAR(100),
    @LoginUserId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @QueueStatus VARCHAR(50) = 'Scheduled';
    DECLARE @TestCaseStatus VARCHAR(50) = 'Scheduled';

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO aut.TestCaseExecutionQueue
        (
            QueueId,
            AssignmentId,
            AssignmentTestCaseId,
            QueueStatus,
            CreatedDate,
            ExecutionDateTime,
            Browser,
            LoginUserId
        )
        SELECT
            NEWID() AS QueueId,
            @AssignmentId,
            AssignmentTestCaseId,
            @QueueStatus,
            GETDATE(),
            @ScheduleDate,
            @Browser,
            @LoginUserId
        FROM @AssignmentTestCaseIds;

        UPDATE ATC
        SET ATC.TestCaseStatus = @TestCaseStatus
        FROM aut.AssignedTestCases ATC
        INNER JOIN @AssignmentTestCaseIds T
            ON ATC.AssignmentTestCaseId = T.AssignmentTestCaseId;

        SELECT 1 AS Success;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE aut.usp_GetPendingExecutionQueues
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Q.QueueId,
        Q.AssignmentTestCaseId,
        ATC.AssignmentId,
        ATC.LibraryName,
        ATC.ClassName,
        ATC.MethodName,
        TCA.Environment,
        TCA.EnvironmentId,
        TCA.ReleaseId,
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

-- 6. Seed data - EnvironmentUrl only, for E2EP3/PROD (see header comment), matching
-- LoginService.GetLoginUrl()'s current hard-coded switch. aut.LoginUser rows for these
-- two environments (mirroring UserCredentials.cs's hard-coded username/password
-- dictionaries) are seeded separately via a real POST api/LoginUser call after
-- deployment, not raw SQL INSERTs here - the API's CredentialCipher encrypts the
-- password server-side, so there's no need for (and no good way to produce, safely, in
-- a raw SQL script) pre-computed ciphertext literals.
IF EXISTS (SELECT 1 FROM aut.[Environment] WHERE EnvironmentId = 21 AND EnvironmentName = 'E2EP3')
BEGIN
    UPDATE aut.[Environment]
    SET EnvironmentUrl = 'https://ohpnm-e2ep3.omes.maximus.com/OH_PNM_E2EP3/Account/Login.aspx'
    WHERE EnvironmentId = 21 AND EnvironmentUrl IS NULL;
END
GO

IF EXISTS (SELECT 1 FROM aut.[Environment] WHERE EnvironmentId = 11 AND EnvironmentName = 'PROD')
BEGIN
    UPDATE aut.[Environment]
    SET EnvironmentUrl = 'https://ohpnm.omes.maximus.com/OH_PNM_PROD/Account/Login.aspx'
    WHERE EnvironmentId = 11 AND EnvironmentUrl IS NULL;
END
GO
