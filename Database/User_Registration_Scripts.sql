
GO
/****** Object:  StoredProcedure [aut].[usp_CreateUser]    Script Date: 23-11-2025 10:07:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [aut].[usp_CreateUser]
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
    @PhoneNumber    NVARCHAR(20) = NULL
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
        PhoneNumber
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
        @PhoneNumber
    );

    SELECT SCOPE_IDENTITY() AS NewUserID;
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetPriorityStatus]    Script Date: 23-11-2025 10:07:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetPriorityStatus]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        PriorityID,
        PriorityName
    FROM [aut].[PriorityStatus]
    ORDER BY PriorityName;
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetTimeZones]    Script Date: 23-11-2025 10:07:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetTimeZones]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        TimeZoneID,
        TimeZoneName,
        UTCOffsetMinutes,
        Description
    FROM [aut].[TimeZone]
    ORDER BY TimeZoneName;
END
GO
/****** Object:  StoredProcedure [aut].[usp_GetUserStatuses]    Script Date: 23-11-2025 10:07:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_GetUserStatuses]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        StatusID,
        StatusName
    FROM [aut].[UserStatus]
    ORDER BY StatusName;
END
GO
/****** Object:  StoredProcedure [aut].[usp_RegisterUser]    Script Date: 23-11-2025 10:07:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [aut].[usp_RegisterUser]
    @Username NVARCHAR(100),
    @Email NVARCHAR(255),
    @Password NVARCHAR(255)
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
        Password,
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
        @Password,
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
