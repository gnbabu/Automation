CREATE OR ALTER PROCEDURE [aut].[usp_GetUserByUsername]
    @Username NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

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
        role.RoleID
    FROM [aut].[User] usr
    INNER JOIN [aut].[UserRole] role ON usr.RoleID = role.RoleID
    WHERE usr.UserName = @Username
      AND usr.Active = 1;
END
