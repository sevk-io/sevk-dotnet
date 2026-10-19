using System.Linq;
using System.Text.RegularExpressions;

namespace Sevk.Markup;

public static partial class Renderer
{
    private static string NormalizeMarkup(string content)
    {
        var result = content;

        result = Regex.Replace(result, @"<link(\s+(?:[\w-]+=(?:""[^""]*""|'[^']*')\s+)*)href=", "<sevk-link${1}href=", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"</link>", "</sevk-link>", RegexOptions.IgnoreCase);

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
    /// Separates a markup document's body from its email head settings.
    /// </summary>
    public static ParsedEmailContent ParseEmailHTML(string content)
    {
        if (Regex.IsMatch(content, @"<(?:email|mail)[\s>]", RegexOptions.IgnoreCase))
        {
            return ParseSevkMarkup(content);
        }

        return ParseHtmlDocument(content);
    }

    private static ParsedEmailContent ParseHtmlDocument(string content)
    {
        var headSettings = new EmailHeadSettings
        {
            Title = (TagScanner.GetFirstTagContent(content, "title") ?? "").Trim(),
            Styles = string.Join("\n", TagScanner.GetAllTagContents(content, "style")),
        };

        foreach (var attributes in TagScanner.GetTagAttributes(content, "link"))
        {
            var hrefMatch = Regex.Match(attributes, @"href=[""']([^""']*)[""']", RegexOptions.IgnoreCase);
            if (!hrefMatch.Success || !hrefMatch.Groups[1].Value.ToLowerInvariant().Contains("fonts"))
            {
                continue;
            }

            var url = hrefMatch.Groups[1].Value;
            var index = headSettings.Fonts.Count;
            var familyMatch = Regex.Match(url, @"family=([^:&]+)");
            headSettings.Fonts.Add(new FontConfig
            {
                Id = $"font-{index}",
                Name = familyMatch.Success ? familyMatch.Groups[1].Value.Replace("+", " ") : $"Font {index + 1}",
                Url = url
            });
        }

        var preview = TagScanner.FindHiddenPreviewDiv(content);
        var body = TagScanner.GetFirstTagContent(content, "body") ?? content;

        if (preview is { } hidden)
        {
            headSettings.PreviewText = hidden.Inner.Trim();
            body = ReplaceFirst(body, hidden.Full);
        }

        return new ParsedEmailContent { Body = body.Trim(), HeadSettings = headSettings };
    }

    private static ParsedEmailContent ParseSevkMarkup(string content)
    {
        var headSettings = new EmailHeadSettings();

        var rootAttributes = TagScanner.GetTagAttributes(content, "email");
        if (rootAttributes.Count == 0)
        {
            rootAttributes = TagScanner.GetTagAttributes(content, "mail");
        }
        if (rootAttributes.Count > 0)
        {
            var langMatch = Regex.Match(rootAttributes[0], @"lang=[""']([^""']*)[""']", RegexOptions.IgnoreCase);
            var dirMatch = Regex.Match(rootAttributes[0], @"dir=[""']([^""']*)[""']", RegexOptions.IgnoreCase);

            if (langMatch.Success)
            {
                headSettings.Lang = langMatch.Groups[1].Value;
            }
            if (dirMatch.Success)
            {
                headSettings.Dir = dirMatch.Groups[1].Value;
            }
        }

        headSettings.Title = (TagScanner.GetFirstTagContent(content, "title") ?? "").Trim();
        headSettings.PreviewText = (TagScanner.GetFirstTagContent(content, "preview") ?? "").Trim();
        headSettings.Styles = (TagScanner.GetFirstTagContent(content, "style") ?? "").Trim();

        var seenFontUrls = new HashSet<string>();

        foreach (var attributes in TagScanner.GetTagAttributes(content, "font"))
        {
            var nameMatch = Regex.Match(attributes, @"name=[""']([^""']*)[""']", RegexOptions.IgnoreCase);
            var urlMatch = Regex.Match(attributes, @"url=[""']([^""']*)[""']", RegexOptions.IgnoreCase);
            if (!nameMatch.Success || !urlMatch.Success || !seenFontUrls.Add(urlMatch.Groups[1].Value))
            {
                continue;
            }

            headSettings.Fonts.Add(new FontConfig
            {
                Id = $"font-{headSettings.Fonts.Count}",
                Name = nameMatch.Groups[1].Value,
                Url = urlMatch.Groups[1].Value
            });
        }

        var body = (TagScanner.GetFirstTagContent(content, "body") ?? "").Trim();

        if (body.Length == 0)
        {
            var stripped = TagScanner.ReplaceSelfClosing(content, "email", false, _ => "");
            stripped = Regex.Replace(TagScanner.ReplaceSelfClosing(stripped, "mail", false, _ => ""), @"</(?:email|mail)>", "", RegexOptions.IgnoreCase);
            foreach (var tag in new[] { "head", "title", "preview", "style" })
            {
                stripped = TagScanner.StripTagPairs(stripped, tag);
            }
            body = Regex.Replace(TagScanner.ReplaceSelfClosing(stripped, "font", false, _ => ""), @"</font>", "", RegexOptions.IgnoreCase).Trim();
        }

        return new ParsedEmailContent { Body = body, HeadSettings = headSettings };
    }

    private static string ReplaceFirst(string value, string needle)
    {
        var index = value.IndexOf(needle, StringComparison.Ordinal);

        return index < 0 ? value : value.Remove(index, needle.Length);
    }
}
