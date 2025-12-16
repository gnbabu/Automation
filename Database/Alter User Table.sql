ALTER TABLE [aut].[User]
ADD
    ResetPasswordToken NVARCHAR(200) NULL,
    ResetPasswordExpiry DATETIME NULL;
GO


CREATE NONCLUSTERED INDEX IX_User_ResetPasswordToken
ON [aut].[User] (ResetPasswordToken)
WHERE ResetPasswordToken IS NOT NULL;
GO
