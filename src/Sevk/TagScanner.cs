using System.Text;
using System.Text.RegularExpressions;

namespace Sevk.Markup;

internal static class TagScanner
{
    private static readonly Regex DivOpen = new("<div", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex DivClose = new("</div>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex HiddenStyle = new(@"style=[""'][^""']*display:\s*none", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string ReplaceSelfClosing(string source, string tag, bool selfCloseOnly, Func<string, string> processor)
    {
        var opening = OpeningPattern(tag);
        var nameLength = tag.Length + 1;
        var output = new StringBuilder();
        var lastIndex = 0;
        var position = 0;

        while (position <= source.Length)
        {
            var match = opening.Match(source, position);
            if (!match.Success)
            {
                break;
            }
            var open = match.Index;
            var closingBracket = source.IndexOf('>', open + nameLength);
            if (closingBracket == -1)
            {
                break;
            }
            position = closingBracket + 1;
            if (selfCloseOnly && source[closingBracket - 1] != '/')
            {
                continue;
            }

            var attributesEnd = selfCloseOnly ? closingBracket - 1 : closingBracket;
            output.Append(source, lastIndex, open - lastIndex);
            output.Append(processor(source.Substring(open + nameLength, attributesEnd - open - nameLength)));
            lastIndex = closingBracket + 1;
        }

        return output.Append(source, lastIndex, source.Length - lastIndex).ToString();
    }

    public static string? GetFirstTagContent(string source, string tag)
    {
        var opening = OpeningPattern(tag).Match(source);
        if (!opening.Success)
        {
            return null;
        }
        var closingBracket = source.IndexOf('>', opening.Index + tag.Length + 1);
        if (closingBracket == -1)
        {
            return null;
        }
        var closing = ClosingPattern(tag).Match(source, closingBracket + 1);
        if (!closing.Success)
        {
            return null;
        }
        return source.Substring(closingBracket + 1, closing.Index - closingBracket - 1);
    }

    public static List<string> GetAllTagContents(string source, string tag)
    {
        var opening = OpeningPattern(tag);
        var closing = ClosingPattern(tag);
        var contents = new List<string>();
        var position = 0;

        while (position <= source.Length)
        {
            var match = opening.Match(source, position);
            if (!match.Success)
            {
                break;
            }
            var closingBracket = source.IndexOf('>', match.Index + tag.Length + 1);
            if (closingBracket == -1)
            {
                break;
            }
            var close = closing.Match(source, closingBracket + 1);
            if (!close.Success)
            {
                break;
            }
            contents.Add(source.Substring(closingBracket + 1, close.Index - closingBracket - 1));
            position = close.Index + close.Length;
        }

        return contents;
    }

    public static string StripTagPairs(string source, string tag)
    {
        var opening = OpeningPattern(tag);
        var closing = ClosingPattern(tag);
        var output = new StringBuilder();
        var lastIndex = 0;
        var position = 0;

        while (position <= source.Length)
        {
            var match = opening.Match(source, position);
            if (!match.Success)
            {
                break;
            }
            var closingBracket = source.IndexOf('>', match.Index + tag.Length + 1);
            if (closingBracket == -1)
            {
                break;
            }
            var close = closing.Match(source, closingBracket + 1);
            if (!close.Success)
            {
                break;
            }
            output.Append(source, lastIndex, match.Index - lastIndex);
            lastIndex = close.Index + close.Length;
            position = lastIndex;
        }

        return output.Append(source, lastIndex, source.Length - lastIndex).ToString();
    }

    public static List<string> GetTagAttributes(string source, string tag)
    {
        var opening = OpeningPattern(tag);
        var attributes = new List<string>();
        var position = 0;

        while (position <= source.Length)
        {
            var match = opening.Match(source, position);
            if (!match.Success)
            {
                break;
            }
            var closingBracket = source.IndexOf('>', match.Index + tag.Length + 1);
            if (closingBracket == -1)
            {
                break;
            }
            attributes.Add(source.Substring(match.Index + tag.Length + 1, closingBracket - match.Index - tag.Length - 1));
            position = closingBracket + 1;
        }

        return attributes;
    }

    public static (string Inner, string Full)? FindHiddenPreviewDiv(string html)
    {
        var position = 0;

        while (position <= html.Length)
        {
            var opening = DivOpen.Match(html, position);
            if (!opening.Success)
            {
                return null;
            }
            var open = opening.Index;
            var closingBracket = html.IndexOf('>', open + 4);
            if (closingBracket == -1)
            {
                return null;
            }
            if (HiddenStyle.IsMatch(html.Substring(open, closingBracket - open + 1)))
            {
                var closing = DivClose.Match(html, closingBracket + 1);
                if (!closing.Success)
                {
                    return null;
                }
                return (html.Substring(closingBracket + 1, closing.Index - closingBracket - 1), html.Substring(open, closing.Index + closing.Length - open));
            }
            position = closingBracket + 1;
        }

        return null;
    }

    private static Regex OpeningPattern(string tag)
    {
        return new Regex("<" + Regex.Escape(tag), RegexOptions.IgnoreCase);
    }

    private static Regex ClosingPattern(string tag)
    {
        return new Regex("</" + Regex.Escape(tag) + ">", RegexOptions.IgnoreCase);
    }
}
