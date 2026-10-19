using System.Text.RegularExpressions;

namespace Sevk.Markup;

public static partial class Renderer
{
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

    internal static Dictionary<string, string> ExtractAllStyleAttributes(Dictionary<string, string> source)
    {
        var attrs = source.Where(entry => entry.Value.Length > 0).ToDictionary(entry => entry.Key, entry => entry.Value);
        var style = new Dictionary<string, string>();

        if (attrs.TryGetValue("text-color", out var textColor))
            style["color"] = textColor;
        else if (attrs.TryGetValue("color", out var color))
            style["color"] = color;

        if (attrs.TryGetValue("background-color", out var bgColor))
            style["background-color"] = bgColor;

        // Background image group lives between background-color and font-size to
        // match the reference extractStyleAttributes insertion order.
        var hasBgImage = attrs.TryGetValue("background-image", out var backgroundImage);

        if (hasBgImage)
            style["background-image"] = $"url('{backgroundImage}')";
        if (attrs.TryGetValue("background-size", out var bgSize))
            style["background-size"] = bgSize;
        else if (hasBgImage)
            style["background-size"] = "cover";

        if (attrs.TryGetValue("background-position", out var bgPos))
            style["background-position"] = bgPos;
        else if (hasBgImage)
            style["background-position"] = "center";

        if (attrs.TryGetValue("background-repeat", out var bgRepeat))
            style["background-repeat"] = bgRepeat;
        else if (hasBgImage)
            style["background-repeat"] = "no-repeat";

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

        if (attrs.TryGetValue("padding", out var padding))
            style["padding"] = padding;

        if (attrs.TryGetValue("padding-top", out var pt))
            style["padding-top"] = pt;

        if (attrs.TryGetValue("padding-right", out var pr))
            style["padding-right"] = pr;

        if (attrs.TryGetValue("padding-bottom", out var pb))
            style["padding-bottom"] = pb;

        if (attrs.TryGetValue("padding-left", out var pl))
            style["padding-left"] = pl;

        if (attrs.TryGetValue("margin", out var margin))
            style["margin"] = margin;

        if (attrs.TryGetValue("margin-top", out var mt))
            style["margin-top"] = mt;

        if (attrs.TryGetValue("margin-right", out var mr))
            style["margin-right"] = mr;

        if (attrs.TryGetValue("margin-bottom", out var mb))
            style["margin-bottom"] = mb;

        if (attrs.TryGetValue("margin-left", out var ml))
            style["margin-left"] = ml;

        // Borders cascade: shorthand -> per-property -> per-side (most specific last)
        if (attrs.TryGetValue("border", out var border))
            style["border"] = border;

        if (attrs.TryGetValue("border-color", out var bc))
            style["border-color"] = bc;

        if (attrs.TryGetValue("border-width", out var bw))
            style["border-width"] = bw;

        if (attrs.TryGetValue("border-style", out var bs))
            style["border-style"] = bs;

        if (attrs.TryGetValue("border-top", out var bt))
            style["border-top"] = bt;

        if (attrs.TryGetValue("border-right", out var br))
            style["border-right"] = br;

        if (attrs.TryGetValue("border-bottom", out var bb))
            style["border-bottom"] = bb;

        if (attrs.TryGetValue("border-left", out var bl))
            style["border-left"] = bl;

        if (attrs.TryGetValue("border-radius", out var borderRadius))
            style["border-radius"] = borderRadius;

        if (attrs.TryGetValue("border-top-left-radius", out var btlr))
            style["border-top-left-radius"] = btlr;

        if (attrs.TryGetValue("border-top-right-radius", out var btrr))
            style["border-top-right-radius"] = btrr;

        if (attrs.TryGetValue("border-bottom-left-radius", out var bblr))
            style["border-bottom-left-radius"] = bblr;

        if (attrs.TryGetValue("border-bottom-right-radius", out var bbrr))
            style["border-bottom-right-radius"] = bbrr;

        return style;
    }

    private static readonly string[] StyleOrder = {
        "color", "background-color", "background-image", "background-size", "background-position", "background-repeat",
        "font-size", "font-family", "font-weight", "line-height", "text-align", "text-decoration",
        "width", "height", "max-width", "max-height", "min-width", "min-height",
        "padding", "padding-top", "padding-right", "padding-bottom", "padding-left",
        "margin", "margin-top", "margin-right", "margin-bottom", "margin-left",
        "border", "border-color", "border-width", "border-style",
        "border-top", "border-right", "border-bottom", "border-left",
        "border-radius", "border-top-left-radius", "border-top-right-radius", "border-bottom-left-radius", "border-bottom-right-radius",
        "border-collapse", "list-style-type", "vertical-align"
    };

    internal static string StyleToString(Dictionary<string, string> style)
    {
        var parts = new List<string>();
        var seen = new HashSet<string>();

        foreach (var key in StyleOrder)
        {
            if (!style.TryGetValue(key, out var value))
            {
                continue;
            }
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }
            parts.Add($"{key}:{value}");
            seen.Add(key);
        }
        foreach (var kvp in style)
        {
            if (seen.Contains(kvp.Key))
            {
                continue;
            }
            if (string.IsNullOrEmpty(kvp.Value))
            {
                continue;
            }
            parts.Add($"{kvp.Key}:{kvp.Value}");
        }

        return string.Join(";", parts);
    }
}
