using System.Text;
using System.Text.RegularExpressions;

namespace Sevk.Markup;

public static partial class Renderer
{
    private static (string Body, string GapStyles) ProcessMarkup(string content)
    {
        AssertMarkupDepth(content);
        var budget = new RenderBudget();
        var gapRules = new List<string>();

        var result = ProcessTag(content, "block", (attrs, inner) => ProcessBlockTag(attrs, inner), budget);
        result = Regex.Replace(result, @"<block([^>]*?)/>", m => ProcessBlockTag(ParseAttributes(m.Groups[1].Value), ""), RegexOptions.IgnoreCase);
        AssertMarkupDepth(result);
        result = Regex.Replace(result, @"<link(\s+(?:[\w-]+=(?:""[^""]*""|'[^']*')\s+)*)href=", "<sevk-link${1}href=", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"</link>", "</sevk-link>", RegexOptions.IgnoreCase);

        string RenderSection(Dictionary<string, string> attrs, string inner)
        {
            var style = ExtractAllStyleAttributes(attrs);
            var tdStyle = new Dictionary<string, string>();

            if (style.TryGetValue("padding", out var pad))
            {
                tdStyle["padding"] = pad;
                style.Remove("padding");
            }
            if (style.TryGetValue("text-align", out var ta))
            {
                tdStyle["text-align"] = ta;
                style.Remove("text-align");
            }
            // Tables default to border-collapse:collapse via the global reset,
            // which disables border-radius. Flip to separate when any corner
            // radius is set so the rounded border actually renders. Rebuild
            // the dict before appending so .NET's Dictionary doesn't reuse a
            // slot freed by the padding/text-align Removes, which would move
            // border-collapse to the wrong position in the emission order.
            var radiusKeys = new[] { "border-radius", "border-top-left-radius", "border-top-right-radius", "border-bottom-left-radius", "border-bottom-right-radius" };
            foreach (var rk in radiusKeys)
            {
                if (style.ContainsKey(rk))
                {
                    style = new Dictionary<string, string>(style);
                    style["border-collapse"] = "separate";
                    break;
                }
            }

            return $@"<table align=""center"" width=""100%"" border=""0"" cellPadding=""0"" cellSpacing=""0"" role=""presentation"" style=""{StyleToString(style)}"">
<tbody>
<tr>
<td style=""{StyleToString(tdStyle)}"">{inner}</td>
</tr>
</tbody>
</table>";
        }
        result = ProcessTag(result, "section", RenderSection, budget);

        // Columns must be converted before rows so row layout can inspect the generated cells.
        result = ProcessTag(result, "column", (attrs, inner) =>
        {
            var style = ExtractAllStyleAttributes(attrs);

            if (!style.ContainsKey("vertical-align"))
                style["vertical-align"] = "top";
            var styleStr = StyleToString(style);

            return $@"<td class=""sevk-column"" style=""{styleStr}"">{inner}</td>";
        }, budget);

        var rowCounter = 0;
        result = ProcessTag(result, "row", (attrs, inner) =>
        {
            var gap = attrs.GetValueOrDefault("gap", "0");
            var style = ExtractAllStyleAttributes(attrs);
            style.Remove("gap");
            var styleStr = StyleToString(style);
            var gapPx = ReplaceFirst(gap, "px");
            var gapNum = ParsePx(gap);
            var rowId = $"sevk-row-{rowCounter++}";

            var processedInner = inner;
            var columnMatches = Regex.Matches(processedInner, @"class=""sevk-column""");
            var columnCount = columnMatches.Count;

            if (columnCount > 1)
            {
                var equalWidth = $"{Math.Floor(100.0 / columnCount)}%";
                processedInner = Regex.Replace(processedInner, @"<td class=""sevk-column"" style=""([^""]*)""", (m) =>
                {
                    var existingStyle = m.Groups[1].Value;

                    if (existingStyle.Contains("width:"))
                    {
                        return m.Value;
                    }

                    return $@"<td class=""sevk-column"" style=""width:{equalWidth};{existingStyle}""";
                });
            }

            if (gapNum > 0)
            {
                var spacerTd = $@"</td><td class=""sevk-gap"" style=""width:{gapPx}px;min-width:{gapPx}px"" width=""{gapPx}""></td><td class=""sevk-column""";
                processedInner = Regex.Replace(processedInner, @"</td>\s*<td class=""sevk-column""", spacerTd);
                gapRules.Add($".{rowId} .sevk-gap{{display:none !important;}}.{rowId} > tbody > tr > td.sevk-column{{display:block !important;width:100% !important;margin-bottom:{gapPx}px !important;}}.{rowId} > tbody > tr > td.sevk-column:last-of-type{{margin-bottom:0 !important;}}");
            }

            return $@"<table align=""center"" width=""100%"" border=""0"" cellPadding=""0"" cellSpacing=""0"" role=""presentation"" class=""sevk-row-table {rowId}"" style=""{styleStr}"">
<tbody style=""width:100%"">
<tr style=""width:100%"">{processedInner}</tr>
</tbody>
</table>";
        }, budget);

        result = ProcessTag(result, "container", (attrs, inner) => ProcessContainer(attrs, inner), budget);

        result = ProcessTag(result, "heading", (attrs, inner) =>
        {
            var level = attrs.TryGetValue("level", out var requestedLevel) && requestedLevel.Length > 0 ? requestedLevel : "1";
            var style = ExtractAllStyleAttributes(attrs);

            if (!style.ContainsKey("margin"))
            {
                style["margin"] = "0";
            }

            var styleStr = StyleToString(style);

            return $@"<h{level} style=""{styleStr}"">{inner}</h{level}>";
        }, budget);

        result = ProcessTag(result, "paragraph", (attrs, inner) =>
        {
            var style = ExtractAllStyleAttributes(attrs);

            if (!style.ContainsKey("margin"))
            {
                style["margin"] = "0";
            }

            var styleStr = StyleToString(style);

            return $@"<p style=""{styleStr}"">{inner}</p>";
        }, budget);

        result = ProcessTag(result, "text", (attrs, inner) =>
        {
            var style = ExtractAllStyleAttributes(attrs);
            var styleStr = StyleToString(style);

            return $@"<span style=""{styleStr}"">{inner}</span>";
        }, budget);

        result = ProcessTag(result, "button", (attrs, inner) =>
        {
            return ProcessButton(attrs, inner);
        }, budget);

        // Images are handled as opening tags so an optional closing tag cannot consume surrounding whitespace.
        result = Regex.Replace(result, @"<image([^>]*?)/?>", match =>
        {
            var attrs = ParseAttributes(match.Groups[1].Value);
            var src = attrs.GetValueOrDefault("src", "");
            var alt = attrs.GetValueOrDefault("alt", "");
            attrs.TryGetValue("width", out var width);
            attrs.TryGetValue("height", out var height);

            var style = ExtractAllStyleAttributes(attrs);

            if (!style.ContainsKey("vertical-align"))
            {
                style["vertical-align"] = "middle";
            }
            if (!style.ContainsKey("max-width"))
            {
                style["max-width"] = "100%";
            }

            var styleStr = StyleToString(style);
            var widthValue = width is null ? "" : ReplaceFirst(width, "px");
            var heightValue = height is null ? "" : ReplaceFirst(height, "px");
            var widthAttr = widthValue.Length > 0 ? $@" width=""{widthValue}""" : "";
            var heightAttr = heightValue.Length > 0 ? $@" height=""{heightValue}""" : "";

            return $@"<img src=""{src}"" alt=""{alt}""{widthAttr}{heightAttr} style=""{styleStr}"" />";
        }, RegexOptions.IgnoreCase);

        // Match the opening tag directly because ProcessTag does not accept self-closing dividers.
        result = Regex.Replace(result, @"<divider([^>]*?)/?>", match =>
        {
            var attrs = ParseAttributes(match.Groups[1].Value);
            var style = ExtractAllStyleAttributes(attrs);
            var styleStr = StyleToString(style);
            var classStr = ClassAttribute(attrs);
            return $@"<hr style=""{styleStr}""{classStr} />";
        }, RegexOptions.IgnoreCase);

        result = Regex.Replace(result, @"</divider>", "", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"</image>", "", RegexOptions.IgnoreCase);

        result = ProcessTag(result, "sevk-link", (attrs, inner) =>
        {
            var href = attrs.GetValueOrDefault("href", "");
            var target = attrs.GetValueOrDefault("target", "");
            href = href.Length > 0 ? href : "#";
            target = target.Length > 0 ? target : "_blank";
            var style = ExtractAllStyleAttributes(attrs);
            var styleStr = StyleToString(style);

            return $@"<a href=""{href}"" target=""{target}"" style=""{styleStr}"">{inner}</a>";
        }, budget);

        result = ProcessTag(result, "list", (attrs, inner) =>
        {
            var listType = attrs.GetValueOrDefault("type", "unordered");
            var tag = listType == "ordered" ? "ol" : "ul";
            var style = ExtractAllStyleAttributes(attrs);

            if (!style.ContainsKey("margin"))
            {
                style["margin"] = "0";
            }
            if (attrs.TryGetValue("list-style-type", out var lst))
            {
                style["list-style-type"] = lst;
            }

            var styleStr = StyleToString(style);
            var classStr = ClassAttribute(attrs);
            return $@"<{tag} style=""{styleStr}""{classStr}>{inner}</{tag}>";
        }, budget);

        result = ProcessTag(result, "li", (attrs, inner) =>
        {
            var style = ExtractAllStyleAttributes(attrs);
            var styleStr = StyleToString(style);
            var classStr = ClassAttribute(attrs);
            return $@"<li style=""{styleStr}""{classStr}>{inner}</li>";
        }, budget);

        result = ProcessTag(result, "codeblock", (attrs, inner) => SyntaxHighlighter.ProcessCodeBlock(attrs, inner, budget), budget);

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

        var gapStyles = gapRules.Count > 0
            ? $"@media only screen and (max-width:479px){{{string.Join("", gapRules)}}}"
            : "";

        return (result.Trim(), gapStyles);
    }

