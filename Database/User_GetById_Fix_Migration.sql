-- User_GetById_Fix_Migration.sql
-- Idempotent (CREATE OR ALTER). Fixes usp_GetUserById, which was missing Status/
-- StatusName/Priority/PriorityName/LastLogin entirely (pre-existing gap, only noticed now
-- that Settings' redesigned Profile card actually surfaces Status/Last Login - they were
-- always silently blank for GET /api/Users/{id}, unlike GetAllUsers which already joins
-- and selects all of these). Brought in line with usp_GetAllUsers's joins/columns.
--
-- SET QUOTED_IDENTIFIER ON for consistency with every other proc that touches aut.[User]
-- (see User_Self_Profile_Migration.sql / AGENTS.md) - not strictly required for a
-- SELECT-only proc against the filtered-index table, but keeps this proc's baked-in
-- setting correct in case anything here ever changes to a modification statement.
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [aut].[usp_GetUserById]
    @UserID INT
AS
BEGIN
    SELECT
        usr.UserID,
        usr.UserName,
        usr.PasswordHash,
        usr.FirstName,
        usr.LastName,
        usr.Email,
        usr.Photo,
        usr.Active,
        role.RoleName,
        role.RoleID,
        usr.[Priority],
        priority.PriorityName,
        usr.TimeZone,
        tz.TimeZoneName,
        usr.LastLogin,
        usr.[Status],
        us.[StatusName],
        usr.[PhoneNumber],
        usr.TwoFactorEnabled AS TwoFactor,
        usr.[TeamsProjects] AS Teams
    FROM [aut].[User] AS usr
    INNER JOIN [aut].[UserRole] AS role ON usr.RoleID = role.RoleID
    LEFT JOIN [aut].[PriorityStatus] AS priority ON usr.Priority = priority.PriorityID
    LEFT JOIN [aut].TimeZone AS tz ON usr.TimeZone = tz.TimeZoneID
    LEFT JOIN [aut].[UserStatus] AS us ON us.StatusID = usr.[Status]
    WHERE usr.UserID = @UserID;
END
GO
