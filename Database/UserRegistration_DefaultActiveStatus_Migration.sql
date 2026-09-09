-- UserRegistration_DefaultActiveStatus_Migration.sql
-- Idempotent (safe to re-run). Fixes aut.usp_RegisterUser's default Status lookup.
--
-- aut.User has two separate status concepts: the Active bit (already hard-coded to 1
-- on registration - it's what actually gates login, see AuthService.Login) and a
-- separate Status FK to aut.UserStatus (Active/Suspended/Pending), shown as a badge in
-- the Users grid. usp_RegisterUser previously defaulted the latter to 'Suspended',
-- which is not an intentional approval gate - it just meant a newly registered user,
-- despite already being able to log in (Active = 1), showed up in the Users grid with
-- a misleading "Suspended" badge. This changes the default lookup to 'Active' so the
-- badge matches reality. The Active bit itself is unchanged (already correct).

SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE [aut].[usp_RegisterUser]
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

    -- Default Status = Active (matches the Active bit below - not an approval gate)
    SELECT @StatusID = StatusID
    FROM [aut].[UserStatus]
    WHERE StatusName = 'Active';

    IF (@StatusID IS NULL)
    BEGIN
        THROW 50002, 'Default status ''Active'' not found in UserStatus table.', 1;
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