    private static string ClassAttribute(Dictionary<string, string> attrs)
    {
        var value = attrs.GetValueOrDefault("class", "");
        if (value.Length == 0)
        {
            value = attrs.GetValueOrDefault("className", "");
        }

        return value.Length == 0 ? "" : $@" class=""{value}""";
    }

    private static string ProcessContainer(Dictionary<string, string> attrs, string inner)
    {
        var style = ExtractAllStyleAttributes(attrs);
        var tdStyle = new Dictionary<string, string>();
        var tableStyle = new Dictionary<string, string>();

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

        var hasBorderRadius = tdStyle.ContainsKey("border-radius") ||
                              tdStyle.ContainsKey("border-top-left-radius") ||
                              tdStyle.ContainsKey("border-top-right-radius") ||
                              tdStyle.ContainsKey("border-bottom-left-radius") ||
                              tdStyle.ContainsKey("border-bottom-right-radius");

        if (hasBorderRadius)
        {
            tableStyle["border-collapse"] = "separate";
        }

        if (tableStyle.TryGetValue("width", out var widthVal) && widthVal != "100%" && widthVal != "auto")
        {
            if (!tableStyle.ContainsKey("max-width"))
                tableStyle["max-width"] = widthVal;
            tableStyle["width"] = "100%";
        }

        var tableStyleStr = StyleToString(tableStyle);
        var tdStyleStr = StyleToString(tdStyle);

        // No `align="center"` on container: centering is opt-in via the
        // user's markup (e.g. `margin="0 auto"`). Editor preview and the
        // rendered email follow the same "no implicit centering" contract.
        return $@"<table width=""100%"" border=""0"" cellPadding=""0"" cellSpacing=""0"" role=""presentation"" style=""{tableStyleStr}"">
<tbody>
<tr style=""width:100%"">
<td style=""{tdStyleStr}"">{inner}</td>
</tr>
</tbody>
</table>";
    }

