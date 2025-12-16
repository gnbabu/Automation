CREATE OR ALTER PROCEDURE [aut].[usp_GetUsernameByEmail]
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
