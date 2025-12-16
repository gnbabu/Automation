CREATE OR ALTER PROCEDURE [aut].[usp_ResetPassword]
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
        SELECT 0; -- Invalid or expired token
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

    SELECT 1; -- Success
END
GO
