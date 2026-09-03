-- Release_Activate_Guard_Migration.sql
-- Idempotent (CREATE OR ALTER). Fixes duplicate Release notifications: usp_ActivateRelease
-- had no guard against activating a release that's already Active/Completed/Rejected - it
-- unconditionally re-stamped ActivatedBy/ActivatedOn and returned success every time
-- called, and ReleaseController.Activate() unconditionally sends a fresh notification
-- batch to every Manager/Admin after every successful call. The frontend's canActivate
-- guard normally keeps the Activate button disabled once a release isn't Draft, but that's
-- a UI-only guard - calling the endpoint directly (or a stray double-click/race before the
-- button re-disables) could still re-activate and re-notify. Confirmed live: a Release
-- activated twice during testing ended up with 14 notification rows (2 full batches of 7
-- recipients) instead of 7, which is what actually prompted this fix.
--
-- Now: only a Draft release can be activated; anything else raises a clear error (which
-- the controller already surfaces as a 400 via GetUserMessage), matching the same
-- Draft-only-transition convention already used elsewhere (e.g. Delete only allowed while
-- Draft).

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE aut.usp_ActivateRelease
(
    @ReleaseId    INT,
    @ActivatedBy  NVARCHAR(100)
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EnvironmentId INT, @FolderPath NVARCHAR(500), @EnvActive BIT, @CurrentLifecycle NVARCHAR(50), @LifecycleForMessage NVARCHAR(50);

    SELECT
        @EnvironmentId = EnvironmentId,
        @FolderPath = ReleaseFolderPath,
        @CurrentLifecycle = ReleaseLifecycle
    FROM aut.[Release] WHERE ReleaseId = @ReleaseId;

    IF @EnvironmentId IS NULL
    BEGIN
        RAISERROR('Cannot activate: release has no environment.', 16, 1); RETURN;
    END

    -- Guard: only a Draft release can be activated. Without this, calling activate on an
    -- already-Active/Completed/Rejected release silently re-stamped ActivatedOn/By and let
    -- the controller re-send a full notification batch every time.
    IF @CurrentLifecycle IS NULL OR @CurrentLifecycle <> 'Draft'
    BEGIN
        SET @LifecycleForMessage = ISNULL(@CurrentLifecycle, N'in an unknown lifecycle state');
        RAISERROR('Cannot activate: release is already %s.', 16, 1, @LifecycleForMessage); RETURN;
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