    /// <summary>
    /// Outlook button padding hack ported from react-email's Button component
    /// (https://github.com/resend/react-email/blob/main/packages/components/src/button.tsx).
    /// Uses mso-padding-alt:0, hairspace runs scaled by mso-font-width, and
    /// mso-text-raise to get Outlook to honour horizontal/vertical padding.
    /// </summary>
    private static string ProcessButton(Dictionary<string, string> attrs, string inner)
    {
        var href = attrs.GetValueOrDefault("href", "");
        href = href.Length > 0 ? href : "#";
        var style = ExtractAllStyleAttributes(attrs);

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

        foreach (var kvp in style)
        {
            buttonStyle[kvp.Key] = kvp.Value;
        }

        // Re-emit padding after the merge so the parsed per-side values win
        // over any shorthand copied in from the extracted styles.
        buttonStyle["padding-top"] = $"{paddingTop}px";
        buttonStyle["padding-right"] = $"{paddingRight}px";
        buttonStyle["padding-bottom"] = $"{paddingBottom}px";
        buttonStyle["padding-left"] = $"{paddingLeft}px";

        var styleStr = StyleToString(buttonStyle);

        var leftMsoSpaces = "";
        var rightMsoSpaces = "";

        for (int i = 0; i < plSpaceCount; i++)
        {
            leftMsoSpaces += "&#8202;";
        }

        for (int i = 0; i < prSpaceCount; i++)
        {
            rightMsoSpaces += "&#8202;";
        }

        return $@"<a href=""{href}"" target=""_blank"" style=""{styleStr}""><!--[if mso]><i style=""mso-font-width:{Math.Floor(plFontWidth * 100 + 0.5)}%;mso-text-raise:{textRaise}"" hidden>{leftMsoSpaces}</i><![endif]--><span style=""max-width:100%;display:inline-block;line-height:120%;mso-padding-alt:0px;mso-text-raise:{PxToPt(paddingBottom)}"">{inner}</span><!--[if mso]><i style=""mso-font-width:{Math.Floor(prFontWidth * 100 + 0.5)}%"" hidden>{rightMsoSpaces}&#8203;</i><![endif]--></a>";
    }

