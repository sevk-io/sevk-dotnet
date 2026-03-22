using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

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
/// Sevk Markup Renderer - converts Sevk markup to email-compatible HTML
/// </summary>
public static class Renderer
{
    /// <summary>
    /// Generate email HTML from Sevk markup
    /// </summary>
    public static string Render(string? markup, EmailHeadSettings? headSettings = null)
    {
        if (string.IsNullOrEmpty(markup)) return "";

        // Always parse to extract clean body content (strips <mail>/<head> wrapper tags)
        var parsed = ParseEmailHTML(markup);
        var settings = headSettings ?? parsed.HeadSettings;
        var contentToProcess = parsed.Body;

        var normalized = NormalizeMarkup(contentToProcess);
        var processed = ProcessMarkup(normalized);

        // Build head content
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
  .sevk-column {{ display: block !important; width: 100% !important; max-width: 100% !important; }}
}}
</style>
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

    private static string NormalizeMarkup(string content)
    {
        var result = content;

        // Replace <link> with <sevk-link>
        if (result.Contains("<link"))
        {
            result = Regex.Replace(result, @"<link\s+href=", "<sevk-link href=", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"</link>", "</sevk-link>", RegexOptions.IgnoreCase);
        }

        if (!result.Contains("<sevk-email") && !result.Contains("<email") && !result.Contains("<mail"))
        {
            result = $"<mail><body>{result}</body></mail>";
        }

        return result;
    }

    private static string GenerateFontLinks(List<FontConfig> fonts)
    {
        var links = new List<string>();
        foreach (var font in fonts)
        {
            links.Add($"<link href=\"{font.Url}\" rel=\"stylesheet\" type=\"text/css\" />");
        }
        return string.Join("\n", links);
    }

    /// <summary>
    /// Parse email HTML and extract head settings
    /// </summary>
    public static ParsedEmailContent ParseEmailHTML(string content)
    {
        if (content.Contains("<email>") || content.Contains("<email ") ||
            content.Contains("<mail>") || content.Contains("<mail "))
        {
            return ParseSevkMarkup(content);
        }
        return new ParsedEmailContent { Body = content, HeadSettings = new EmailHeadSettings() };
    }

    private static ParsedEmailContent ParseSevkMarkup(string content)
    {
        var headSettings = new EmailHeadSettings();

        // Parse lang and dir from <mail> or <email> root tag
        var rootMatch = Regex.Match(content, @"<(?:email|mail)([^>]*)>", RegexOptions.IgnoreCase);
        if (rootMatch.Success)
        {
            var rootAttrs = rootMatch.Groups[1].Value;
            var langMatch = Regex.Match(rootAttrs, @"lang=[""']([^""']*)[""']", RegexOptions.IgnoreCase);
            var dirMatch = Regex.Match(rootAttrs, @"dir=[""']([^""']*)[""']", RegexOptions.IgnoreCase);
            if (langMatch.Success) headSettings.Lang = langMatch.Groups[1].Value;
            if (dirMatch.Success) headSettings.Dir = dirMatch.Groups[1].Value;
        }

        // Extract title
        var titleMatch = Regex.Match(content, @"<title[^>]*>([\s\S]*?)</title>", RegexOptions.IgnoreCase);
        if (titleMatch.Success)
        {
            headSettings.Title = titleMatch.Groups[1].Value.Trim();
        }

        // Extract preview
        var previewMatch = Regex.Match(content, @"<preview[^>]*>([\s\S]*?)</preview>", RegexOptions.IgnoreCase);
        if (previewMatch.Success)
        {
            headSettings.PreviewText = previewMatch.Groups[1].Value.Trim();
        }

        // Extract styles
        var styleMatch = Regex.Match(content, @"<style[^>]*>([\s\S]*?)</style>", RegexOptions.IgnoreCase);
        if (styleMatch.Success)
        {
            headSettings.Styles = styleMatch.Groups[1].Value.Trim();
        }

        // Extract fonts
        var fontMatches = Regex.Matches(content, @"<font[^>]*name=[""']([^""']*)[""'][^>]*url=[""']([^""']*)[""'][^>]*/?\s*>", RegexOptions.IgnoreCase);
        for (int i = 0; i < fontMatches.Count; i++)
        {
            var match = fontMatches[i];
            headSettings.Fonts.Add(new FontConfig
            {
                Id = $"font-{i}",
                Name = match.Groups[1].Value,
                Url = match.Groups[2].Value
            });
        }

        // Extract body
        string body;
        var bodyMatch = Regex.Match(content, @"<body[^>]*>([\s\S]*?)</body>", RegexOptions.IgnoreCase);
        if (bodyMatch.Success)
        {
            body = bodyMatch.Groups[1].Value.Trim();
        }
        else
        {
            body = content;
            var patterns = new[]
            {
                @"<email[^>]*>", @"</email>",
                @"<mail[^>]*>", @"</mail>",
                @"<head[^>]*>[\s\S]*?</head>",
                @"<title[^>]*>[\s\S]*?</title>",
                @"<preview[^>]*>[\s\S]*?</preview>",
                @"<style[^>]*>[\s\S]*?</style>",
                @"<font[^>]*>[\s\S]*?</font>",
                @"<font[^>]*/?>",
            };
            foreach (var pattern in patterns)
            {
                body = Regex.Replace(body, pattern, "", RegexOptions.IgnoreCase);
            }
            body = body.Trim();
        }

        return new ParsedEmailContent { Body = body, HeadSettings = headSettings };
    }

    private static string ProcessMarkup(string content)
    {
        var result = content;

        // Process block tags BEFORE other tags
        result = ProcessTag(result, "block", (attrs, inner) => ProcessBlockTag(attrs, inner));

        // Process section tags (both sevk-section and section)
        result = ProcessTag(result, "sevk-section", (attrs, inner) =>
        {
            var style = ExtractAllStyleAttributes(attrs);
            var styleStr = StyleToString(style);
            return $@"<table align=""center"" width=""100%"" border=""0"" cellPadding=""0"" cellSpacing=""0"" role=""presentation"" style=""{styleStr}"">
<tbody>
<tr>
<td>{inner}</td>
</tr>
</tbody>
</table>";
        });
        result = ProcessTag(result, "section", (attrs, inner) =>
        {
            var style = ExtractAllStyleAttributes(attrs);
            var styleStr = StyleToString(style);
            return $@"<table align=""center"" width=""100%"" border=""0"" cellPadding=""0"" cellSpacing=""0"" role=""presentation"" style=""{styleStr}"">
<tbody>
<tr>
<td>{inner}</td>
</tr>
</tbody>
</table>";
        });

        // Process row tags
        var rowCounter = 0;
        var currentRowGap = 0;
        result = ProcessTag(result, "row", (attrs, inner) =>
        {
            var gap = attrs.GetValueOrDefault("gap", "0");
            var style = ExtractAllStyleAttributes(attrs);
            style.Remove("gap");
            var styleStr = StyleToString(style);
            var gapPx = gap.Replace("px", "");
            var gapNum = int.Parse(gapPx);
            var rowId = $"sevk-row-{rowCounter++}";
            currentRowGap = gapNum;

            // Assign equal widths to columns if more than one
            var processedInner = inner;
            var columnMatches = Regex.Matches(processedInner, @"class=""sevk-column""");
            var columnCount = columnMatches.Count;
            if (columnCount > 1)
            {
                var equalWidth = $"{Math.Floor(100.0 / columnCount)}%";
                processedInner = Regex.Replace(processedInner, @"<td class=""sevk-column"" style=""([^""]*)""", (m) =>
                {
                    var existingStyle = m.Groups[1].Value;
                    if (existingStyle.Contains("width:")) return m.Value;
                    return $@"<td class=""sevk-column"" style=""width:{equalWidth};{existingStyle}""";
                });
            }

            var gapStyle = gapNum > 0 ? $"<style>@media only screen and (max-width:479px){{.{rowId} > tbody > tr > td{{margin-bottom:{gapPx}px !important;padding-left:0 !important;padding-right:0 !important;}}.{rowId} > tbody > tr > td:last-child{{margin-bottom:0 !important;}}}}</style>" : "";
            return $@"{gapStyle}<table class=""sevk-row-table {rowId}"" align=""center"" width=""100%"" border=""0"" cellPadding=""0"" cellSpacing=""0"" role=""presentation"" style=""{styleStr}"">
<tbody style=""width:100%"">
<tr style=""width:100%"">{processedInner}</tr>
</tbody>
</table>";
        });

        // Process column tags - apply half-gap padding from parent row and vertical-align:top
        result = ProcessTag(result, "column", (attrs, inner) =>
        {
            var style = ExtractAllStyleAttributes(attrs);
            if (!style.ContainsKey("vertical-align"))
                style["vertical-align"] = "top";
            if (currentRowGap > 0)
            {
                var halfGap = currentRowGap / 2.0;
                var halfGapStr = halfGap == (int)halfGap ? ((int)halfGap).ToString() : halfGap.ToString();
                if (!style.ContainsKey("padding-left"))
                    style["padding-left"] = $"{halfGapStr}px";
                if (!style.ContainsKey("padding-right"))
                    style["padding-right"] = $"{halfGapStr}px";
            }
            var styleStr = StyleToString(style);
            return $@"<td class=""sevk-column"" style=""{styleStr}"">{inner}</td>";
        });

        // Process container tags (both sevk-container and container)
        result = ProcessTag(result, "sevk-container", (attrs, inner) => ProcessContainer(attrs, inner));
        result = ProcessTag(result, "container", (attrs, inner) => ProcessContainer(attrs, inner));

        // Process heading tags (both sevk-heading and heading)
        result = ProcessTag(result, "sevk-heading", (attrs, inner) =>
        {
            var level = attrs.GetValueOrDefault("level", "1");
            var style = ExtractAllStyleAttributes(attrs);
            if (!style.ContainsKey("margin")) style["margin"] = "0";
            var styleStr = StyleToString(style);
            return $@"<h{level} style=""{styleStr}"">{inner}</h{level}>";
        });
        result = ProcessTag(result, "heading", (attrs, inner) =>
        {
            var level = attrs.GetValueOrDefault("level", "1");
            var style = ExtractAllStyleAttributes(attrs);
            if (!style.ContainsKey("margin")) style["margin"] = "0";
            var styleStr = StyleToString(style);
            return $@"<h{level} style=""{styleStr}"">{inner}</h{level}>";
        });

        // Process paragraph tags
        result = ProcessTag(result, "paragraph", (attrs, inner) =>
        {
            var style = ExtractAllStyleAttributes(attrs);
            if (!style.ContainsKey("margin")) style["margin"] = "0";
            var styleStr = StyleToString(style);
            return $@"<p style=""{styleStr}"">{inner}</p>";
        });

        // Process text tags
        result = ProcessTag(result, "text", (attrs, inner) =>
        {
            var style = ExtractAllStyleAttributes(attrs);
            var styleStr = StyleToString(style);
            return $@"<span style=""{styleStr}"">{inner}</span>";
        });

        // Process button tags with MSO compatibility (both sevk-button and button)
        result = ProcessTag(result, "sevk-button", (attrs, inner) =>
        {
            return ProcessButton(attrs, inner);
        });
        result = ProcessTag(result, "button", (attrs, inner) =>
        {
            return ProcessButton(attrs, inner);
        });

        // Process image tags (both sevk-image and image)
        result = ProcessTag(result, "sevk-image", (attrs, _) =>
        {
            var src = attrs.GetValueOrDefault("src", "");
            var alt = attrs.GetValueOrDefault("alt", "");
            attrs.TryGetValue("width", out var width);
            attrs.TryGetValue("height", out var height);

            var style = ExtractAllStyleAttributes(attrs);
            if (!style.ContainsKey("vertical-align")) style["vertical-align"] = "middle";
            if (!style.ContainsKey("max-width")) style["max-width"] = "100%";
            if (!style.ContainsKey("outline")) style["outline"] = "none";
            if (!style.ContainsKey("border")) style["border"] = "none";
            if (!style.ContainsKey("text-decoration")) style["text-decoration"] = "none";

            var styleStr = StyleToString(style);
            var widthAttr = width != null ? $@" width=""{width.Replace("px", "")}""" : "";
            var heightAttr = height != null ? $@" height=""{height.Replace("px", "")}""" : "";

            return $@"<img src=""{src}"" alt=""{alt}""{widthAttr}{heightAttr} style=""{styleStr}"" />";
        });
        result = ProcessTag(result, "image", (attrs, _) =>
        {
            var src = attrs.GetValueOrDefault("src", "");
            var alt = attrs.GetValueOrDefault("alt", "");
            attrs.TryGetValue("width", out var width);
            attrs.TryGetValue("height", out var height);

            var style = ExtractAllStyleAttributes(attrs);
            if (!style.ContainsKey("vertical-align")) style["vertical-align"] = "middle";
            if (!style.ContainsKey("max-width")) style["max-width"] = "100%";
            if (!style.ContainsKey("outline")) style["outline"] = "none";
            if (!style.ContainsKey("border")) style["border"] = "none";
            if (!style.ContainsKey("text-decoration")) style["text-decoration"] = "none";

            var styleStr = StyleToString(style);
            var widthAttr = width != null ? $@" width=""{width.Replace("px", "")}""" : "";
            var heightAttr = height != null ? $@" height=""{height.Replace("px", "")}""" : "";

            return $@"<img src=""{src}"" alt=""{alt}""{widthAttr}{heightAttr} style=""{styleStr}"" />";
        });

        // Process divider tags (both sevk-divider and divider)
        result = ProcessTag(result, "sevk-divider", (attrs, _) =>
        {
            var style = ExtractAllStyleAttributes(attrs);
            var styleStr = StyleToString(style);
            var classAttr = (attrs.TryGetValue("class", out var cls) ? cls : null) ?? (attrs.TryGetValue("className", out var clsName) ? clsName : null);
            var classStr = classAttr != null ? $@" class=""{classAttr}""" : "";
            return $@"<div style=""{styleStr}""{classStr}></div>";
        });
        result = ProcessTag(result, "divider", (attrs, _) =>
        {
            var style = ExtractAllStyleAttributes(attrs);
            var styleStr = StyleToString(style);
            var classAttr = (attrs.TryGetValue("class", out var cls) ? cls : null) ?? (attrs.TryGetValue("className", out var clsName) ? clsName : null);
            var classStr = classAttr != null ? $@" class=""{classAttr}""" : "";
            return $@"<div style=""{styleStr}""{classStr}></div>";
        });

        // Clean up stray </divider> closing tags
        result = Regex.Replace(result, @"</divider>", "", RegexOptions.IgnoreCase);

        // Process link tags
        result = ProcessTag(result, "sevk-link", (attrs, inner) =>
        {
            var href = attrs.GetValueOrDefault("href", "#");
            var target = attrs.GetValueOrDefault("target", "_blank");
            var style = ExtractAllStyleAttributes(attrs);
            var styleStr = StyleToString(style);
            return $@"<a href=""{href}"" target=""{target}"" style=""{styleStr}"">{inner}</a>";
        });

        // Process list tags
        result = ProcessTag(result, "list", (attrs, inner) =>
        {
            var listType = attrs.GetValueOrDefault("type", "unordered");
            var tag = listType == "ordered" ? "ol" : "ul";
            var style = ExtractAllStyleAttributes(attrs);
            if (!style.ContainsKey("margin")) style["margin"] = "0";
            if (attrs.TryGetValue("list-style-type", out var lst))
            {
                style["list-style-type"] = lst;
            }
            var styleStr = StyleToString(style);
            var classAttr = (attrs.TryGetValue("class", out var cls) ? cls : null) ?? (attrs.TryGetValue("className", out var clsName) ? clsName : null);
            var classStr = classAttr != null ? $@" class=""{classAttr}""" : "";
            return $@"<{tag} style=""{styleStr}""{classStr}>{inner}</{tag}>";
        });

        // Process list item tags
        result = ProcessTag(result, "li", (attrs, inner) =>
        {
            var style = ExtractAllStyleAttributes(attrs);
            var styleStr = StyleToString(style);
            var classAttr = (attrs.TryGetValue("class", out var cls) ? cls : null) ?? (attrs.TryGetValue("className", out var clsName) ? clsName : null);
            var classStr = classAttr != null ? $@" class=""{classAttr}""" : "";
            return $@"<li style=""{styleStr}""{classStr}>{inner}</li>";
        });

        // Process codeblock tags
        result = ProcessTag(result, "codeblock", (attrs, inner) =>
        {
            return SyntaxHighlighter.ProcessCodeBlock(attrs, inner);
        });

        // Blocks - <block type="..." config="..." />
        result = Regex.Replace(result, @"<block([^>]*)/?\s*>(?:</block>)?", m =>
        {
            var attrs = ParseAttributes(m.Groups[1].Value);
            return ProcessBlockTag(attrs, "");
        }, RegexOptions.IgnoreCase);

        // Clean up stray Sevk closing tags
        var strayClosingTags = new[]
        {
            @"</container>", @"</section>", @"</row>", @"</column>",
            @"</heading>", @"</paragraph>", @"</text>", @"</button>",
            @"</sevk-link>"
        };
        foreach (var tag in strayClosingTags)
        {
            result = Regex.Replace(result, tag, "", RegexOptions.IgnoreCase);
        }

        // Clean up wrapper tags
        var wrapperPatterns = new[]
        {
            @"<sevk-email[^>]*>", @"</sevk-email>",
            @"<sevk-body[^>]*>", @"</sevk-body>",
            @"<email[^>]*>", @"</email>",
            @"<mail[^>]*>", @"</mail>",
            @"<body[^>]*>", @"</body>",
        };
        foreach (var pattern in wrapperPatterns)
        {
            result = Regex.Replace(result, pattern, "", RegexOptions.IgnoreCase);
        }

        return result.Trim();
    }

    /// <summary>
    /// Process container tag - splits visual styles (td) from layout styles (table).
    /// Supports border-radius with border-collapse:separate and responsive width.
    /// </summary>
    private static string ProcessContainer(Dictionary<string, string> attrs, string inner)
    {
        var style = ExtractAllStyleAttributes(attrs);
        var tdStyle = new Dictionary<string, string>();
        var tableStyle = new Dictionary<string, string>();

        // Visual styles on <td>, layout styles on <table>
        var visualKeys = new HashSet<string>
        {
            "background-color", "background-image", "background-size", "background-position", "background-repeat",
            "border", "border-top", "border-right", "border-bottom", "border-left",
            "border-color", "border-width", "border-style",
            "border-radius", "border-top-left-radius", "border-top-right-radius",
            "border-bottom-left-radius", "border-bottom-right-radius",
            "padding", "padding-top", "padding-right", "padding-bottom", "padding-left"
        };

        foreach (var kvp in style)
        {
            if (visualKeys.Contains(kvp.Key))
                tdStyle[kvp.Key] = kvp.Value;
            else
                tableStyle[kvp.Key] = kvp.Value;
        }

        // Add border-collapse: separate when border-radius is used
        var hasBorderRadius = tdStyle.ContainsKey("border-radius") ||
                              tdStyle.ContainsKey("border-top-left-radius") ||
                              tdStyle.ContainsKey("border-top-right-radius") ||
                              tdStyle.ContainsKey("border-bottom-left-radius") ||
                              tdStyle.ContainsKey("border-bottom-right-radius");
        if (hasBorderRadius)
        {
            tableStyle["border-collapse"] = "separate";
        }

        // Make fixed widths responsive: width becomes max-width, width set to 100%
        if (tableStyle.TryGetValue("width", out var widthVal) && widthVal != "100%" && widthVal != "auto")
        {
            if (!tableStyle.ContainsKey("max-width"))
                tableStyle["max-width"] = widthVal;
            tableStyle["width"] = "100%";
        }

        var tableStyleStr = StyleToString(tableStyle);
        var tdStyleStr = StyleToString(tdStyle);

        return $@"<table align=""center"" width=""100%"" border=""0"" cellPadding=""0"" cellSpacing=""0"" role=""presentation"" style=""{tableStyleStr}"">
<tbody>
<tr style=""width:100%"">
<td style=""{tdStyleStr}"">{inner}</td>
</tr>
</tbody>
</table>";
    }

    /// <summary>
    /// Process button with MSO compatibility (like Node.js)
    /// </summary>
    private static string ProcessButton(Dictionary<string, string> attrs, string inner)
    {
        var href = attrs.GetValueOrDefault("href", "#");
        var style = ExtractAllStyleAttributes(attrs);

        // Parse padding
        var (paddingTop, paddingRight, paddingBottom, paddingLeft) = ParsePadding(style);

        var y = paddingTop + paddingBottom;
        var textRaise = PxToPt(y);

        var (plFontWidth, plSpaceCount) = ComputeFontWidthAndSpaceCount(paddingLeft);
        var (prFontWidth, prSpaceCount) = ComputeFontWidthAndSpaceCount(paddingRight);

        var buttonStyle = new Dictionary<string, string>
        {
            ["line-height"] = "100%",
            ["text-decoration"] = "none",
            ["display"] = "inline-block",
            ["max-width"] = "100%",
            ["mso-padding-alt"] = "0px"
        };

        // Merge with extracted styles
        foreach (var kvp in style)
        {
            buttonStyle[kvp.Key] = kvp.Value;
        }

        // Override padding with parsed values
        buttonStyle["padding-top"] = $"{paddingTop}px";
        buttonStyle["padding-right"] = $"{paddingRight}px";
        buttonStyle["padding-bottom"] = $"{paddingBottom}px";
        buttonStyle["padding-left"] = $"{paddingLeft}px";

        var styleStr = StyleToString(buttonStyle);

        var leftMsoSpaces = new string('​', 0); // Will use &#8202;
        var rightMsoSpaces = new string('​', 0);

        for (int i = 0; i < plSpaceCount; i++) leftMsoSpaces += "&#8202;";
        for (int i = 0; i < prSpaceCount; i++) rightMsoSpaces += "&#8202;";

        return $@"<a href=""{href}"" target=""_blank"" style=""{styleStr}""><!--[if mso]><i style=""mso-font-width:{Math.Round(plFontWidth * 100)}%;mso-text-raise:{textRaise}"" hidden>{leftMsoSpaces}</i><![endif]--><span style=""max-width:100%;display:inline-block;line-height:120%;mso-padding-alt:0px;mso-text-raise:{PxToPt(paddingBottom)}"">{inner}</span><!--[if mso]><i style=""mso-font-width:{Math.Round(prFontWidth * 100)}%"" hidden>{rightMsoSpaces}&#8203;</i><![endif]--></a>";
    }

    /// <summary>
    /// Parse padding values from style
    /// </summary>
    private static (int, int, int, int) ParsePadding(Dictionary<string, string> style)
    {
        if (style.TryGetValue("padding", out var padding))
        {
            var parts = padding.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            switch (parts.Length)
            {
                case 1:
                    var val = ParsePx(parts[0]);
                    return (val, val, val, val);
                case 2:
                    var vertical = ParsePx(parts[0]);
                    var horizontal = ParsePx(parts[1]);
                    return (vertical, horizontal, vertical, horizontal);
                case 4:
                    return (ParsePx(parts[0]), ParsePx(parts[1]), ParsePx(parts[2]), ParsePx(parts[3]));
            }
        }

        var pt = ParsePx(style.GetValueOrDefault("padding-top", "0"));
        var pr = ParsePx(style.GetValueOrDefault("padding-right", "0"));
        var pb = ParsePx(style.GetValueOrDefault("padding-bottom", "0"));
        var pl = ParsePx(style.GetValueOrDefault("padding-left", "0"));
        return (pt, pr, pb, pl);
    }

    private static int ParsePx(string s)
    {
        s = s.Replace("px", "");
        return int.TryParse(s, out var val) ? val : 0;
    }

    /// <summary>
    /// Convert px to pt for MSO
    /// </summary>
    private static int PxToPt(int px)
    {
        return (px * 3) / 4;
    }

    /// <summary>
    /// Compute font width and space count for MSO padding
    /// </summary>
    private static (double, int) ComputeFontWidthAndSpaceCount(int expectedWidth)
    {
        if (expectedWidth == 0)
        {
            return (0, 0);
        }

        var smallestSpaceCount = 0;
        var maxFontWidth = 5.0;

        while (true)
        {
            double requiredFontWidth;
            if (smallestSpaceCount > 0)
            {
                requiredFontWidth = (double)expectedWidth / smallestSpaceCount / 2.0;
            }
            else
            {
                requiredFontWidth = double.PositiveInfinity;
            }

            if (requiredFontWidth <= maxFontWidth)
            {
                return (requiredFontWidth, smallestSpaceCount);
            }
            smallestSpaceCount++;
        }
    }

    private static string ProcessTag(string content, string tagName, Func<Dictionary<string, string>, string, string> processor)
    {
        var result = content;
        var openPattern = $@"<{tagName}([^>]*)>";
        var closeTag = $"</{tagName}>";
        var openTagStart = $"<{tagName}";
        var openRe = new Regex(openPattern, RegexOptions.IgnoreCase);

        var maxIterations = 10000;
        var iterations = 0;

        while (iterations < maxIterations)
        {
            iterations++;

            // Find all opening tags
            var matches = openRe.Matches(result);
            if (matches.Count == 0) break;

            var processed = false;

            // Find the innermost tag (one that has no nested same tags)
            for (int i = matches.Count - 1; i >= 0; i--)
            {
                var match = matches[i];
                var start = match.Index;
                var innerStart = match.Index + match.Length;
                var attrsStr = match.Groups[1].Value;

                // Find the next close tag after this opening tag
                var closePos = result.IndexOf(closeTag, innerStart, StringComparison.OrdinalIgnoreCase);
                if (closePos == -1) continue;

                var inner = result.Substring(innerStart, closePos - innerStart);

                // Check if there's another opening tag inside
                if (inner.Contains(openTagStart, StringComparison.OrdinalIgnoreCase))
                {
                    // This tag has nested same tags, skip it
                    continue;
                }

                // This is an innermost tag, process it
                var attrs = ParseAttributes(attrsStr);
                var replacement = processor(attrs, inner);
                var end = closePos + closeTag.Length;

                result = result.Substring(0, start) + replacement + result.Substring(end);
                processed = true;
                break;
            }

            if (!processed) break;
        }

        return result;
    }

    private static Dictionary<string, string> ParseAttributes(string attrsStr)
    {
        var attrs = new Dictionary<string, string>();
        var re = new Regex(@"([\w-]+)=(?:""([^""]*)""|'([^']*)')");
        var matches = re.Matches(attrsStr);
        foreach (Match match in matches)
        {
            attrs[match.Groups[1].Value] = match.Groups[2].Success ? match.Groups[2].Value : match.Groups[3].Value;
        }
        return attrs;
    }

    /// <summary>
    /// Extract all style attributes from element attributes (like Node.js extractStyleAttributes)
    /// </summary>
    private static Dictionary<string, string> ExtractAllStyleAttributes(Dictionary<string, string> attrs)
    {
        var style = new Dictionary<string, string>();

        // Typography attributes
        if (attrs.TryGetValue("text-color", out var textColor))
            style["color"] = textColor;
        else if (attrs.TryGetValue("color", out var color))
            style["color"] = color;

        if (attrs.TryGetValue("background-color", out var bgColor))
            style["background-color"] = bgColor;
        if (attrs.TryGetValue("font-size", out var fontSize))
            style["font-size"] = fontSize;
        if (attrs.TryGetValue("font-family", out var fontFamily))
            style["font-family"] = fontFamily;
        if (attrs.TryGetValue("font-weight", out var fontWeight))
            style["font-weight"] = fontWeight;
        if (attrs.TryGetValue("line-height", out var lineHeight))
            style["line-height"] = lineHeight;
        if (attrs.TryGetValue("text-align", out var textAlign))
            style["text-align"] = textAlign;
        if (attrs.TryGetValue("text-decoration", out var textDecoration))
            style["text-decoration"] = textDecoration;

        // Dimensions
        if (attrs.TryGetValue("width", out var width))
            style["width"] = width;
        if (attrs.TryGetValue("height", out var height))
            style["height"] = height;
        if (attrs.TryGetValue("max-width", out var maxWidth))
            style["max-width"] = maxWidth;
        if (attrs.TryGetValue("max-height", out var maxHeight))
            style["max-height"] = maxHeight;
        if (attrs.TryGetValue("min-width", out var minWidth))
            style["min-width"] = minWidth;
        if (attrs.TryGetValue("min-height", out var minHeight))
            style["min-height"] = minHeight;

        // Spacing - Padding
        if (attrs.TryGetValue("padding", out var padding))
        {
            style["padding"] = padding;
        }
        else
        {
            if (attrs.TryGetValue("padding-top", out var pt))
                style["padding-top"] = pt;
            if (attrs.TryGetValue("padding-right", out var pr))
                style["padding-right"] = pr;
            if (attrs.TryGetValue("padding-bottom", out var pb))
                style["padding-bottom"] = pb;
            if (attrs.TryGetValue("padding-left", out var pl))
                style["padding-left"] = pl;
        }

        // Spacing - Margin
        if (attrs.TryGetValue("margin", out var margin))
        {
            style["margin"] = margin;
        }
        else
        {
            if (attrs.TryGetValue("margin-top", out var mt))
                style["margin-top"] = mt;
            if (attrs.TryGetValue("margin-right", out var mr))
                style["margin-right"] = mr;
            if (attrs.TryGetValue("margin-bottom", out var mb))
                style["margin-bottom"] = mb;
            if (attrs.TryGetValue("margin-left", out var ml))
                style["margin-left"] = ml;
        }

        // Borders
        if (attrs.TryGetValue("border", out var border))
        {
            style["border"] = border;
        }
        else
        {
            if (attrs.TryGetValue("border-top", out var bt))
                style["border-top"] = bt;
            if (attrs.TryGetValue("border-right", out var br))
                style["border-right"] = br;
            if (attrs.TryGetValue("border-bottom", out var bb))
                style["border-bottom"] = bb;
            if (attrs.TryGetValue("border-left", out var bl))
                style["border-left"] = bl;
            if (attrs.TryGetValue("border-color", out var bc))
                style["border-color"] = bc;
            if (attrs.TryGetValue("border-width", out var bw))
                style["border-width"] = bw;
            if (attrs.TryGetValue("border-style", out var bs))
                style["border-style"] = bs;
        }

        // Border Radius
        if (attrs.TryGetValue("border-radius", out var borderRadius))
        {
            style["border-radius"] = borderRadius;
        }
        else
        {
            if (attrs.TryGetValue("border-top-left-radius", out var btlr))
                style["border-top-left-radius"] = btlr;
            if (attrs.TryGetValue("border-top-right-radius", out var btrr))
                style["border-top-right-radius"] = btrr;
            if (attrs.TryGetValue("border-bottom-left-radius", out var bblr))
                style["border-bottom-left-radius"] = bblr;
            if (attrs.TryGetValue("border-bottom-right-radius", out var bbrr))
                style["border-bottom-right-radius"] = bbrr;
        }

        // Background image
        if (attrs.TryGetValue("background-image", out var backgroundImage))
        {
            style["background-image"] = $"url('{backgroundImage}')";
            if (attrs.TryGetValue("background-size", out var bgSize))
                style["background-size"] = bgSize;
            else
                style["background-size"] = "cover";
            if (attrs.TryGetValue("background-position", out var bgPos))
                style["background-position"] = bgPos;
            else
                style["background-position"] = "center";
            if (attrs.TryGetValue("background-repeat", out var bgRepeat))
                style["background-repeat"] = bgRepeat;
            else
                style["background-repeat"] = "no-repeat";
        }
        else
        {
            if (attrs.TryGetValue("background-size", out var bgSize))
                style["background-size"] = bgSize;
            if (attrs.TryGetValue("background-position", out var bgPos))
                style["background-position"] = bgPos;
            if (attrs.TryGetValue("background-repeat", out var bgRepeat))
                style["background-repeat"] = bgRepeat;
        }

        return style;
    }

    /// <summary>
    /// Convert style dictionary to inline style string
    /// </summary>
    private static string StyleToString(Dictionary<string, string> style)
    {
        var parts = new List<string>();
        foreach (var kvp in style)
        {
            parts.Add($"{kvp.Key}:{kvp.Value}");
        }
        return string.Join(";", parts);
    }

    private static bool EvaluateCondition(string expr, Dictionary<string, object> config)
    {
        var trimmed = expr.Trim();

        // OR: split on ||, return true if any part is true
        if (trimmed.Contains("||"))
        {
            return trimmed.Split("||").Any(part => EvaluateCondition(part, config));
        }

        // AND: split on &&, return true if all parts are true
        if (trimmed.Contains("&&"))
        {
            return trimmed.Split("&&").All(part => EvaluateCondition(part, config));
        }

        // Equality: key == "value"
        var eqMatch = Regex.Match(trimmed, @"^(\w+)\s*==\s*""([^""]*)""$");
        if (eqMatch.Success)
        {
            config.TryGetValue(eqMatch.Groups[1].Value, out var val);
            return ObjectToString(val ?? "") == eqMatch.Groups[2].Value;
        }

        // Inequality: key != "value"
        var neqMatch = Regex.Match(trimmed, @"^(\w+)\s*!=\s*""([^""]*)""$");
        if (neqMatch.Success)
        {
            config.TryGetValue(neqMatch.Groups[1].Value, out var val);
            return ObjectToString(val ?? "") != neqMatch.Groups[2].Value;
        }

        // Simple truthy check
        config.TryGetValue(trimmed, out var condVal);
        return IsTruthy(condVal);
    }

    private static bool IsTruthy(object? val)
    {
        if (val == null) return false;
        if (val is string s) return !string.IsNullOrEmpty(s);
        if (val is bool b) return b;
        if (val is int i) return i != 0;
        if (val is long l) return l != 0;
        if (val is double d) return d != 0;
        if (val is float f) return f != 0;
        if (val is JsonElement je)
        {
            switch (je.ValueKind)
            {
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return false;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.Number:
                    return je.GetDouble() != 0;
                case JsonValueKind.String:
                    return !string.IsNullOrEmpty(je.GetString());
                case JsonValueKind.Array:
                    return je.GetArrayLength() > 0;
                case JsonValueKind.Object:
                    return true;
            }
        }
        if (val is System.Collections.ICollection col) return col.Count > 0;
        if (val is System.Collections.IEnumerable) return true;
        return true;
    }

    private static string ObjectToString(object? val)
    {
        if (val == null) return "";
        if (val is string s) return s;
        if (val is JsonElement je)
        {
            switch (je.ValueKind)
            {
                case JsonValueKind.String: return je.GetString() ?? "";
                case JsonValueKind.Number: return je.GetRawText();
                case JsonValueKind.True: return "true";
                case JsonValueKind.False: return "false";
                case JsonValueKind.Null:
                case JsonValueKind.Undefined: return "";
                default: return je.GetRawText();
            }
        }
        return val.ToString() ?? "";
    }

    private static string RenderTemplate(string template, Dictionary<string, object> config)
    {
        var result = template;

        // Process #each loops
        var eachRe = new Regex(@"\{%#each\s+(\w+)(?:\s+as\s+(\w+))?%\}([\s\S]*?)\{%/each%\}");
        result = eachRe.Replace(result, m =>
        {
            var key = m.Groups[1].Value;
            var alias = m.Groups[2].Success && !string.IsNullOrEmpty(m.Groups[2].Value) ? m.Groups[2].Value : "item";
            var body = m.Groups[3].Value;
            if (!config.TryGetValue(key, out var val)) return "";

            var items = new List<Dictionary<string, object>>();
            if (val is JsonElement je && je.ValueKind == JsonValueKind.Array)
            {
                foreach (var elem in je.EnumerateArray())
                {
                    if (elem.ValueKind == JsonValueKind.Object)
                    {
                        var dict = new Dictionary<string, object>();
                        foreach (var prop in elem.EnumerateObject())
                        {
                            dict[prop.Name] = prop.Value;
                        }
                        items.Add(dict);
                    }
                }
            }
            else if (val is List<object> list)
            {
                foreach (var item in list)
                {
                    if (item is Dictionary<string, object> d) items.Add(d);
                }
            }

            var sb = new StringBuilder();
            foreach (var item in items)
            {
                var merged = new Dictionary<string, object>(config);
                foreach (var kv in item)
                {
                    merged[$"{alias}.{kv.Key}"] = kv.Value;
                }
                sb.Append(RenderTemplate(body, merged));
            }
            return sb.ToString();
        });

        // Process #if / #else conditionals - handle innermost first, loop until stable
        var ifRe = new Regex(@"\{%#if\s+([^%]+)%\}((?:(?!\{%#if\s)[\s\S])*?)\{%/if%\}");
        var prevResult = "";
        var maxIter = 100;
        var iter = 0;
        while (result != prevResult && iter < maxIter)
        {
            iter++;
            prevResult = result;
            result = ifRe.Replace(result, im =>
            {
                var condition = im.Groups[1].Value;
                var innerContent = im.Groups[2].Value;

                var condResult = EvaluateCondition(condition, config);

                var elseParts = Regex.Split(innerContent, @"\{%else%\}");
                var trueBranch = elseParts[0];
                var falseBranch = elseParts.Length > 1 ? elseParts[1] : "";

                return condResult ? trueBranch : falseBranch;
            });
        }

        // Process fallback variables: {%variable ?? fallback%}
        var fallbackRe = new Regex(@"\{%(\w[\w.]*)\s*\?\?\s*([^%]+)%\}");
        result = fallbackRe.Replace(result, fm =>
        {
            var fKey = fm.Groups[1].Value;
            var fallback = fm.Groups[2].Value.Trim();
            if (config.TryGetValue(fKey, out var fVal) && IsTruthy(fVal))
            {
                return ObjectToString(fVal);
            }
            return fallback;
        });

        // Process simple variables: {%variable%}
        var simpleRe = new Regex(@"\{%(\w[\w.]*)%\}");
        result = simpleRe.Replace(result, sm =>
        {
            var sKey = sm.Groups[1].Value;
            if (config.TryGetValue(sKey, out var sVal))
            {
                return ObjectToString(sVal);
            }
            return "";
        });

        return result;
    }

    private static string ProcessBlockTag(Dictionary<string, string> attrs, string inner)
    {
        var template = !string.IsNullOrWhiteSpace(inner) ? inner.Trim() : (attrs.GetValueOrDefault("template") ?? "");
        if (string.IsNullOrEmpty(template)) return "";
        var configStr = (attrs.GetValueOrDefault("config") ?? "{}").Replace("'", "\"").Replace("&quot;", "\"").Replace("&amp;", "&");
        Dictionary<string, object>? config;
        try { config = JsonSerializer.Deserialize<Dictionary<string, object>>(configStr); }
        catch { config = new Dictionary<string, object>(); }
        return RenderTemplate(template, config ?? new Dictionary<string, object>());
    }
}

