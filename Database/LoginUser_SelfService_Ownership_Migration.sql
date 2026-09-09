-- LoginUser_SelfService_Ownership_Migration.sql
-- Idempotent (safe to re-run). Implements self-service ownership for aut.LoginUser:
--   - Each Portal User manages only their own login credential per environment (no more
--     admin picking a Portal User on someone else's behalf via a dropdown).
--   - New usp_LoginUserGetByEnvironmentAndPortalUser - ownership-filtered read, used only
--     by the new self-service "Credential Configuration" screen.
--   - usp_LoginUserGetByEnvironment (unfiltered - every login user for an environment,
--     any owner) is left completely UNCHANGED - Run Now/Schedule now call the new
--     "mine" endpoint/proc instead, but nothing about the existing one needs to change.
--   - usp_LoginUserUpdate/SoftDelete/HardDelete gain a @PortalUserId ownership parameter:
--     a caller who doesn't own the row affects 0 rows, so the API layer can distinguish
--     "not found" vs "not yours" and return 403 instead of silently no-op'ing.
--   - usp_LoginUserCreate is UNCHANGED (still just inserts whatever @PortalUserId it's
--     given) - ownership enforcement on create happens in the API layer instead (it
--     always passes the caller's own id, never trusting the client), not here.

SET QUOTED_IDENTIFIER ON;
GO

-- Same shape as usp_LoginUserGetByEnvironment, plus an owner filter. Used only by the
-- self-service Credential Configuration screen - Run Now/Schedule keep using the
-- existing, unfiltered usp_LoginUserGetByEnvironment (they need to see the credential
-- the person running/scheduling the test picked for themselves via the same ownership
-- filter, resolved by the API from the caller's own JWT - see LoginUserController).
CREATE OR ALTER PROCEDURE [aut].[usp_LoginUserGetByEnvironmentAndPortalUser]
(
    @EnvironmentId INT,
    @PortalUserId INT
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
    WHERE lu.EnvironmentId = @EnvironmentId AND lu.PortalUserId = @PortalUserId
    ORDER BY lu.CreatedOn DESC;
END
GO

-- Ownership-enforced: @PortalUserId must match the row's own PortalUserId, or the
-- UPDATE affects 0 rows. A pre-existing row with PortalUserId IS NULL (e.g. old seed/
-- verification data) matches nobody - intentionally not self-service-editable.
CREATE OR ALTER PROCEDURE [aut].[usp_LoginUserUpdate]
(
    @LoginUserId INT,
    @PortalUserId INT,
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
        UserRole = @UserRole,
        UserName = @UserName,
        EncryptedPassword = COALESCE(@EncryptedPassword, EncryptedPassword),
        IsActive = @IsActive,
        ModifiedBy = @ModifiedBy,
        ModifiedOn = SYSDATETIME()
    WHERE LoginUserId = @LoginUserId AND PortalUserId = @PortalUserId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

CREATE OR ALTER PROCEDURE [aut].[usp_LoginUserSoftDelete]
(
    @LoginUserId INT,
    @PortalUserId INT,
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
    WHERE LoginUserId = @LoginUserId AND PortalUserId = @PortalUserId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- Still guarded like usp_EnvironmentHardDelete (a LoginUserId already used by a real
-- queued/scheduled/executed run can't be hard-deleted without violating the FK) - now
-- also ownership-checked.
CREATE OR ALTER PROCEDURE [aut].[usp_LoginUserHardDelete]
(
    @LoginUserId INT,
    @PortalUserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM aut.LoginUser WHERE LoginUserId = @LoginUserId AND PortalUserId = @PortalUserId)
    BEGIN
        SELECT 0 AS RowsAffected;
        RETURN;
    END

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
    WHERE LoginUserId = @LoginUserId AND PortalUserId = @PortalUserId;

    SELECT @@ROWCOUNT AS RowsAffected;
END
GO