    private static (int, int, int, int) ParsePadding(Dictionary<string, string> style)
    {
        int pt = 0, pr = 0, pb = 0, pl = 0;

        if (style.TryGetValue("padding", out var padding))
        {
            var parts = Regex.Split(padding.Trim(), @"\s+");
            switch (parts.Length)
            {
                case 1:
                    {
                        var val = ParsePx(parts[0]);
                        pt = pr = pb = pl = val;
                        break;
                    }
                case 2:
                    {
                        var vertical = ParsePx(parts[0]);
                        var horizontal = ParsePx(parts[1]);
                        pt = vertical; pr = horizontal; pb = vertical; pl = horizontal;
                        break;
                    }
                case 3:
                    pt = ParsePx(parts[0]);
                    pr = ParsePx(parts[1]);
                    pb = ParsePx(parts[2]);
                    pl = ParsePx(parts[1]);
                    break;
                case 4:
                    pt = ParsePx(parts[0]);
                    pr = ParsePx(parts[1]);
                    pb = ParsePx(parts[2]);
                    pl = ParsePx(parts[3]);
                    break;
            }
        }
        if (style.TryGetValue("padding-top", out var ptv))
        {
            pt = ParsePx(ptv);
        }
        if (style.TryGetValue("padding-right", out var prv))
        {
            pr = ParsePx(prv);
        }
        if (style.TryGetValue("padding-bottom", out var pbv))
        {
            pb = ParsePx(pbv);
        }
        if (style.TryGetValue("padding-left", out var plv))
        {
            pl = ParsePx(plv);
        }

        return (pt, pr, pb, pl);
    }

    private static int ParsePx(string s)
    {
        var prefix = Regex.Match(s, @"^\s*[+-]?[0-9]+").Value;

        return int.TryParse(prefix, out var val) ? val : 0;
    }

    private static int PxToPt(int px)
    {
        return (int)Math.Floor((px * 3 + 2) / 4.0);
    }

