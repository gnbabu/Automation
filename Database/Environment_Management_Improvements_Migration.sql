-- Environment_Management_Improvements_Migration.sql
-- Idempotent (safe to re-run). Implements:
--   1. ModifiedBy tracking on aut.Environment (previously only ModifiedOn was tracked -
--      no record of *who* last edited/disabled an environment).
--   2. A guard in usp_EnvironmentHardDelete against deleting an environment that's still
--      referenced by real data (aut.Release / aut.TestCaseAssignment / aut.AutomationData -
--      all 3 confirmed via sys.foreign_keys) - previously an unguarded DELETE that would
--      surface a raw FK-violation error to the caller.
--   3. usp_EnvironmentGetAll/usp_EnvironmentGetById now also return ModifiedByName and a
--      ReleaseCount (how many Releases currently use this environment), needed by the
--      frontend to show "N releases use this environment" and gate the Delete button.

SET QUOTED_IDENTIFIER ON;
GO

-- 1. Add ModifiedBy column (nullable - existing rows have never been "modified" by anyone
-- distinct from their creator under the old code).
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'aut' AND TABLE_NAME = 'Environment' AND COLUMN_NAME = 'ModifiedBy'
)
BEGIN
    ALTER TABLE aut.[Environment] ADD ModifiedBy INT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Environment_ModifiedBy'
)
BEGIN
    ALTER TABLE aut.[Environment]
    ADD CONSTRAINT FK_Environment_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES aut.[User](UserID);
END
GO

-- 2. usp_EnvironmentUpdate - now also stamps ModifiedBy.
CREATE OR ALTER PROCEDURE [aut].[usp_EnvironmentUpdate]
(
    @EnvironmentId INT,
    @EnvironmentName NVARCHAR(50),
    @Description NVARCHAR(255),
    @IsActive BIT,
    @ModifiedBy INT = NULL
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
        ModifiedOn = SYSDATETIME()
    WHERE EnvironmentId = @EnvironmentId;
END
GO

-- 3. usp_EnvironmentSoftDelete - now also stamps ModifiedBy.
CREATE OR ALTER PROCEDURE [aut].[usp_EnvironmentSoftDelete]
(
    @EnvironmentId INT,
    @ModifiedBy INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE aut.Environment
    SET
        IsActive = 0,
        ModifiedBy = @ModifiedBy,
        ModifiedOn = SYSDATETIME()
    WHERE EnvironmentId = @EnvironmentId;
END
GO

-- 4. usp_EnvironmentHardDelete - guarded against in-use environments instead of an
-- unconditional DELETE that would otherwise surface a raw FK-violation error.
CREATE OR ALTER PROCEDURE [aut].[usp_EnvironmentHardDelete]
(
    @EnvironmentId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ReleaseCount INT, @AssignmentCount INT, @AutomationDataCount INT;

    SELECT @ReleaseCount = COUNT(*) FROM aut.[Release] WHERE EnvironmentId = @EnvironmentId;
    SELECT @AssignmentCount = COUNT(*) FROM aut.TestCaseAssignment WHERE EnvironmentId = @EnvironmentId;
    SELECT @AutomationDataCount = COUNT(*) FROM aut.AutomationData WHERE EnvironmentId = @EnvironmentId;

    IF (@ReleaseCount + @AssignmentCount + @AutomationDataCount) > 0
    BEGIN
        RAISERROR('Cannot delete: this environment has %d associated Release(s) and/or other data. Deactivate it instead.', 16, 1, @ReleaseCount);
        RETURN;
    END

    DELETE FROM aut.Environment
    WHERE EnvironmentId = @EnvironmentId;
END
GO

-- 5. usp_EnvironmentGetAll - adds ModifiedByName + ReleaseCount.
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

-- 6. usp_EnvironmentGetById - same additions.
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
