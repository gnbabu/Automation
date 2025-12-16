CREATE OR ALTER PROCEDURE [aut].[usp_ForgotPassword]
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
        SELECT 0; -- User not found
        RETURN;
    END

    UPDATE [MES_AUT].[aut].[User]
    SET
        ResetPasswordToken = @Token,
        ResetPasswordExpiry = @Expiry
    WHERE Email = @Email
      AND Active = 1;

    SELECT 1; -- Success
END
GO
