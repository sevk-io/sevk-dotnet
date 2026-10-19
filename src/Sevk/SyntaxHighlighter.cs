using System.Text;
using System.Text.RegularExpressions;
using ColorCode;

namespace Sevk.Markup;

internal static class SyntaxHighlighter
{
    private const string TokenSpace = "\u00A0\u200D\u200B";

    private static readonly HashSet<string> PrismLanguages = new()
    {
        "plain", "extend", "plaintext", "text", "txt", "markup", "html", "mathml", "svg", "xml", "ssml", "atom", "rss",
        "css", "clike", "javascript", "js", "typescript", "ts", "jsx", "tsx", "python", "py", "java", "c", "cpp",
        "csharp", "cs", "dotnet", "php", "ruby", "rb", "go", "rust", "sql", "bash", "sh", "shell", "json",
        "webmanifest", "yaml", "yml", "markdown", "md",
    };

    private static readonly Dictionary<string, string> Aliases = new()
    {
        ["jsx"] = "javascript",
        ["tsx"] = "typescript",
    };

    private static readonly Dictionary<string, ILanguage> ColorCodeLanguages = new()
    {
        ["javascript"] = Languages.JavaScript,
        ["js"] = Languages.JavaScript,
        ["typescript"] = Languages.Typescript,
        ["ts"] = Languages.Typescript,
        ["python"] = Languages.Python,
        ["py"] = Languages.Python,
        ["java"] = Languages.Java,
        ["php"] = Languages.Php,
        ["csharp"] = Languages.CSharp,
        ["cs"] = Languages.CSharp,
        ["dotnet"] = Languages.CSharp,
        ["markup"] = Languages.Html,
        ["html"] = Languages.Html,
        ["svg"] = Languages.Xml,
        ["mathml"] = Languages.Xml,
        ["ssml"] = Languages.Xml,
        ["atom"] = Languages.Xml,
        ["rss"] = Languages.Xml,
        ["xml"] = Languages.Xml,
        ["css"] = Languages.Css,
        ["sql"] = Languages.Sql,
        ["markdown"] = Languages.Markdown,
        ["md"] = Languages.Markdown,
        ["cpp"] = Languages.Cpp,
    };

    private static readonly Regex SpanTag = new(@"<span\b([^>]*)>|</span>", RegexOptions.Compiled);
    private static readonly Regex SpanStyle = new(@"style=""([^""]*)""", RegexOptions.Compiled);

    public static string ProcessCodeBlock(Dictionary<string, string> attrs, string code, RenderBudget budget)
    {
        var requested = attrs.GetValueOrDefault("language", "");
        requested = (requested.Length == 0 ? "javascript" : requested).ToLowerInvariant();
        var language = Aliases.GetValueOrDefault(requested, requested);
        var themeName = attrs.GetValueOrDefault("theme", "");
        themeName = CodeBlockThemes.Resolve(themeName.Length == 0 ? "oneDark" : themeName);
        var customStyle = Renderer.ExtractAllStyleAttributes(attrs);

        if (code.Length == 0)
        {
            return "<pre><code></code></pre>";
        }

        if (!PrismLanguages.Contains(language) || !budget.Admits(code.Length))
        {
            return $"<pre style=\"{PreStyle(themeName, false, customStyle)}\"><code>{code.Replace("<", "&lt;").Replace(">", "&gt;")}</code></pre>";
        }
        budget.Charge(code.Length);

        var inherited = new Dictionary<string, string>();
        var fontFamily = attrs.GetValueOrDefault("font-family", "");
        if (fontFamily.Length > 0)
        {
            inherited["font-family"] = fontFamily;
        }

        var numbered = attrs.GetValueOrDefault("line-numbers", "") == "true";
        var html = new StringBuilder();
        var lines = HighlightLines(code, language, themeName, inherited);
        for (var index = 0; index < lines.Count; index++)
        {
            html.Append("<p style=\"margin:0;min-height:1em\">");
            if (numbered)
            {
                html.Append($"<span style=\"width:2em;display:inline-block\">{index + 1}</span>");
            }
            html.Append(lines[index]).Append("</p>");
        }

        return $"<pre style=\"{PreStyle(themeName, true, customStyle)}\"><code>{html}</code></pre>";
    }

