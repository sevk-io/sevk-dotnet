using System;
using System.Collections.Generic;
using System.Text;
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
public static class MarkupRenderer
{
    /// <summary>
    /// Generate email HTML from Sevk markup
    /// </summary>
    public static string Render(string? markup, EmailHeadSettings? headSettings = null)
    {
        if (string.IsNullOrEmpty(markup)) return "";

        string contentToProcess;
        EmailHeadSettings settings;

        if (headSettings != null)
        {
            contentToProcess = markup;
            settings = headSettings;
        }
        else
        {
            var parsed = ParseEmailHTML(markup);
            contentToProcess = parsed.Body;
            settings = parsed.HeadSettings;
        }

        var normalized = NormalizeMarkup(contentToProcess);
        var processed = ProcessMarkup(normalized);

        // Build head content
        var titleTag = !string.IsNullOrEmpty(settings.Title) ? $"<title>{settings.Title}</title>" : "";
        var fontLinks = GenerateFontLinks(settings.Fonts);
        var customStyles = !string.IsNullOrEmpty(settings.Styles) ? $"<style type=\"text/css\">{settings.Styles}</style>" : "";
        var previewText = !string.IsNullOrEmpty(settings.PreviewText)
            ? $"<div style=\"display:none;font-size:1px;color:#ffffff;line-height:1px;max-height:0px;max-width:0px;opacity:0;overflow:hidden;\">{settings.PreviewText}</div>"
            : "";

        return $@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html lang=""en"" dir=""ltr"">
<head>
<meta content=""text/html; charset=UTF-8"" http-equiv=""Content-Type""/>
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
{titleTag}
{fontLinks}
{customStyles}
</head>
<body style=""margin:0;padding:0;font-family:ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;background-color:#ffffff"">
{previewText}
{processed}
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
            result = result.Replace("</link>", "</sevk-link>");
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
        result = ProcessTag(result, "row", (attrs, inner) =>
        {
            var style = ExtractAllStyleAttributes(attrs);
            var styleStr = StyleToString(style);
            return $@"<table align=""center"" width=""100%"" border=""0"" cellPadding=""0"" cellSpacing=""0"" role=""presentation"" style=""{styleStr}"">
<tbody style=""width:100%"">
<tr style=""width:100%"">{inner}</tr>
</tbody>
</table>";
        });

        // Process column tags
        result = ProcessTag(result, "column", (attrs, inner) =>
        {
            var style = ExtractAllStyleAttributes(attrs);
            var styleStr = StyleToString(style);
            return $@"<td style=""{styleStr}"">{inner}</td>";
        });

        // Process container tags (both sevk-container and container)
        result = ProcessTag(result, "sevk-container", (attrs, inner) =>
        {
            var style = ExtractAllStyleAttributes(attrs);
            var styleStr = StyleToString(style);
            return $@"<table align=""center"" width=""100%"" border=""0"" cellPadding=""0"" cellSpacing=""0"" role=""presentation"" style=""{styleStr}"">
<tbody>
<tr style=""width:100%"">
<td>{inner}</td>
</tr>
</tbody>
</table>";
        });
        result = ProcessTag(result, "container", (attrs, inner) =>
        {
            var style = ExtractAllStyleAttributes(attrs);
            var styleStr = StyleToString(style);
            return $@"<table align=""center"" width=""100%"" border=""0"" cellPadding=""0"" cellSpacing=""0"" role=""presentation"" style=""{styleStr}"">
<tbody>
<tr style=""width:100%"">
<td>{inner}</td>
</tr>
</tbody>
</table>";
        });

        // Process heading tags (both sevk-heading and heading)
        result = ProcessTag(result, "sevk-heading", (attrs, inner) =>
        {
            var level = attrs.GetValueOrDefault("level", "1");
            var style = ExtractAllStyleAttributes(attrs);
            var styleStr = StyleToString(style);
            return $@"<h{level} style=""{styleStr}"">{inner}</h{level}>";
        });
        result = ProcessTag(result, "heading", (attrs, inner) =>
        {
            var level = attrs.GetValueOrDefault("level", "1");
            var style = ExtractAllStyleAttributes(attrs);
            var styleStr = StyleToString(style);
            return $@"<h{level} style=""{styleStr}"">{inner}</h{level}>";
        });

        // Process paragraph tags
        result = ProcessTag(result, "paragraph", (attrs, inner) =>
        {
            var style = ExtractAllStyleAttributes(attrs);
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
            var width = attrs.GetValueOrDefault("width", null);
            var height = attrs.GetValueOrDefault("height", null);

            var style = ExtractAllStyleAttributes(attrs);
            if (!style.ContainsKey("outline")) style["outline"] = "none";
            if (!style.ContainsKey("border")) style["border"] = "none";
            if (!style.ContainsKey("text-decoration")) style["text-decoration"] = "none";

            var styleStr = StyleToString(style);
            var widthAttr = width != null ? $@" width=""{width}""" : "";
            var heightAttr = height != null ? $@" height=""{height}""" : "";

            return $@"<img src=""{src}"" alt=""{alt}""{widthAttr}{heightAttr} style=""{styleStr}"" />";
        });
        result = ProcessTag(result, "image", (attrs, _) =>
        {
            var src = attrs.GetValueOrDefault("src", "");
            var alt = attrs.GetValueOrDefault("alt", "");
            var width = attrs.GetValueOrDefault("width", null);
            var height = attrs.GetValueOrDefault("height", null);

            var style = ExtractAllStyleAttributes(attrs);
            if (!style.ContainsKey("outline")) style["outline"] = "none";
            if (!style.ContainsKey("border")) style["border"] = "none";
            if (!style.ContainsKey("text-decoration")) style["text-decoration"] = "none";

            var styleStr = StyleToString(style);
            var widthAttr = width != null ? $@" width=""{width}""" : "";
            var heightAttr = height != null ? $@" height=""{height}""" : "";

            return $@"<img src=""{src}"" alt=""{alt}""{widthAttr}{heightAttr} style=""{styleStr}"" />";
        });

        // Process divider tags (both sevk-divider and divider)
        result = ProcessTag(result, "sevk-divider", (attrs, _) =>
        {
            var style = ExtractAllStyleAttributes(attrs);
            var styleStr = StyleToString(style);
            var classAttr = attrs.GetValueOrDefault("class", null) ?? attrs.GetValueOrDefault("className", null);
            var classStr = classAttr != null ? $@" class=""{classAttr}""" : "";
            return $@"<div style=""{styleStr}""{classStr}></div>";
        });
        result = ProcessTag(result, "divider", (attrs, _) =>
        {
            var style = ExtractAllStyleAttributes(attrs);
            var styleStr = StyleToString(style);
            var classAttr = attrs.GetValueOrDefault("class", null) ?? attrs.GetValueOrDefault("className", null);
            var classStr = classAttr != null ? $@" class=""{classAttr}""" : "";
            return $@"<div style=""{styleStr}""{classStr}></div>";
        });

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
            if (attrs.TryGetValue("list-style-type", out var lst))
            {
                style["list-style-type"] = lst;
            }
            var styleStr = StyleToString(style);
            var classAttr = attrs.GetValueOrDefault("class", null) ?? attrs.GetValueOrDefault("className", null);
            var classStr = classAttr != null ? $@" class=""{classAttr}""" : "";
            return $@"<{tag} style=""{styleStr}""{classStr}>{inner}</{tag}>";
        });

        // Process list item tags
        result = ProcessTag(result, "li", (attrs, inner) =>
        {
            var style = ExtractAllStyleAttributes(attrs);
            var styleStr = StyleToString(style);
            var classAttr = attrs.GetValueOrDefault("class", null) ?? attrs.GetValueOrDefault("className", null);
            var classStr = classAttr != null ? $@" class=""{classAttr}""" : "";
            return $@"<li style=""{styleStr}""{classStr}>{inner}</li>";
        });

        // Process codeblock tags
        result = ProcessTag(result, "codeblock", (attrs, inner) =>
        {
            var style = ExtractAllStyleAttributes(attrs);
            if (!style.ContainsKey("width")) style["width"] = "100%";
            if (!style.ContainsKey("box-sizing")) style["box-sizing"] = "border-box";
            var styleStr = StyleToString(style);
            var escaped = inner.Replace("<", "&lt;").Replace(">", "&gt;");
            return $@"<pre style=""{styleStr}""><code>{escaped}</code></pre>";
        });

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
        var re = new Regex(@"([\w-]+)=[""']([^""']*)[""']");
        var matches = re.Matches(attrsStr);
        foreach (Match match in matches)
        {
            attrs[match.Groups[1].Value] = match.Groups[2].Value;
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
}
