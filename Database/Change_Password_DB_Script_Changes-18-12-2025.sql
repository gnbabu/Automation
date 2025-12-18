/****** Object:  StoredProcedure [aut].[usp_UpdateUser]    Script Date: 18-12-2025 23:49:30 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_UpdateUser]
GO
/****** Object:  StoredProcedure [aut].[usp_UpdateLastLogin]    Script Date: 18-12-2025 23:49:30 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_UpdateLastLogin]
GO
/****** Object:  StoredProcedure [aut].[usp_SetUserActiveStatus]    Script Date: 18-12-2025 23:49:30 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_SetUserActiveStatus]
GO
/****** Object:  StoredProcedure [aut].[usp_ResetPassword]    Script Date: 18-12-2025 23:49:30 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_ResetPassword]
GO
/****** Object:  StoredProcedure [aut].[usp_RegisterUser]    Script Date: 18-12-2025 23:49:30 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_RegisterUser]
GO
/****** Object:  StoredProcedure [aut].[usp_GetUsernameByEmail]    Script Date: 18-12-2025 23:49:30 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetUsernameByEmail]
GO
/****** Object:  StoredProcedure [aut].[usp_GetUserByUsername]    Script Date: 18-12-2025 23:49:30 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetUserByUsername]
GO
/****** Object:  StoredProcedure [aut].[usp_GetUserById]    Script Date: 18-12-2025 23:49:30 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetUserById]
GO
/****** Object:  StoredProcedure [aut].[usp_GetFilteredUsers]    Script Date: 18-12-2025 23:49:30 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetFilteredUsers]
GO
/****** Object:  StoredProcedure [aut].[usp_GetAllUsers]    Script Date: 18-12-2025 23:49:30 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_GetAllUsers]
GO
/****** Object:  StoredProcedure [aut].[usp_ForgotPassword]    Script Date: 18-12-2025 23:49:30 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_ForgotPassword]
GO
/****** Object:  StoredProcedure [aut].[usp_DeleteUser]    Script Date: 18-12-2025 23:49:30 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_DeleteUser]
GO
/****** Object:  StoredProcedure [aut].[usp_CreateUser]    Script Date: 18-12-2025 23:49:30 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_CreateUser]
GO
/****** Object:  StoredProcedure [aut].[usp_ChangePassword]    Script Date: 18-12-2025 23:49:30 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_ChangePassword]
GO

/****** Object:  StoredProcedure [aut].[usp_ValidateUserByUsernameAndPassword]    Script Date: 18-12-2025 23:49:30 ******/
DROP PROCEDURE IF EXISTS [aut].[usp_ValidateUserByUsernameAndPassword]
GO

/****** Object:  StoredProcedure [aut].[usp_ChangePassword]    Script Date: 18-12-2025 23:49:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [aut].[usp_ChangePassword]
    @UserID INT,
    @NewPasswordHash NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1 FROM [aut].[User]
        WHERE UserID = @UserID AND Active = 1
    )
    BEGIN
        SELECT 0;
        RETURN;
    END

    UPDATE [aut].[User]
    SET PasswordHash = @NewPasswordHash
    WHERE UserID = @UserID;

    SELECT 1;
END
GO
/****** Object:  StoredProcedure [aut].[usp_CreateUser]    Script Date: 18-12-2025 23:49:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_CreateUser]
    @UserName       NVARCHAR(100),
    @PasswordHash    NVARCHAR(500),
    @FirstName      NVARCHAR(100) = NULL,
    @LastName       NVARCHAR(100) = NULL,
    @Email          NVARCHAR(255),
    @Photo          VARBINARY(MAX) = NULL,
    @RoleID         INT, 
    @Active         BIT = 1,           
    @TimeZone       INT = NULL,
    @TwoFactor      BIT = 0,          
    @Teams          NVARCHAR(255) = NULL,
    @PhoneNumber    NVARCHAR(20) = NULL,
	@Priority       INT = NULL,
	@Status         INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [aut].[User] 
    (
        UserName,
        PasswordHash,
        FirstName,
        LastName,
        Email,
        Photo,
        RoleID,
        Active,
		CreatedAt,
        TimeZone,
        TwoFactorEnabled,
        TeamsProjects,
        PhoneNumber,
		Priority,
        Status
    )
    VALUES 
    (
        @UserName,
        @PasswordHash,
        @FirstName,
        @LastName,
        @Email,
        @Photo,
        @RoleID,
        @Active,
		GETDATE(),
        @TimeZone,
        @TwoFactor,
        @Teams,
        @PhoneNumber,
		@Priority,
        @Status
    );

    SELECT SCOPE_IDENTITY() AS NewUserID;
END
GO
/****** Object:  StoredProcedure [aut].[usp_DeleteUser]    Script Date: 18-12-2025 23:49:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_DeleteUser]
    @UserID INT
AS
BEGIN
    DELETE FROM [aut].[User]
    WHERE UserID = @UserID;
END
GO
/****** Object:  StoredProcedure [aut].[usp_ForgotPassword]    Script Date: 18-12-2025 23:49:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [aut].[usp_ForgotPassword]
    @Email NVARCHAR(256),
    @Token NVARCHAR(200),
    @Expiry DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    -- User does not exist
    IF NOT EXISTS (
        SELECT 1
        FROM [MES_AUT].[aut].[User]
        WHERE Email = @Email AND Active = 1
    )
    BEGIN
        SELECT 0; -- ❌ User not found
        RETURN;
    END

    UPDATE [MES_AUT].[aut].[User]
    SET
        ResetPasswordToken = @Token,
        ResetPasswordExpiry = @Expiry
    WHERE Email = @Email
      AND Active = 1;

    SELECT 1; -- ✅ Success
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetAllUsers]    Script Date: 18-12-2025 23:49:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_GetAllUsers]
AS
BEGIN
    SELECT 
        usr.UserID,
        usr.UserName,
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
		usr.TwoFactorEnabled as TwoFactor,
		usr.[TeamsProjects] as Teams
    FROM [aut].[User] AS usr
    LEFT JOIN [aut].[UserRole] AS role ON usr.RoleID = role.RoleID
	LEFT JOIN [aut].[PriorityStatus] AS priority ON usr.Priority = priority.PriorityID
	LEFT JOIN [aut].TimeZone AS tz on usr.TimeZone = tz.TimeZoneID
	LEFT JOIN [aut].[UserStatus] AS us on us.StatusID = usr.[Status];
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetFilteredUsers]    Script Date: 18-12-2025 23:49:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/**********************************************************************************************************
Project Name: OHPNM - Automation Portal
SP Name: aut.[usp_GetFilteredUsers]
Purpose: This sp is used to get users based on search filters
Initial Creator: Shikha Aggarwal
Initial Creation Date: 15-Oct-2025
***********************************************************************************************************/

