-- Environment_SSO_Migration.sql
-- Idempotent (safe to re-run). Adds an "Enable SSO" flag to aut.Environment, for
-- environments where the real application uses third-party/SSO authentication in
-- production and has no login screen of its own - the environment still requires
-- authentication (RequiresAuthentication stays true/1), but there's no form for Selenium
-- tests to log into, so at execution time this is treated the same as
-- RequiresAuthentication = false (no Login User needed, no login step attempted).
--
-- EnableSso and RequiresAuthentication are mutually exclusive: checking one in the Portal
-- UI automatically unchecks the other (a manual username/password login and "no login
-- screen at all" can't both be true for the same environment), and this migration's own
-- updated procedures normalize it server-side too (defense in depth against a direct API
-- call bypassing the UI) - EnableSso wins if somehow both were sent true.

SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'aut' AND TABLE_NAME = 'Environment' AND COLUMN_NAME = 'EnableSso'
)
BEGIN
    ALTER TABLE aut.[Environment] ADD EnableSso BIT NOT NULL DEFAULT 0;
END
GO

CREATE OR ALTER PROCEDURE [aut].[usp_EnvironmentCreate]
(
    @EnvironmentName NVARCHAR(50),
    @Description NVARCHAR(255),
    @CreatedBy INT,
    @EnvironmentUrl NVARCHAR(500) = NULL,
    @RequiresAuthentication BIT = 1,
    @EnableSso BIT = 0
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

    -- Authentication Required and Enable SSO are mutually exclusive - normalized here
    -- server-side even though the Portal's own UI already prevents this combination. SSO
    -- wins if somehow both were sent true.
    IF @EnableSso = 1 SET @RequiresAuthentication = 0;

    INSERT INTO aut.Environment
    (
        EnvironmentName,
        Description,
        IsActive,
        CreatedBy,
        EnvironmentUrl,
        RequiresAuthentication,
        EnableSso
    )
    VALUES
    (
        @EnvironmentName,
        @Description,
        1,
        @CreatedBy,
        @EnvironmentUrl,
        @RequiresAuthentication,
        @EnableSso
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
    @RequiresAuthentication BIT = 1,
    @EnableSso BIT = 0
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @EnableSso = 1 SET @RequiresAuthentication = 0;

    UPDATE aut.Environment
    SET
        EnvironmentName = @EnvironmentName,
        Description = @Description,
        IsActive = @IsActive,
        ModifiedBy = @ModifiedBy,
        ModifiedOn = SYSDATETIME(),
        EnvironmentUrl = @EnvironmentUrl,
        RequiresAuthentication = @RequiresAuthentication,
        EnableSso = @EnableSso
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
        e.EnableSso,

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