    private static (double, int) ComputeFontWidthAndSpaceCount(int expectedWidth)
    {
        if (expectedWidth <= 0)
        {
            return (0, 0);
        }

        var spaceCount = Math.Clamp((int)Math.Ceiling(expectedWidth / 10.0), 1, 1000);
        return ((double)expectedWidth / spaceCount / 2.0, spaceCount);
    }

    private static string ProcessTag(string content, string tagName, Func<Dictionary<string, string>, string, string> processor, RenderBudget budget)
    {
        var result = content;
        var closeTag = $"</{tagName}>";
        var openTagStart = $"<{tagName}";
        var openRe = new Regex($@"<{tagName}([^>]*)>", RegexOptions.IgnoreCase);

        for (var iteration = 0; iteration < RenderBudget.MaxTagIterations; iteration++)
        {
            budget.ChargeScanWork(result.Length);
            var matches = openRe.Matches(result);

            if (matches.Count == 0)
            {
                break;
            }

            var replacements = new List<(int Start, int End, string Text)>();
            var consumedUntil = 0;

            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                var start = match.Index;

                if (start < consumedUntil)
                {
                    continue;
                }

                var innerStart = match.Index + match.Length;
                var closePos = result.IndexOf(closeTag, innerStart, StringComparison.OrdinalIgnoreCase);

                if (closePos == -1)
                {
                    budget.ChargeScanWork(result.Length - innerStart);
                    continue;
                }
                budget.ChargeScanWork(closePos - innerStart);

                var inner = result.Substring(innerStart, closePos - innerStart);

                if (inner.Contains(openTagStart, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var end = closePos + closeTag.Length;
                replacements.Add((start, end, processor(ParseAttributes(match.Groups[1].Value), inner)));
                consumedUntil = end;
            }

            if (replacements.Count == 0)
            {
                break;
            }

            var rebuilt = new StringBuilder();
            var cursor = 0;

            foreach (var r in replacements)
            {
                rebuilt.Append(result, cursor, r.Start - cursor);
                rebuilt.Append(r.Text);
                cursor = r.End;
                if (rebuilt.Length > RenderBudget.MaxRenderOutput)
                {
                    throw new ArgumentException($"Email markup expands beyond the maximum size of {RenderBudget.MaxRenderOutput} bytes");
                }
            }
            rebuilt.Append(result.Substring(cursor));
            var next = rebuilt.ToString();
            if (next == result)
            {
                break;
            }
            result = next;
        }

        return result;
    }

    private static readonly HashSet<string> DepthTags = new()
    {
        "block", "section", "column", "row", "container", "heading", "paragraph",
        "text", "button", "link", "sevk-link", "list", "li", "codeblock"
    };

    private static void AssertMarkupDepth(string content)
    {
        var depths = new Dictionary<string, int>();
        var index = 0;

        while (index < content.Length)
        {
            var open = content.IndexOf('<', index);
            if (open < 0)
            {
                break;
            }
            var close = content.IndexOf('>', open + 1);
            if (close < 0)
            {
                break;
            }
            index = close + 1;

            var position = open + 1;
            var closing = content[position] == '/';
            if (closing)
            {
                position++;
            }
            var nameEnd = position;
            while (nameEnd < close && IsTagNameChar(content[nameEnd]))
            {
                nameEnd++;
            }
            var name = content.Substring(position, nameEnd - position).ToLowerInvariant();
            if (!DepthTags.Contains(name))
            {
                continue;
            }

            depths.TryGetValue(name, out var depth);
            if (closing)
            {
                if (depth > 0)
                {
                    depths[name] = depth - 1;
                }
            }
            else if (content[close - 1] != '/')
            {
                depth++;
                if (depth > RenderBudget.MaxMarkupDepth)
                {
                    throw new ArgumentException($"Email markup nesting exceeds the maximum depth of {RenderBudget.MaxMarkupDepth}");
                }
                depths[name] = depth;
            }
        }
    }

    private static bool IsTagNameChar(char c)
    {
        return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '-';
    }
}