CREATE PROCEDURE [aut].[usp_GetFilteredUsers]
    @Search NVARCHAR(100) = NULL,
    @Status INT = NULL,
    @Role INT = NULL,
    @Priority INT = NULL
AS
BEGIN
    SELECT 
	    usr.UserID,
        usr.UserName,
        usr.FirstName,
        usr.LastName,
        usr.Email,
        usr.Photo,
		usr.Active,
        ur.RoleName,
		usr.RoleID,
		usr.[Priority],
		up.PriorityName,
		usr.TimeZone,
		tz.TimeZoneName,
		usr.LastLogin,
		usr.[Status],
		us.[StatusName],
		usr.[PhoneNumber],
		usr.TwoFactorEnabled as TwoFactor,
		usr.[TeamsProjects] as Teams
    FROM aut.[User] usr
    LEFT JOIN [aut].[UserRole] AS ur ON usr.RoleID = ur.RoleID
    LEFT JOIN [aut].[PriorityStatus] AS up ON usr.Priority = up.PriorityID
    LEFT JOIN [aut].[TimeZone] AS tz ON usr.TimeZone = tz.TimeZoneID
    LEFT JOIN [aut].[UserStatus] AS us ON us.StatusID = usr.[Status]
    WHERE
	(
        @Search IS NULL OR  @Search = '' OR
        (LTRIM(RTRIM(usr.FirstName)) + ' ' + LTRIM(RTRIM(usr.LastName))) LIKE '%' + @Search + '%' OR 
        usr.Email LIKE '%' + @Search + '%'
    )
    AND ((@Status IS NULL OR @Status = 0 OR usr.Status = @Status)
    AND (@Role IS NULL OR @Role = 0 OR usr.RoleID = @Role)
    AND (@Priority IS NULL OR @Priority = 0 OR usr.Priority = @Priority))
END


