namespace AutomationAPI.Repositories.Helpers
{
    public static class EmailTemplates
    {
        public static string BuildForgotUsernameEmail(string username)
        {
            return $@"<!DOCTYPE html>
                    <html>
                    <body style='font-family:Arial;background:#f4f6f8;padding:20px;'>
                        <div style='max-width:600px;margin:auto;background:#fff;padding:24px;border-radius:8px;'>
                            <h2 style='color:#0d6efd;'>OHPNM Automation Portal</h2>

                            <p>Hello,</p>

                            <p>You requested your username.</p>

                            <p>
                                <strong>Your Username:</strong><br/>
                                <span style='font-size:18px;color:#0d6efd;'>{username}</span>
                            </p>

                            <p>If you did not request this, please ignore this email.</p>

                            <hr/>
                            <small>This is an automated email. Do not reply.</small>
                        </div>
                    </body>
                    </html>";
        }

        public static string ResetPassword(string link)
        {
            return $@"
                    <h3>Password Reset Request</h3>
                    <p>Click the link below to reset your password:</p>
                    <a href='{link}'>Reset Password</a>
                    <p>This link expires in 30 minutes.</p>
                    <br/>
                    <p>OHPNM Automation Portal</p>";
        }

    }
}
