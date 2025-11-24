
GO
/****** Object:  StoredProcedure [aut].[usp_UpdateUser]    Script Date: 24-11-2025 23:22:33 ******/
DROP PROCEDURE [aut].[usp_UpdateUser]
GO
/****** Object:  StoredProcedure [aut].[usp_CreateUser]    Script Date: 24-11-2025 23:22:33 ******/
DROP PROCEDURE [aut].[usp_CreateUser]
GO
/****** Object:  StoredProcedure [aut].[usp_CreateUser]    Script Date: 24-11-2025 23:22:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_CreateUser]
    @UserName       NVARCHAR(100),
    @Password       NVARCHAR(255),
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
        Password,
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
        @Password,
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
/****** Object:  StoredProcedure [aut].[usp_UpdateUser]    Script Date: 24-11-2025 23:22:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [aut].[usp_UpdateUser]
    @UserID INT,
    @UserName       NVARCHAR(100),
    @Password       NVARCHAR(255) = NULL,
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
        Password        = COALESCE(@Password, Password),
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