GO
/****** Object:  StoredProcedure [aut].[usp_GetUserById]    Script Date: 18-12-2025 23:49:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_GetUserById]
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
		usr.TimeZone,
		tz.TimeZoneName,
		usr.[PhoneNumber],
		usr.TwoFactorEnabled as TwoFactor,
		usr.[TeamsProjects] as Teams
    FROM [aut].[User] AS usr
    INNER JOIN [aut].[UserRole] AS role ON usr.RoleID = role.RoleID
	LEFT JOIN [aut].TimeZone AS tz on usr.TimeZone = tz.TimeZoneID
	LEFT JOIN [aut].[UserStatus] AS us on us.StatusID = usr.[Status]
    WHERE usr.UserID = @UserID;
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetUserByUsername]    Script Date: 18-12-2025 23:49:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [aut].[usp_GetUserByUsername]
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
GO
/****** Object:  StoredProcedure [aut].[usp_GetUsernameByEmail]    Script Date: 18-12-2025 23:49:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetUsernameByEmail]
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        UserID,
        UserName,
        Email
    FROM [MES_AUT].[aut].[User]
    WHERE Email = @Email
      AND Active = 1;
END
GO
/****** Object:  StoredProcedure [aut].[usp_RegisterUser]    Script Date: 18-12-2025 23:49:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_RegisterUser]
    @Username NVARCHAR(100),
    @Email NVARCHAR(255),
    @PasswordHash NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @RoleID INT;
    DECLARE @StatusID INT;

    -- Default Role = Viewer
    SELECT @RoleID = RoleID 
    FROM [aut].[UserRole]
    WHERE RoleName = 'Viewer';

    IF (@RoleID IS NULL)
    BEGIN
        THROW 50001, 'Default role ''Viewer'' not found in UserRole table.', 1;
        RETURN;
    END;

    -- Default Status = Suspended
    SELECT @StatusID = StatusID
    FROM [aut].[UserStatus]
    WHERE StatusName = 'Suspended';

    IF (@StatusID IS NULL)
    BEGIN
        THROW 50002, 'Default status ''Suspended'' not found in UserStatus table.', 1;
        RETURN;
    END;

    -- Insert User
    INSERT INTO [aut].[User]
    (
        UserName,
        Email,
        PasswordHash,
        FirstName,
        LastName,
        RoleID,
        Status,
        Active,
        CreatedAt
    )
    VALUES
    (
        @Username,
        @Email,
        @PasswordHash,
        @Username,     -- FirstName = Username 
        '',            -- LastName empty
        @RoleID,
        @StatusID,
        1,             -- Active
        GETDATE()
    );

    SELECT SCOPE_IDENTITY() AS NewUserID;
END
GO
/****** Object:  StoredProcedure [aut].[usp_ResetPassword]    Script Date: 18-12-2025 23:49:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [aut].[usp_ResetPassword]
    @Token NVARCHAR(200),
    @NewPasswordHash NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate token
    IF NOT EXISTS (
        SELECT 1
        FROM [MES_AUT].[aut].[User]
        WHERE ResetPasswordToken = @Token
          AND ResetPasswordExpiry > GETUTCDATE()
          AND Active = 1
    )
    BEGIN
        SELECT 0; -- ❌ Invalid or expired token
        RETURN;
    END

    -- Update password and clear token
    UPDATE [MES_AUT].[aut].[User]
    SET
        PasswordHash = @NewPasswordHash,
        ResetPasswordToken = NULL,
        ResetPasswordExpiry = NULL
    WHERE ResetPasswordToken = @Token
      AND Active = 1;

    SELECT 1; -- ✅ Success
END
GO
/****** Object:  StoredProcedure [aut].[usp_SetUserActiveStatus]    Script Date: 18-12-2025 23:49:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_SetUserActiveStatus]
    @UserID INT,
    @Active BIT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if user exists (optional, for validation)
    IF EXISTS (SELECT 1 FROM [aut].[User] WHERE UserId = @UserID)
    BEGIN
        UPDATE [aut].[User]
        SET Active = @Active
        WHERE UserId = @UserID;
    END
    ELSE
    BEGIN
        -- Optional: Return an error message
        RAISERROR('User with UserId %d not found.', 16, 1, @UserID);
    END
END
GO
/****** Object:  StoredProcedure [aut].[usp_UpdateLastLogin]    Script Date: 18-12-2025 23:49:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [aut].[usp_UpdateLastLogin]
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [MES_AUT].[aut].[User]
    SET LastLogin = GETDATE()
    WHERE UserID = @UserID
      AND Active = 1;
END
GO
/****** Object:  StoredProcedure [aut].[usp_UpdateUser]    Script Date: 18-12-2025 23:49:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_UpdateUser]
    @UserID INT,
    @UserName       NVARCHAR(100),    
    @FirstName      NVARCHAR(100) = NULL,
    @LastName       NVARCHAR(100) = NULL,
    @Email          NVARCHAR(255),
    @Photo          VARBINARY(MAX) = NULL,
    @RoleID         INT,
    @Active         BIT = 1,           
    @TimeZone       INT = NULL,
    @TwoFactor      BIT = 0,          
    @Teams          NVARCHAR(255) = NULL,
    @PhoneNumber    NVARCHAR(20) = NULL,
    @Priority       INT = NULL,
    @Status         INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [aut].[User]
    SET 
        UserName        = @UserName,        
        FirstName       = @FirstName,
        LastName        = @LastName,
        Email           = @Email,
        Photo           = @Photo,
        RoleID          = @RoleID,
        Active          = @Active,
        TimeZone        = @TimeZone,
        TwoFactorEnabled= @TwoFactor,
        TeamsProjects   = @Teams,
        PhoneNumber     = @PhoneNumber,
        Priority        = @Priority,
        Status          = @Status
    WHERE UserID = @UserID;
END
GO

-- 2. Finally drop the column
IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE name = 'Password'
      AND object_id = OBJECT_ID('[aut].[User]')
)
BEGIN
    ALTER TABLE [aut].[User]
    DROP COLUMN [Password];
END
GO
