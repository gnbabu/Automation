using System.Net;
using System.Text;

namespace AutomationAPI.Repositories
{
    /// <summary>
    /// Builds branded HTML email bodies - Outlook desktop is the primary client
    /// (confirmed with the user), so this is deliberately designed around Outlook's
    /// Word-based rendering engine rather than "generically email-safe":
    ///   - Solid brand colors only, never CSS gradients (Outlook doesn't render them) -
    ///     even though the Portal itself uses a gradient header on-screen.
    ///   - Table-based layout, every style inline, no flexbox/grid/float.
    ///   - Square corners everywhere (no border-radius) - designed to look intentional
    ///     rather than "a rounded thing that rendered wrong".
    ///   - Web-safe fonts only (Arial/Helvetica; Consolas/Courier New for the error
    ///     block) - Outlook ignores @font-face/web fonts entirely.
    ///   - MSO conditional comments for the CTA button, so Outlook gets a real
    ///     click-area (VML) instead of a shrink-wrapped inline link.
    ///   - No external image assets - a plain colored table-cell "badge" with a single
    ///     HTML-entity icon character instead, since many clients (Outlook included)
    ///     block remote images by default.
    /// Colors/wordmark pulled directly from the Portal's own CSS (.btn-purple's
    /// #5c3c9e, the sidebar's "Automation" wordmark, the header gradient's dark navy
    /// #1a1c2e as the solid header band here) - not invented.
    /// </summary>
    public static class EmailTemplateBuilder
    {
        private const string FontFamily = "Arial, Helvetica, sans-serif";
        private const string HeaderBg = "#1a1c2e";
        private const string BrandPurple = "#5c3c9e";

