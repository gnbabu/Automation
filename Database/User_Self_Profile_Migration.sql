-- User_Self_Profile_Migration.sql
-- Idempotent (CREATE OR ALTER). Adds usp_UpdateUserProfile: a self-service,
-- *partial* update used by Settings' new "Edit Profile" feature (PUT /api/Users/me/profile).
--
-- Deliberately narrower than usp_UpdateUser: only Photo/PhoneNumber/TimeZone are ever
-- written. RoleID/Active/Status/PriorityId/Teams/UserName/Email/PasswordHash are never
-- touched by this proc, so it's safe to expose without any Admin/role restriction - any
-- authenticated user may call it for their own UserId (identity comes from the caller's
-- JWT server-side, never from this proc's parameters).
--
-- @Photo is only overwritten when a new photo was actually supplied (COALESCE), since the
-- Settings page's "Edit Profile" form lets a user update just their phone/time zone
-- without re-uploading a photo every time. @PhoneNumber/@TimeZone are always set to
-- whatever was submitted (including clearing them to NULL).
--
-- aut.[User] has a filtered index (IX_User_ResetPasswordToken), so - same as aut.Release,
-- see AGENTS.md - any UPDATE against it requires QUOTED_IDENTIFIER ON. A stored procedure
-- bakes in whatever QUOTED_IDENTIFIER setting was active at CREATE time, so this must be
-- set explicitly here rather than relying on the caller's session/sqlcmd's -I flag.
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [aut].[usp_UpdateUserProfile]
    @UserId      INT,
    @Photo       VARBINARY(MAX) = NULL,
    @PhoneNumber NVARCHAR(20)   = NULL,
    @TimeZone    INT            = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [aut].[User]
    SET
        Photo       = COALESCE(@Photo, Photo),
        PhoneNumber = @PhoneNumber,
        TimeZone    = @TimeZone
    WHERE UserID = @UserId;
END
GO
