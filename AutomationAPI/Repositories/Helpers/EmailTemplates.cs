namespace AutomationAPI.Repositories.Helpers
{
    public static class EmailTemplates
    {
        public static string BuildForgotUsernameEmail(string username)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <title>Forgot Username</title>
            </head>
            <body style='margin:0;padding:0;background-color:#f4f6f8;font-family:Arial,Helvetica,sans-serif;'>

            <table width='100%' cellpadding='0' cellspacing='0'>
                <tr>
                    <td align='center' style='padding:30px 0;'>

                        <table width='600' cellpadding='0' cellspacing='0'
                            style='background-color:#ffffff;border-radius:6px;
                            box-shadow:0 2px 8px rgba(0,0,0,0.05);'>

                            <tr>
                                <td style='padding:30px;'>

                                    <p style='font-size:16px;color:#333;margin-top:0;'>
                                        Hello,
                                    </p>

                                    <p style='font-size:15px;color:#555;line-height:1.6;'>
                                        You requested your username for the
                                        <strong>Automation Portal</strong>.
                                    </p>

                                    <p style='font-size:15px;color:#555;line-height:1.6;'>
                                        <strong>Your Username:</strong>
                                    </p>

                                    <p style='font-size:20px;color:#2f7d7b;font-weight:bold;margin:10px 0 20px 0;'>
                                        {username}
                                    </p>

                                    <p style='font-size:14px;color:#555;line-height:1.6;'>
                                        If you did not request this, please ignore this email.
                                    </p>

                                    <hr style='border:none;border-top:1px solid #e5e7eb;margin:30px 0;' />

                                    <p style='font-size:14px;color:#555;margin-bottom:4px;'>
                                        Regards,
                                    </p>
                                    <p style='font-size:14px;color:#555;margin-top:0;'>
                                        <strong>Automation Portal Team</strong>
                                    </p>

                                    <p style='font-size:12px;color:#888;margin-top:20px;'>
                                        This is an automated email. Please do not reply.
                                    </p>

                                </td>
                            </tr>
                        </table>

                    </td>
                </tr>
            </table>

            </body>
            </html>";
        }

        public static string ResetPassword(string link, string username)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <title>Password Reset</title>
                </head>
                <body style='margin:0;padding:0;background-color:#f4f6f8;font-family:Arial,Helvetica,sans-serif;'>

                <table width='100%' cellpadding='0' cellspacing='0'>
                    <tr>
                        <td align='center' style='padding:30px 0;'>

                            <table width='600' cellpadding='0' cellspacing='0'
                                style='background-color:#ffffff;border-radius:6px;
                                box-shadow:0 2px 8px rgba(0,0,0,0.05);'>

                                <tr>
                                    <td style='padding:30px;'>

                                        <p style='font-size:16px;color:#333;margin-top:0;'>
                                            Hello <strong>{username}</strong>,
                                        </p>

                                        <p style='font-size:15px;color:#555;line-height:1.6;'>
                                            Someone (hopefully you!) has requested to change your
                                            <strong>Automation Portal</strong> password.
                                            Please click the button below to change your password now.
                                        </p>

                                        <!-- Email-safe button -->
                                        <table cellpadding='0' cellspacing='0' border='0' align='center' style='margin:30px 0;'>
                                            <tr>
                                                <td align='center' bgcolor='#2f7d7b' style='border-radius:4px;'>
                                                    <a href='{link}'
                                                       style='display:block;
                                                       padding:14px 28px;
                                                       font-size:15px;
                                                       font-family:Arial,Helvetica,sans-serif;
                                                       color:#ffffff;
                                                       text-decoration:none;
                                                       font-weight:bold;'>
                                                        Change My Password
                                                    </a>
                                                </td>
                                            </tr>
                                        </table>

                                        <p style='font-size:14px;color:#555;line-height:1.6;'>
                                            If you didn’t make this request, please disregard this email.
                                        </p>

                                        <p style='font-size:14px;color:#555;line-height:1.6;'>
                                            Your password will not change unless you click the link above
                                            and create a new one.
                                            <strong>This link will expire in 30 minutes.</strong>
                                        </p>

                                        <hr style='border:none;border-top:1px solid #e5e7eb;margin:30px 0;' />

                                        <p style='font-size:14px;color:#555;margin-bottom:4px;'>
                                            Sincerely,
                                        </p>
                                        <p style='font-size:14px;color:#555;margin-top:0;'>
                                            <strong>Automation Portal Team</strong>
                                        </p>

                                    </td>
                                </tr>
                            </table>

                        </td>
                    </tr>
                </table>

                </body>
                </html>";
        }

    }
}
