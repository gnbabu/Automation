CREATE OR ALTER PROCEDURE [aut].[usp_UpdateLastLogin]
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
