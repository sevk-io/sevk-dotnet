using System.Collections.Generic;

namespace Sevk.Markup;

/// <summary>
/// Font configuration for email
/// </summary>
public class FontConfig
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";
}

/// <summary>
/// Head settings for email generation
/// </summary>
public class EmailHeadSettings
{
    public string Title { get; set; } = "";
    public string PreviewText { get; set; } = "";
    public string Styles { get; set; } = "";
    public List<FontConfig> Fonts { get; set; } = new();
    public string Lang { get; set; } = "";
    public string Dir { get; set; } = "";
}

/// <summary>
/// Parsed email content
/// </summary>
public class ParsedEmailContent
{
    public string Body { get; set; } = "";
    public EmailHeadSettings HeadSettings { get; set; } = new();
}

/// <summary>
/// sevk Markup Renderer - converts sevk markup to email-compatible HTML
/// </summary>
public static partial class Renderer
{
    /// <summary>
    /// Converts sevk markup into complete, email-compatible HTML.
    /// </summary>
    public static string Render(string? markup, EmailHeadSettings? headSettings = null)
    {
        var parsed = ParseEmailHTML(markup ?? "");
        var settings = headSettings ?? parsed.HeadSettings;
        var contentToProcess = parsed.Body;

        var normalized = NormalizeMarkup(contentToProcess);
        var (processed, gapStyles) = ProcessMarkup(normalized);

        var gapStyleTag = !string.IsNullOrEmpty(gapStyles) ? $"<style type=\"text/css\">{gapStyles}</style>" : "";
        var titleTag = !string.IsNullOrEmpty(settings.Title) ? $"<title>{settings.Title}</title>" : "";
        var fontLinks = GenerateFontLinks(settings.Fonts);
        var customStyles = !string.IsNullOrEmpty(settings.Styles) ? $"<style type=\"text/css\">{settings.Styles}</style>" : "";
        var previewText = !string.IsNullOrEmpty(settings.PreviewText)
            ? $"<div style=\"display:none;font-size:1px;color:#ffffff;line-height:1px;max-height:0px;max-width:0px;opacity:0;overflow:hidden;\">{settings.PreviewText}</div>"
            : "";

        var lang = !string.IsNullOrEmpty(settings.Lang) ? settings.Lang : "en";
        var dir = !string.IsNullOrEmpty(settings.Dir) ? settings.Dir : "ltr";

        return $@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html lang=""{lang}"" dir=""{dir}"" xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">
<head>
<meta content=""text/html; charset=UTF-8"" http-equiv=""Content-Type""/>
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
<meta name=""x-apple-disable-message-reformatting""/>
<meta content=""IE=edge"" http-equiv=""X-UA-Compatible""/>
<meta name=""format-detection"" content=""telephone=no,address=no,email=no,date=no,url=no""/>
<!--[if mso]>
<noscript>
<xml>
<o:OfficeDocumentSettings>
<o:AllowPNG/>
<o:PixelsPerInch>96</o:PixelsPerInch>
</o:OfficeDocumentSettings>
</xml>
</noscript>
<![endif]-->
<style type=""text/css"">
#outlook a {{ padding: 0; }}
body {{ margin: 0; padding: 0; -webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; }}
table, td {{ border-collapse: collapse; mso-table-lspace: 0pt; mso-table-rspace: 0pt; }}
.sevk-row-table {{ border-collapse: separate !important; }}
img {{ border: 0; height: auto; line-height: 100%; outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; }}
@media only screen and (max-width: 479px) {{
  .sevk-row-table {{ width: 100% !important; }}
  .sevk-column {{ display: block !important; width: 100% !important; max-width: 100% !important; box-sizing: border-box !important; }}
}}
</style>
{gapStyleTag}
{titleTag}
{fontLinks}
{customStyles}
</head>
<body style=""margin:0;padding:0;word-spacing:normal;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;font-family:ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;"">
<div aria-roledescription=""email"" role=""article"">
{previewText}
{processed}
</div>
</body>
</html>";
    }
}
