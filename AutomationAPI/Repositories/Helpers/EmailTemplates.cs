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
                    style='background-color:#ffffff;border-radius:6px;'>

                    <tr>
                    <td style='padding:30px;'>

                    <p style='font-size:16px;color:#333;'>Hello,</p>

                    <p style='font-size:15px;color:#555;line-height:1.6;'>
                    You requested your username for the <strong>Automation Portal</strong>.
                    </p>

                    <p style='font-size:15px;color:#555;'><strong>Your Username:</strong></p>

                    <p style='font-size:20px;color:#2f7d7b;font-weight:bold;'>
                    {username}
                    </p>

                    <p style='font-size:14px;color:#555;'>
                    If you did not request this, please ignore this email.
                    </p>

                    <hr style='border:none;border-top:1px solid #e5e7eb;margin:30px 0;' />

                    <p style='font-size:14px;color:#555;'><strong>Automation Portal Team</strong></p>
                    <p style='font-size:12px;color:#888;'>This is an automated email.</p>

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
                    style='background-color:#ffffff;border-radius:6px;'>

                    <tr>
                    <td style='padding:30px;'>

                    <p style='font-size:16px;color:#333;'>
                    Hello <strong>{username}</strong>,
                    </p>

                    <p style='font-size:15px;color:#555;line-height:1.6;'>
                    You requested to reset your <strong>Automation Portal</strong> password.
                    </p>

                    <!-- OUTLOOK SAFE BUTTON -->
                    <table align='center' cellpadding='0' cellspacing='0' border='0' style='margin:30px 0;'>
                    <tr>
                    <td align='center'>

                    <!--[if mso]>
                    <v:roundrect xmlns:v='urn:schemas-microsoft-com:vml'
                    href='{link}'
                    style='height:45px;v-text-anchor:middle;width:220px;'
                    arcsize='10%'
                    stroke='f'
                    fillcolor='#2f7d7b'>
                    <w:anchorlock/>
                    <center style='color:#ffffff;font-family:Arial,Helvetica,sans-serif;
                    font-size:15px;font-weight:bold;'>
                    Change My Password
                    </center>
                    </v:roundrect>
                    <![endif]-->

                    <!--[if !mso]><!-- -->
                    <a href='{link}'
                    style='display:inline-block;background-color:#2f7d7b;color:#ffffff;
                    padding:14px 28px;font-size:15px;font-weight:bold;
                    text-decoration:none;border-radius:4px;'>
                    Change My Password
                    </a>
                    <!--<![endif]-->

                    </td>
                    </tr>
                    </table>

                    <p style='font-size:14px;color:#555;line-height:1.6;'>
                    This link expires in <strong>30 minutes</strong>.
                    </p>

                    <p style='font-size:13px;color:#555;'>
                    If the button doesn’t work, copy this link:<br/>
                    <a href='{link}'>{link}</a>
                    </p>

                    <hr style='border:none;border-top:1px solid #e5e7eb;margin:30px 0;' />

                    <p style='font-size:14px;color:#555;'>
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