        public static string BuildShell(
            string accentColor,
            string icon,
            string heading,
            string messageHtml,
            IEnumerable<(string Label, string Value)> facts,
            string? errorBlock,
            string ctaText,
            string ctaUrl)
        {
            var factsHtml = new StringBuilder();
            foreach (var (label, value) in facts)
            {
                factsHtml.Append($@"
        <tr>
          <td style=""padding:10px 16px; font-size:13px; color:#6b6b78; font-family:{FontFamily}; width:140px; border-bottom:1px solid #e2e2ea;"">{WebUtility.HtmlEncode(label)}</td>
          <td style=""padding:10px 16px; font-size:13px; color:#1a1c2e; font-family:{FontFamily}; font-weight:bold; border-bottom:1px solid #e2e2ea;"">{WebUtility.HtmlEncode(value)}</td>
        </tr>");
            }

            var errorHtml = string.IsNullOrWhiteSpace(errorBlock) ? "" : $@"
      <tr>
        <td style=""padding:16px 32px 0 32px;"">
          <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#2b2b36;"">
            <tr>
              <td style=""padding:12px 16px; font-family:Consolas,'Courier New',monospace; font-size:12px; color:#f2f2f2; white-space:pre-wrap;"">{WebUtility.HtmlEncode(errorBlock)}</td>
            </tr>
          </table>
        </td>
      </tr>";

            return $@"<!DOCTYPE html>
<html>
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
<!--[if mso]>
<style type=""text/css"">
table {{border-collapse:collapse;}}
</style>
<![endif]-->
</head>
<body style=""margin:0; padding:0; background-color:#f4f4f7;"">
<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f4f7;"">
  <tr>
    <td align=""center"" style=""padding:24px 12px;"">
      <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff; max-width:600px; width:100%;"">
        <tr>
          <td style=""background-color:{HeaderBg}; padding:20px 32px;"">
            <span style=""color:#ffffff; font-size:20px; font-weight:bold; font-family:{FontFamily};"">OHPNM Automation Portal</span>
          </td>
        </tr>
        <tr>
          <td style=""background-color:{accentColor}; height:6px; line-height:6px; font-size:1px;"">&nbsp;</td>
        </tr>
        <tr>
          <td style=""padding:32px 32px 8px 32px;"">
            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"">
              <tr>
                <td style=""width:40px; height:40px; background-color:{accentColor}; text-align:center; vertical-align:middle; font-size:20px; color:#ffffff; font-family:{FontFamily};"">{icon}</td>
                <td style=""padding-left:12px; font-size:20px; font-weight:bold; color:#1a1c2e; font-family:{FontFamily};"">{WebUtility.HtmlEncode(heading)}</td>
              </tr>
            </table>
          </td>
        </tr>
        <tr>
          <td style=""padding:8px 32px 0 32px; font-size:14px; color:#333333; font-family:{FontFamily}; line-height:1.5;"">
            {messageHtml}
          </td>
        </tr>
        <tr>
          <td style=""padding:20px 32px 0 32px;"">
            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f7f7fa; border:1px solid #e2e2ea;"">
{factsHtml}
            </table>
          </td>
        </tr>
{errorHtml}
        <tr>
          <td style=""padding:28px 32px;"" align=""center"">
            <!--[if mso]>
            <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{ctaUrl}"" style=""height:44px;v-text-anchor:middle;width:240px;"" arcsize=""0%"" fillcolor=""{BrandPurple}"" stroke=""f"">
            <w:anchorlock/>
            <center style=""color:#ffffff;font-family:Arial,sans-serif;font-size:14px;font-weight:bold;"">{WebUtility.HtmlEncode(ctaText)}</center>
            </v:roundrect>
            <![endif]-->
            <!--[if !mso]><!-->
            <a href=""{ctaUrl}"" style=""background-color:{BrandPurple}; color:#ffffff; text-decoration:none; font-family:{FontFamily}; font-size:14px; font-weight:bold; padding:14px 28px; display:inline-block;"">{WebUtility.HtmlEncode(ctaText)}</a>
            <!--<![endif]-->
          </td>
        </tr>
        <tr>
          <td style=""background-color:#f0f0f5; padding:16px 32px; text-align:center; font-size:11px; color:#8b8b96; font-family:{FontFamily};"">
            This is an automated message from OHPNM Automation Portal. Please do not reply to this email.
          </td>
        </tr>
      </table>
    </td>
  </tr>
</table>
</body>
</html>";
        }

        public static string BuildReleaseActivatedEmail(string releaseName, string version, string environmentName, string ctaUrl)
            => BuildShell(
                accentColor: "#2e9e5b",
                icon: "&#10003;",
                heading: "Release Ready for Testing",
                messageHtml: $"Release <strong>{WebUtility.HtmlEncode(releaseName)}</strong> has been activated and is now available for testing.",
                facts: new[] { ("Release", releaseName), ("Version", version), ("Environment", environmentName) },
                errorBlock: null,
                ctaText: "Open Release Management",
                ctaUrl: ctaUrl);

        public static string BuildReleaseReadyToActivateEmail(string releaseName, string version, string environmentName, string ctaUrl)
            => BuildShell(
                accentColor: "#2f6fed",
                icon: "&#9889;",
                heading: "Release Ready to Activate",
                messageHtml: $"Release <strong>{WebUtility.HtmlEncode(releaseName)}</strong> now has usable DLLs in its release folder and is ready to be activated.",
                facts: new[] { ("Release", releaseName), ("Version", version), ("Environment", environmentName) },
                errorBlock: null,
                ctaText: "Review & Activate",
                ctaUrl: ctaUrl);

        public static string BuildScheduledFailureEmail(string testCaseId, string environmentName, DateTime failedAt, string errorMessage, string ctaUrl)
            => BuildShell(
                accentColor: "#d64545",
                icon: "&#10007;",
                heading: "Scheduled Test Failed",
                messageHtml: $"Scheduled test case <strong>{WebUtility.HtmlEncode(testCaseId)}</strong> failed. You can retry it directly from the Test Case Execution Panel.",
                facts: new[] { ("Test Case", testCaseId), ("Environment", environmentName), ("Failed At", failedAt.ToString("g")) },
                errorBlock: errorMessage,
                ctaText: "View in Test Case Execution Panel",
                ctaUrl: ctaUrl);
    }
}