    private static string PreStyle(string themeName, bool highlighted, Dictionary<string, string> customStyle)
    {
        var style = new List<KeyValuePair<string, string>>(CodeBlockThemes.Base(themeName));
        Merge(style, "width", "100%");
        if (highlighted)
        {
            Merge(style, "box-sizing", "border-box");
        }
        foreach (var entry in customStyle)
        {
            Merge(style, entry.Key, entry.Value);
        }

        return string.Join(";", style.Select(entry => $"{KebabProperty(entry.Key)}:{entry.Value}"));
    }

    private static void Merge(List<KeyValuePair<string, string>> style, string key, string value)
    {
        var index = style.FindIndex(entry => entry.Key == key);
        var entry = new KeyValuePair<string, string>(key, value);
        if (index >= 0)
        {
            style[index] = entry;
        }
        else
        {
            style.Add(entry);
        }
    }

    private static string KebabProperty(string key)
    {
        var property = Regex.Replace(key, "[A-Z]", match => "-" + match.Value.ToLowerInvariant());
        return property.StartsWith("ms-", StringComparison.Ordinal) ? "-" + property : property;
    }

    private static List<string> HighlightLines(string code, string language, string themeName, Dictionary<string, string> inherited)
    {
        var inheritedStyle = Renderer.StyleToString(inherited);
        var lines = new List<string> { "" };
        var open = new List<string>();
        var html = Tokenize(code, language, themeName);
        var cursor = 0;

        foreach (Match tag in SpanTag.Matches(html))
        {
            AppendText(lines, open, html.Substring(cursor, tag.Index - cursor), inheritedStyle);
            cursor = tag.Index + tag.Length;
            if (!tag.Groups[1].Success)
            {
                if (open.Count == 0)
                {
                    continue;
                }
                var closed = open[open.Count - 1];
                open.RemoveAt(open.Count - 1);
                if (closed.Length > 0)
                {
                    lines[lines.Count - 1] += "</span>";
                }
                continue;
            }
            var opened = TokenTag(SpanStyle.Match(tag.Groups[1].Value).Groups[1].Value, inherited);
            open.Add(opened);
            lines[lines.Count - 1] += opened;
        }
        AppendText(lines, open, html.Substring(cursor), inheritedStyle);

        return lines;
    }

    private static void AppendText(List<string> lines, List<string> open, string text, string inheritedStyle)
    {
        var wrap = inheritedStyle.Length > 0 && open.All(tag => tag.Length == 0);
        var parts = text.Split('\n');
        for (var index = 0; index < parts.Length; index++)
        {
            if (index > 0)
            {
                var reopened = new StringBuilder();
                foreach (var tag in open.Where(tag => tag.Length > 0))
                {
                    lines[lines.Count - 1] += "</span>";
                    reopened.Append(tag);
                }
                lines.Add(reopened.ToString());
            }
            if (parts[index].Length == 0)
            {
                continue;
            }
            var escaped = parts[index].Replace(" ", TokenSpace);
            lines[lines.Count - 1] += wrap ? $"<span style=\"{inheritedStyle}\">{escaped}</span>" : escaped;
        }
    }

    private static string TokenTag(string spanStyle, Dictionary<string, string> inherited)
    {
        var style = new Dictionary<string, string>(inherited);
        foreach (var declaration in spanStyle.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = declaration.IndexOf(':');
            if (separator > 0)
            {
                style[declaration.Substring(0, separator).Trim()] = declaration.Substring(separator + 1).Trim();
            }
        }

        return style.Count == 0 ? "" : $"<span style=\"{Renderer.StyleToString(style)}\">";
    }

    private static string Tokenize(string code, string language, string themeName)
    {
        var escaped = EscapeText(code);
        if (!ColorCodeLanguages.TryGetValue(language, out var colorCodeLanguage))
        {
            return escaped;
        }

        var formatted = new HtmlFormatter(CodeBlockThemes.TokenStyles(themeName)).GetHtmlString(code, colorCodeLanguage);
        var content = Regex.Match(formatted, @"<pre[^>]*>([\s\S]*?)</pre>", RegexOptions.IgnoreCase);
        if (!content.Success)
        {
            return escaped;
        }

        var inner = content.Groups[1].Value;
        inner = inner.StartsWith("\r\n", StringComparison.Ordinal) ? inner.Substring(2) : inner.TrimStart('\n');
        inner = inner.EndsWith("\r\n", StringComparison.Ordinal) ? inner.Substring(0, inner.Length - 2) : inner.TrimEnd('\n');
        var stripped = Regex.Replace(inner, @"</?span[^>]*>", "");
        return stripped == escaped ? inner : escaped;
    }

    private static string EscapeText(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}