/// <summary>
/// Syntax highlighter for codeblock elements using ColorCode.HTML library.
/// Supports multiple themes and common programming languages.
/// </summary>
internal static class SyntaxHighlighter
{
    /// <summary>
    /// Theme definition with background and default text colors
    /// </summary>
    private class ThemeColors
    {
        public string Background { get; set; } = "";
        public string DefaultColor { get; set; } = "";
    }

    private static readonly Dictionary<string, ThemeColors> Themes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["oneDark"] = new ThemeColors { Background = "#282c34", DefaultColor = "#abb2bf" },
        ["oneLight"] = new ThemeColors { Background = "#fafafa", DefaultColor = "#383a42" },
        ["vscDarkPlus"] = new ThemeColors { Background = "#1e1e1e", DefaultColor = "#d4d4d4" },
        ["vs"] = new ThemeColors { Background = "white", DefaultColor = "#393A34" },
    };

    /// <summary>
    /// Helper to convert a hex color (#RRGGBB) to ColorCode's ARGB format (#FFRRGGBB)
    /// </summary>
    private static string ToArgb(string hex)
    {
        if (hex.StartsWith("#") && hex.Length == 7)
            return "#FF" + hex.Substring(1);
        return hex;
    }

    /// <summary>
    /// Build a ColorCode StyleDictionary for the One Dark theme
    /// </summary>
    private static ColorCode.Styling.StyleDictionary BuildOneDarkStyles()
    {
        return new ColorCode.Styling.StyleDictionary
        {
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PlainText) { Foreground = ToArgb("#abb2bf"), Background = ToArgb("#282c34") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Keyword) { Foreground = ToArgb("#c678dd") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.ControlKeyword) { Foreground = ToArgb("#c678dd") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PreprocessorKeyword) { Foreground = ToArgb("#c678dd") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.String) { Foreground = ToArgb("#98c379") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.StringCSharpVerbatim) { Foreground = ToArgb("#98c379") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.StringEscape) { Foreground = ToArgb("#56b6c2") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Comment) { Foreground = ToArgb("#5c6370"), Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlComment) { Foreground = ToArgb("#5c6370"), Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlComment) { Foreground = ToArgb("#5c6370"), Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlDocComment) { Foreground = ToArgb("#5c6370"), Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlDocTag) { Foreground = ToArgb("#5c6370") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Number) { Foreground = ToArgb("#d19a66") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.ClassName) { Foreground = ToArgb("#d19a66") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Type) { Foreground = ToArgb("#d19a66") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlTagDelimiter) { Foreground = ToArgb("#abb2bf") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlElementName) { Foreground = ToArgb("#e06c75") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlAttributeName) { Foreground = ToArgb("#d19a66") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlAttributeValue) { Foreground = ToArgb("#98c379") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlOperator) { Foreground = ToArgb("#abb2bf") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlEntity) { Foreground = ToArgb("#abb2bf") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlDelimiter) { Foreground = ToArgb("#abb2bf") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlName) { Foreground = ToArgb("#e06c75") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlAttribute) { Foreground = ToArgb("#d19a66") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlAttributeValue) { Foreground = ToArgb("#98c379") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlAttributeQuotes) { Foreground = ToArgb("#98c379") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlCDataSection) { Foreground = ToArgb("#d19a66") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.CssSelector) { Foreground = ToArgb("#e06c75") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.CssPropertyName) { Foreground = ToArgb("#abb2bf") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.CssPropertyValue) { Foreground = ToArgb("#d19a66") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.SqlSystemFunction) { Foreground = ToArgb("#c678dd") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Operator) { Foreground = ToArgb("#61afef") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Delimiter) { Foreground = ToArgb("#abb2bf") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Constructor) { Foreground = ToArgb("#61afef") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Predefined) { Foreground = ToArgb("#d19a66") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PseudoKeyword) { Foreground = ToArgb("#c678dd") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.NameSpace) { Foreground = ToArgb("#d19a66") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.TypeVariable) { Foreground = ToArgb("#d19a66"), Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.BuiltinFunction) { Foreground = ToArgb("#61afef"), Bold = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.BuiltinValue) { Foreground = ToArgb("#d19a66"), Bold = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Attribute) { Foreground = ToArgb("#d19a66"), Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.JsonKey) { Foreground = ToArgb("#e06c75") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.JsonString) { Foreground = ToArgb("#98c379") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.JsonNumber) { Foreground = ToArgb("#d19a66") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.JsonConst) { Foreground = ToArgb("#d19a66") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.MarkdownHeader) { Foreground = ToArgb("#e06c75"), Bold = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.MarkdownCode) { Foreground = ToArgb("#98c379") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.MarkdownListItem) { Bold = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.MarkdownEmph) { Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.MarkdownBold) { Bold = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellAttribute) { Foreground = ToArgb("#d19a66") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellOperator) { Foreground = ToArgb("#61afef") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellType) { Foreground = ToArgb("#d19a66") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellVariable) { Foreground = ToArgb("#e06c75") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellCommand) { Foreground = ToArgb("#61afef") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellParameter) { Foreground = ToArgb("#abb2bf") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlServerSideScript) { Background = ToArgb("#d19a66") },
        };
    }

    /// <summary>
    /// Build a ColorCode StyleDictionary for the One Light theme
    /// </summary>
    private static ColorCode.Styling.StyleDictionary BuildOneLightStyles()
    {
        return new ColorCode.Styling.StyleDictionary
        {
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PlainText) { Foreground = ToArgb("#383a42"), Background = ToArgb("#fafafa") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Keyword) { Foreground = ToArgb("#a626a4") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.ControlKeyword) { Foreground = ToArgb("#a626a4") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PreprocessorKeyword) { Foreground = ToArgb("#a626a4") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.String) { Foreground = ToArgb("#50a14f") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.StringCSharpVerbatim) { Foreground = ToArgb("#50a14f") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.StringEscape) { Foreground = ToArgb("#0184bc") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Comment) { Foreground = ToArgb("#a0a1a7"), Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlComment) { Foreground = ToArgb("#a0a1a7"), Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlComment) { Foreground = ToArgb("#a0a1a7"), Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlDocComment) { Foreground = ToArgb("#a0a1a7"), Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlDocTag) { Foreground = ToArgb("#a0a1a7") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Number) { Foreground = ToArgb("#b76b01") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.ClassName) { Foreground = ToArgb("#b76b01") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Type) { Foreground = ToArgb("#b76b01") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlTagDelimiter) { Foreground = ToArgb("#383a42") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlElementName) { Foreground = ToArgb("#e45649") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlAttributeName) { Foreground = ToArgb("#b76b01") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlAttributeValue) { Foreground = ToArgb("#50a14f") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlOperator) { Foreground = ToArgb("#383a42") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlEntity) { Foreground = ToArgb("#383a42") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlDelimiter) { Foreground = ToArgb("#383a42") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlName) { Foreground = ToArgb("#e45649") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlAttribute) { Foreground = ToArgb("#b76b01") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlAttributeValue) { Foreground = ToArgb("#50a14f") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlAttributeQuotes) { Foreground = ToArgb("#50a14f") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlCDataSection) { Foreground = ToArgb("#b76b01") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.CssSelector) { Foreground = ToArgb("#e45649") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.CssPropertyName) { Foreground = ToArgb("#383a42") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.CssPropertyValue) { Foreground = ToArgb("#b76b01") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.SqlSystemFunction) { Foreground = ToArgb("#a626a4") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Operator) { Foreground = ToArgb("#4078f2") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Delimiter) { Foreground = ToArgb("#383a42") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Constructor) { Foreground = ToArgb("#4078f2") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Predefined) { Foreground = ToArgb("#b76b01") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PseudoKeyword) { Foreground = ToArgb("#a626a4") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.NameSpace) { Foreground = ToArgb("#b76b01") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.TypeVariable) { Foreground = ToArgb("#b76b01"), Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.BuiltinFunction) { Foreground = ToArgb("#4078f2"), Bold = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.BuiltinValue) { Foreground = ToArgb("#b76b01"), Bold = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Attribute) { Foreground = ToArgb("#b76b01"), Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.JsonKey) { Foreground = ToArgb("#e45649") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.JsonString) { Foreground = ToArgb("#50a14f") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.JsonNumber) { Foreground = ToArgb("#b76b01") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.JsonConst) { Foreground = ToArgb("#b76b01") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.MarkdownHeader) { Foreground = ToArgb("#e45649"), Bold = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.MarkdownCode) { Foreground = ToArgb("#50a14f") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.MarkdownListItem) { Bold = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.MarkdownEmph) { Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.MarkdownBold) { Bold = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellAttribute) { Foreground = ToArgb("#b76b01") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellOperator) { Foreground = ToArgb("#4078f2") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellType) { Foreground = ToArgb("#b76b01") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellVariable) { Foreground = ToArgb("#e45649") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellCommand) { Foreground = ToArgb("#4078f2") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellParameter) { Foreground = ToArgb("#383a42") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlServerSideScript) { Background = ToArgb("#b76b01") },
        };
    }

    /// <summary>
    /// Build a ColorCode StyleDictionary for the VS Code Dark+ theme
    /// </summary>
    private static ColorCode.Styling.StyleDictionary BuildVscDarkPlusStyles()
    {
        return new ColorCode.Styling.StyleDictionary
        {
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PlainText) { Foreground = ToArgb("#d4d4d4"), Background = ToArgb("#1e1e1e") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Keyword) { Foreground = ToArgb("#569CD6") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.ControlKeyword) { Foreground = ToArgb("#569CD6") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PreprocessorKeyword) { Foreground = ToArgb("#569CD6") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.String) { Foreground = ToArgb("#ce9178") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.StringCSharpVerbatim) { Foreground = ToArgb("#ce9178") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.StringEscape) { Foreground = ToArgb("#d7ba7d") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Comment) { Foreground = ToArgb("#6a9955") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlComment) { Foreground = ToArgb("#6a9955") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlComment) { Foreground = ToArgb("#6a9955") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlDocComment) { Foreground = ToArgb("#608B4E") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlDocTag) { Foreground = ToArgb("#608B4E") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Number) { Foreground = ToArgb("#b5cea8") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.ClassName) { Foreground = ToArgb("#4ec9b0") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Type) { Foreground = ToArgb("#4ec9b0") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlTagDelimiter) { Foreground = ToArgb("#808080") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlElementName) { Foreground = ToArgb("#569cd6") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlAttributeName) { Foreground = ToArgb("#9cdcfe") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlAttributeValue) { Foreground = ToArgb("#ce9178") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlOperator) { Foreground = ToArgb("#d4d4d4") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlEntity) { Foreground = ToArgb("#d4d4d4") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlDelimiter) { Foreground = ToArgb("#808080") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlName) { Foreground = ToArgb("#569cd6") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlAttribute) { Foreground = ToArgb("#9cdcfe") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlAttributeValue) { Foreground = ToArgb("#ce9178") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlAttributeQuotes) { Foreground = ToArgb("#569cd6") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlCDataSection) { Foreground = ToArgb("#d7ba7d") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.CssSelector) { Foreground = ToArgb("#d7ba7d") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.CssPropertyName) { Foreground = ToArgb("#9cdcfe") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.CssPropertyValue) { Foreground = ToArgb("#ce9178") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.SqlSystemFunction) { Foreground = ToArgb("#dcdcaa") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Operator) { Foreground = ToArgb("#d4d4d4") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Delimiter) { Foreground = ToArgb("#d4d4d4") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Constructor) { Foreground = ToArgb("#4ec9b0") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Predefined) { Foreground = ToArgb("#9cdcfe") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PseudoKeyword) { Foreground = ToArgb("#569cd6") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.NameSpace) { Foreground = ToArgb("#4ec9b0") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.TypeVariable) { Foreground = ToArgb("#4ec9b0"), Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.BuiltinFunction) { Foreground = ToArgb("#dcdcaa"), Bold = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.BuiltinValue) { Foreground = ToArgb("#9cdcfe"), Bold = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Attribute) { Foreground = ToArgb("#9cdcfe"), Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.JsonKey) { Foreground = ToArgb("#9cdcfe") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.JsonString) { Foreground = ToArgb("#ce9178") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.JsonNumber) { Foreground = ToArgb("#b5cea8") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.JsonConst) { Foreground = ToArgb("#569cd6") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.MarkdownHeader) { Foreground = ToArgb("#569cd6"), Bold = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.MarkdownCode) { Foreground = ToArgb("#ce9178") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.MarkdownListItem) { Bold = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.MarkdownEmph) { Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.MarkdownBold) { Bold = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellAttribute) { Foreground = ToArgb("#9cdcfe") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellOperator) { Foreground = ToArgb("#d4d4d4") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellType) { Foreground = ToArgb("#4ec9b0") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellVariable) { Foreground = ToArgb("#9cdcfe") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellCommand) { Foreground = ToArgb("#dcdcaa") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellParameter) { Foreground = ToArgb("#9cdcfe") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlServerSideScript) { Background = ToArgb("#dcdcaa") },
        };
    }

    /// <summary>
    /// Build a ColorCode StyleDictionary for the VS (light) theme
    /// </summary>
    private static ColorCode.Styling.StyleDictionary BuildVsStyles()
    {
        return new ColorCode.Styling.StyleDictionary
        {
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PlainText) { Foreground = ToArgb("#393A34"), Background = "#FFFFFFFF" },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Keyword) { Foreground = ToArgb("#0000ff") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.ControlKeyword) { Foreground = ToArgb("#0000ff") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PreprocessorKeyword) { Foreground = ToArgb("#0000ff") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.String) { Foreground = ToArgb("#A31515") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.StringCSharpVerbatim) { Foreground = ToArgb("#A31515") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.StringEscape) { Foreground = ToArgb("#ff0000") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Comment) { Foreground = ToArgb("#008000"), Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlComment) { Foreground = ToArgb("#008000"), Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlComment) { Foreground = ToArgb("#008000"), Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlDocComment) { Foreground = ToArgb("#008000"), Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlDocTag) { Foreground = ToArgb("#008000") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Number) { Foreground = ToArgb("#36acaa") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.ClassName) { Foreground = ToArgb("#2B91AF") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Type) { Foreground = ToArgb("#2B91AF") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlTagDelimiter) { Foreground = ToArgb("#800000") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlElementName) { Foreground = ToArgb("#800000") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlAttributeName) { Foreground = ToArgb("#ff0000") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlAttributeValue) { Foreground = ToArgb("#0000ff") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlOperator) { Foreground = ToArgb("#393A34") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlEntity) { Foreground = ToArgb("#ff0000") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlDelimiter) { Foreground = ToArgb("#800000") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlName) { Foreground = ToArgb("#800000") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlAttribute) { Foreground = ToArgb("#ff0000") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlAttributeValue) { Foreground = ToArgb("#0000ff") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlAttributeQuotes) { Foreground = ToArgb("#0000ff") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.XmlCDataSection) { Foreground = ToArgb("#808080") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.CssSelector) { Foreground = ToArgb("#800000") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.CssPropertyName) { Foreground = ToArgb("#ff0000") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.CssPropertyValue) { Foreground = ToArgb("#0000ff") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.SqlSystemFunction) { Foreground = ToArgb("#0000ff") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Operator) { Foreground = ToArgb("#393A34") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Delimiter) { Foreground = ToArgb("#393A34") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Constructor) { Foreground = ToArgb("#2B91AF") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Predefined) { Foreground = ToArgb("#0000ff") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PseudoKeyword) { Foreground = ToArgb("#0000ff") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.NameSpace) { Foreground = ToArgb("#2B91AF") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.TypeVariable) { Foreground = ToArgb("#2B91AF"), Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.BuiltinFunction) { Foreground = ToArgb("#393A34"), Bold = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.BuiltinValue) { Foreground = ToArgb("#36acaa"), Bold = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.Attribute) { Foreground = ToArgb("#2B91AF"), Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.JsonKey) { Foreground = ToArgb("#ff0000") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.JsonString) { Foreground = ToArgb("#A31515") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.JsonNumber) { Foreground = ToArgb("#36acaa") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.JsonConst) { Foreground = ToArgb("#0000ff") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.MarkdownHeader) { Foreground = ToArgb("#0000ff"), Bold = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.MarkdownCode) { Foreground = ToArgb("#A31515") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.MarkdownListItem) { Bold = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.MarkdownEmph) { Italic = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.MarkdownBold) { Bold = true },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellAttribute) { Foreground = ToArgb("#2B91AF") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellOperator) { Foreground = ToArgb("#393A34") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellType) { Foreground = ToArgb("#2B91AF") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellVariable) { Foreground = ToArgb("#36acaa") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellCommand) { Foreground = ToArgb("#393A34") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.PowerShellParameter) { Foreground = ToArgb("#393A34") },
            new ColorCode.Styling.Style(ColorCode.Common.ScopeName.HtmlServerSideScript) { Background = "#FFFFFF00" },
        };
    }

    /// <summary>
    /// Cache of theme name to StyleDictionary
    /// </summary>
    private static readonly Dictionary<string, ColorCode.Styling.StyleDictionary> ThemeStyleDictionaries = new(StringComparer.OrdinalIgnoreCase)
    {
        ["oneDark"] = BuildOneDarkStyles(),
        ["oneLight"] = BuildOneLightStyles(),
        ["vscDarkPlus"] = BuildVscDarkPlusStyles(),
        ["vs"] = BuildVsStyles(),
    };

    /// <summary>
    /// Map of language name aliases to ColorCode language IDs
    /// </summary>
    private static readonly Dictionary<string, ColorCode.ILanguage?> LanguageMap;

    static SyntaxHighlighter()
    {
        LanguageMap = new Dictionary<string, ColorCode.ILanguage?>(StringComparer.OrdinalIgnoreCase)
        {
            ["javascript"] = ColorCode.Languages.JavaScript,
            ["js"] = ColorCode.Languages.JavaScript,
            ["typescript"] = ColorCode.Languages.Typescript,
            ["ts"] = ColorCode.Languages.Typescript,
            ["python"] = ColorCode.Languages.Python,
            ["py"] = ColorCode.Languages.Python,
            ["java"] = ColorCode.Languages.Java,
            ["php"] = ColorCode.Languages.Php,
            ["csharp"] = ColorCode.Languages.CSharp,
            ["cs"] = ColorCode.Languages.CSharp,
            ["c#"] = ColorCode.Languages.CSharp,
            ["html"] = ColorCode.Languages.Html,
            ["css"] = ColorCode.Languages.Css,
            ["xml"] = ColorCode.Languages.Xml,
            ["sql"] = ColorCode.Languages.Sql,
            ["markdown"] = ColorCode.Languages.Markdown,
            ["md"] = ColorCode.Languages.Markdown,
            ["cpp"] = ColorCode.Languages.Cpp,
            ["c++"] = ColorCode.Languages.Cpp,
            ["powershell"] = ColorCode.Languages.PowerShell,
            ["ps"] = ColorCode.Languages.PowerShell,
        };
    }

    /// <summary>
    /// Process a codeblock tag with syntax highlighting
    /// </summary>
    public static string ProcessCodeBlock(Dictionary<string, string> attrs, string inner)
    {
        var language = attrs.GetValueOrDefault("language", "");
        var themeName = attrs.GetValueOrDefault("theme", "oneDark");
        var customStyle = ExtractCodeBlockStyle(attrs);

        var theme = Themes.GetValueOrDefault(themeName) ?? Themes["oneDark"];

        // Try to get ColorCode language
        ColorCode.ILanguage? colorCodeLang = null;
        if (!string.IsNullOrEmpty(language))
        {
            LanguageMap.TryGetValue(language, out colorCodeLang);
        }

        string highlighted;
        if (colorCodeLang != null)
        {
            highlighted = HighlightWithColorCode(inner, colorCodeLang, theme, themeName);
        }
        else
        {
            highlighted = WrapPlainText(HtmlEscape(inner), theme);
        }

        // Build the pre style
        var preStyle = new Dictionary<string, string>
        {
            ["background-color"] = theme.Background,
            ["color"] = theme.DefaultColor,
            ["font-family"] = "'Fira Code', 'Fira Mono', Menlo, Consolas, 'DejaVu Sans Mono', monospace",
            ["font-size"] = "13px",
            ["line-height"] = "1.5",
            ["padding"] = "1em",
            ["margin"] = "0.5em 0",
            ["overflow"] = "auto",
            ["border-radius"] = "0.3em",
            ["width"] = "100%",
            ["box-sizing"] = "border-box",
            ["white-space"] = "pre",
            ["word-spacing"] = "normal",
            ["word-break"] = "normal",
            ["direction"] = "ltr",
            ["text-align"] = "left",
        };

        // Merge custom styles (override defaults)
        foreach (var kvp in customStyle)
        {
            preStyle[kvp.Key] = kvp.Value;
        }

        var styleStr = string.Join(";", preStyle.Select(kvp => $"{kvp.Key}:{kvp.Value}"));

        return $@"<pre style=""{styleStr}""><code>{highlighted}</code></pre>";
    }

    /// <summary>
    /// Highlight code using ColorCode.HTML library with theme-specific StyleDictionary
    /// </summary>
    private static string HighlightWithColorCode(string code, ColorCode.ILanguage language, ThemeColors theme, string themeName)
    {
        var styleDictionary = ThemeStyleDictionaries.GetValueOrDefault(themeName) ?? ThemeStyleDictionaries["oneDark"];
        var formatter = new ColorCode.HtmlFormatter(styleDictionary);
        var rawHtml = formatter.GetHtmlString(code, language);

        // ColorCode wraps output in <div style="..."><pre>...</pre></div>
        // Extract the inner content from the pre tag
        var content = rawHtml;

        // Strip outer div wrapper if present
        var divMatch = Regex.Match(content, @"<div[^>]*>\s*<pre[^>]*>([\s\S]*?)</pre>\s*</div>", RegexOptions.IgnoreCase);
        if (divMatch.Success)
        {
            content = divMatch.Groups[1].Value;
        }
        else
        {
            // Try just stripping pre tags
            var preMatch = Regex.Match(content, @"<pre[^>]*>([\s\S]*?)</pre>", RegexOptions.IgnoreCase);
            if (preMatch.Success)
            {
                content = preMatch.Groups[1].Value;
            }
        }

        // Wrap each line in a <p> tag for email compatibility
        var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            var displayLine = string.IsNullOrEmpty(line) ? "" : line;
            sb.Append($@"<p style=""margin:0;min-height:1em"">{displayLine}</p>");
        }

        return sb.ToString();
    }

    private static Dictionary<string, string> ExtractCodeBlockStyle(Dictionary<string, string> attrs)
    {
        var style = new Dictionary<string, string>();
        string[] passthrough = { "width", "max-width", "margin", "padding", "font-size", "font-family", "border-radius", "background-color" };
        foreach (var key in passthrough)
        {
            if (attrs.TryGetValue(key, out var val))
                style[key] = val;
        }
        return style;
    }

    private static string HtmlEscape(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    /// <summary>
    /// Wrap plain text (no language) in line-wrapped paragraphs with theme colors
    /// </summary>
    private static string WrapPlainText(string escaped, ThemeColors theme)
    {
        var lines = escaped.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            var displayLine = line.Replace(" ", "\u00A0\u200D\u200B");
            sb.Append($@"<p style=""margin:0;min-height:1em"">{displayLine}</p>");
        }
        return sb.ToString();
    }

}
