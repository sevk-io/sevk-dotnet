using ColorCode.Common;
using ColorCode.Styling;

namespace Sevk.Markup;

internal static class CodeBlockThemes
{
    private static readonly Dictionary<string, KeyValuePair<string, string>[]> Bases = new()
    {
        ["vscDarkPlus"] = new[]
        {
            Entry("color", "#d4d4d4"),
            Entry("fontSize", "13px"),
            Entry("textShadow", "none"),
            Entry("fontFamily", "Menlo, Monaco, Consolas, 'Andale Mono', 'Ubuntu Mono', 'Courier New', monospace"),
            Entry("direction", "ltr"),
            Entry("textAlign", "left"),
            Entry("whiteSpace", "pre"),
            Entry("wordSpacing", "normal"),
            Entry("wordBreak", "normal"),
            Entry("lineHeight", "1.5"),
            Entry("MozTabSize", "4"),
            Entry("OTabSize", "4"),
            Entry("tabSize", "4"),
            Entry("WebkitHyphens", "none"),
            Entry("MozHyphens", "none"),
            Entry("hyphens", "none"),
            Entry("padding", "16px"),
            Entry("overflow", "auto"),
            Entry("backgroundColor", "#1e1e1e"),
        },
        ["vs"] = new[]
        {
            Entry("color", "#393A34"),
            Entry("fontFamily", "'Consolas', 'Bitstream Vera Sans Mono', 'Courier New', Courier, monospace"),
            Entry("direction", "ltr"),
            Entry("textAlign", "left"),
            Entry("whiteSpace", "pre"),
            Entry("wordSpacing", "normal"),
            Entry("wordBreak", "normal"),
            Entry("fontSize", ".9em"),
            Entry("lineHeight", "1.2em"),
            Entry("MozTabSize", "4"),
            Entry("OTabSize", "4"),
            Entry("tabSize", "4"),
            Entry("WebkitHyphens", "none"),
            Entry("MozHyphens", "none"),
            Entry("hyphens", "none"),
            Entry("padding", "16px"),
            Entry("overflow", "auto"),
            Entry("border", "1px solid #dddddd"),
            Entry("backgroundColor", "white"),
        },
        ["oneDark"] = new[]
        {
            Entry("backgroundColor", "#282c34"),
            Entry("color", "#abb2bf"),
            Entry("textShadow", "0 1px rgba(0, 0, 0, 0.3)"),
            Entry("fontFamily", "'Fira Code', 'Fira Mono', Menlo, Consolas, 'DejaVu Sans Mono', monospace"),
            Entry("direction", "ltr"),
            Entry("textAlign", "left"),
            Entry("whiteSpace", "pre"),
            Entry("wordSpacing", "normal"),
            Entry("wordBreak", "normal"),
            Entry("lineHeight", "1.5"),
            Entry("MozTabSize", "2"),
            Entry("OTabSize", "2"),
            Entry("tabSize", "2"),
            Entry("WebkitHyphens", "none"),
            Entry("MozHyphens", "none"),
            Entry("hyphens", "none"),
            Entry("padding", "16px"),
            Entry("overflow", "auto"),
            Entry("borderRadius", "6px"),
        },
        ["oneLight"] = new[]
        {
            Entry("backgroundColor", "#fafafa"),
            Entry("color", "#383a42"),
            Entry("fontFamily", "'Fira Code', 'Fira Mono', Menlo, Consolas, 'DejaVu Sans Mono', monospace"),
            Entry("direction", "ltr"),
            Entry("textAlign", "left"),
            Entry("whiteSpace", "pre"),
            Entry("wordSpacing", "normal"),
            Entry("wordBreak", "normal"),
            Entry("lineHeight", "1.5"),
            Entry("MozTabSize", "2"),
            Entry("OTabSize", "2"),
            Entry("tabSize", "2"),
            Entry("WebkitHyphens", "none"),
            Entry("MozHyphens", "none"),
            Entry("hyphens", "none"),
            Entry("padding", "16px"),
            Entry("overflow", "auto"),
            Entry("borderRadius", "6px"),
        },
    };

    private static readonly Dictionary<string, Dictionary<string, KeyValuePair<string, string>[]>> Tokens = new()
    {
        ["vscDarkPlus"] = new()
        {
            ["comment"] = Color("#6a9955"),
            ["prolog"] = Color("#6a9955"),
            ["punctuation"] = Color("#d4d4d4"),
            ["property"] = Color("#9cdcfe"),
            ["tag"] = Color("#569cd6"),
            ["boolean"] = Color("#569cd6"),
            ["number"] = Color("#b5cea8"),
            ["constant"] = Color("#9cdcfe"),
            ["symbol"] = Color("#b5cea8"),
            ["inserted"] = Color("#b5cea8"),
            ["unit"] = Color("#b5cea8"),
            ["selector"] = Color("#d7ba7d"),
            ["attr-name"] = Color("#9cdcfe"),
            ["string"] = Color("#ce9178"),
            ["char"] = Color("#ce9178"),
            ["builtin"] = Color("#ce9178"),
            ["deleted"] = Color("#ce9178"),
            ["operator"] = Color("#d4d4d4"),
            ["entity"] = Color("#569cd6"),
            ["atrule"] = Color("#ce9178"),
            ["keyword"] = Color("#569CD6"),
            ["function"] = Color("#dcdcaa"),
            ["regex"] = Color("#d16969"),
            ["important"] = Color("#569cd6"),
            ["italic"] = new[] { Entry("fontStyle", "italic") },
            ["class-name"] = Color("#4ec9b0"),
            ["maybe-class-name"] = Color("#4ec9b0"),
            ["console"] = Color("#9cdcfe"),
            ["parameter"] = Color("#9cdcfe"),
            ["interpolation"] = Color("#9cdcfe"),
            ["variable"] = Color("#9cdcfe"),
            ["escape"] = Color("#d7ba7d"),
            ["cdata"] = Color("#808080"),
            ["attr-value"] = Color("#ce9178"),
            ["namespace"] = Color("#4ec9b0"),
        },
        ["vs"] = new()
        {
            ["comment"] = new[] { Entry("color", "#008000"), Entry("fontStyle", "italic") },
            ["prolog"] = new[] { Entry("color", "#008000"), Entry("fontStyle", "italic") },
            ["doctype"] = new[] { Entry("color", "#008000"), Entry("fontStyle", "italic") },
            ["cdata"] = new[] { Entry("color", "#008000"), Entry("fontStyle", "italic") },
            ["namespace"] = new[] { Entry("opacity", ".7") },
            ["string"] = Color("#A31515"),
            ["punctuation"] = Color("#393A34"),
            ["operator"] = Color("#393A34"),
            ["url"] = Color("#36acaa"),
            ["symbol"] = Color("#36acaa"),
            ["number"] = Color("#36acaa"),
            ["boolean"] = Color("#36acaa"),
            ["variable"] = Color("#36acaa"),
            ["constant"] = Color("#36acaa"),
            ["inserted"] = Color("#36acaa"),
            ["atrule"] = Color("#0000ff"),
            ["keyword"] = Color("#0000ff"),
            ["attr-value"] = Color("#0000ff"),
            ["function"] = Color("#393A34"),
            ["deleted"] = Color("#9a050f"),
            ["selector"] = Color("#800000"),
            ["important"] = new[] { Entry("color", "#e90"), Entry("fontWeight", "bold") },
            ["bold"] = new[] { Entry("fontWeight", "bold") },
            ["italic"] = new[] { Entry("fontStyle", "italic") },
            ["class-name"] = Color("#2B91AF"),
            ["tag"] = Color("#800000"),
            ["attr-name"] = Color("#ff0000"),
            ["property"] = Color("#ff0000"),
            ["regex"] = Color("#ff0000"),
            ["entity"] = Color("#ff0000"),
        },
        ["oneDark"] = new()
        {
            ["comment"] = new[] { Entry("color", "#5c6370"), Entry("fontStyle", "italic") },
            ["prolog"] = Color("#5c6370"),
            ["cdata"] = Color("#5c6370"),
            ["doctype"] = Color("#abb2bf"),
            ["punctuation"] = Color("#abb2bf"),
            ["entity"] = new[] { Entry("color", "#abb2bf"), Entry("cursor", "help") },
            ["attr-name"] = Color("#d19a66"),
            ["class-name"] = Color("#d19a66"),
            ["boolean"] = Color("#d19a66"),
            ["constant"] = Color("#d19a66"),
            ["number"] = Color("#d19a66"),
            ["atrule"] = Color("#d19a66"),
            ["keyword"] = Color("#c678dd"),
            ["property"] = Color("#e06c75"),
            ["tag"] = Color("#e06c75"),
            ["symbol"] = Color("#e06c75"),
            ["deleted"] = Color("#e06c75"),
            ["important"] = Color("#e06c75"),
            ["selector"] = Color("#98c379"),
            ["string"] = Color("#98c379"),
            ["char"] = Color("#98c379"),
            ["builtin"] = Color("#98c379"),
            ["inserted"] = Color("#98c379"),
            ["regex"] = Color("#98c379"),
            ["attr-value"] = Color("#98c379"),
            ["variable"] = Color("#61afef"),
            ["operator"] = Color("#61afef"),
            ["function"] = Color("#61afef"),
            ["url"] = Color("#56b6c2"),
            ["bold"] = new[] { Entry("fontWeight", "bold") },
            ["italic"] = new[] { Entry("fontStyle", "italic") },
            ["namespace"] = new[] { Entry("opacity", "0.8") },
        },
        ["oneLight"] = new()
        {
            ["comment"] = new[] { Entry("color", "#a0a1a7"), Entry("fontStyle", "italic") },
            ["prolog"] = Color("#a0a1a7"),
            ["cdata"] = Color("#a0a1a7"),
            ["doctype"] = Color("#383a42"),
            ["punctuation"] = Color("#383a42"),
            ["entity"] = new[] { Entry("color", "#383a42"), Entry("cursor", "help") },
            ["attr-name"] = Color("#b76b01"),
            ["class-name"] = Color("#b76b01"),
            ["boolean"] = Color("#b76b01"),
            ["constant"] = Color("#b76b01"),
            ["number"] = Color("#b76b01"),
            ["atrule"] = Color("#b76b01"),
            ["keyword"] = Color("#a626a4"),
            ["property"] = Color("#e45649"),
            ["tag"] = Color("#e45649"),
            ["symbol"] = Color("#e45649"),
            ["deleted"] = Color("#e45649"),
            ["important"] = Color("#e45649"),
            ["selector"] = Color("#50a14f"),
            ["string"] = Color("#50a14f"),
            ["char"] = Color("#50a14f"),
            ["builtin"] = Color("#50a14f"),
            ["inserted"] = Color("#50a14f"),
            ["regex"] = Color("#50a14f"),
            ["attr-value"] = Color("#50a14f"),
            ["variable"] = Color("#4078f2"),
            ["operator"] = Color("#4078f2"),
            ["function"] = Color("#4078f2"),
            ["url"] = Color("#0184bc"),
            ["bold"] = new[] { Entry("fontWeight", "bold") },
            ["italic"] = new[] { Entry("fontStyle", "italic") },
            ["namespace"] = new[] { Entry("opacity", "0.8") },
        },
    };

    private static readonly KeyValuePair<string, string>[] ScopeTokens =
    {
        Entry(ScopeName.Keyword, "keyword"),
        Entry(ScopeName.ControlKeyword, "keyword"),
        Entry(ScopeName.PreprocessorKeyword, "keyword"),
        Entry(ScopeName.PseudoKeyword, "keyword"),
        Entry(ScopeName.SqlSystemFunction, "function"),
        Entry(ScopeName.String, "string"),
        Entry(ScopeName.StringCSharpVerbatim, "string"),
        Entry(ScopeName.StringEscape, "escape"),
        Entry(ScopeName.Comment, "comment"),
        Entry(ScopeName.HtmlComment, "comment"),
        Entry(ScopeName.XmlComment, "comment"),
        Entry(ScopeName.XmlDocComment, "comment"),
        Entry(ScopeName.XmlDocTag, "comment"),
        Entry(ScopeName.Number, "number"),
        Entry(ScopeName.JsonNumber, "number"),
        Entry(ScopeName.ClassName, "class-name"),
        Entry(ScopeName.Type, "class-name"),
        Entry(ScopeName.TypeVariable, "class-name"),
        Entry(ScopeName.Attribute, "class-name"),
        Entry(ScopeName.NameSpace, "namespace"),
        Entry(ScopeName.Predefined, "builtin"),
        Entry(ScopeName.BuiltinValue, "boolean"),
        Entry(ScopeName.BuiltinFunction, "function"),
        Entry(ScopeName.Constructor, "function"),
        Entry(ScopeName.Operator, "operator"),
        Entry(ScopeName.Delimiter, "punctuation"),
        Entry(ScopeName.HtmlTagDelimiter, "punctuation"),
        Entry(ScopeName.HtmlElementName, "tag"),
        Entry(ScopeName.HtmlAttributeName, "attr-name"),
        Entry(ScopeName.HtmlAttributeValue, "attr-value"),
        Entry(ScopeName.HtmlOperator, "punctuation"),
        Entry(ScopeName.HtmlEntity, "entity"),
        Entry(ScopeName.XmlDelimiter, "punctuation"),
        Entry(ScopeName.XmlName, "tag"),
        Entry(ScopeName.XmlAttribute, "attr-name"),
        Entry(ScopeName.XmlAttributeValue, "attr-value"),
        Entry(ScopeName.XmlAttributeQuotes, "attr-value"),
        Entry(ScopeName.XmlCDataSection, "cdata"),
        Entry(ScopeName.CssSelector, "selector"),
        Entry(ScopeName.CssPropertyName, "property"),
        Entry(ScopeName.JsonKey, "property"),
        Entry(ScopeName.JsonString, "string"),
        Entry(ScopeName.JsonConst, "boolean"),
        Entry(ScopeName.MarkdownEmph, "italic"),
        Entry(ScopeName.MarkdownBold, "bold"),
        Entry(ScopeName.PowerShellAttribute, "class-name"),
        Entry(ScopeName.PowerShellOperator, "operator"),
        Entry(ScopeName.PowerShellType, "class-name"),
        Entry(ScopeName.PowerShellVariable, "variable"),
        Entry(ScopeName.PowerShellCommand, "function"),
        Entry(ScopeName.PowerShellParameter, "parameter"),
    };

    private static readonly Dictionary<string, StyleDictionary> Styles = Bases.Keys.ToDictionary(name => name, BuildStyles);

    public static string Resolve(string name)
    {
        return Bases.ContainsKey(name) ? name : "oneDark";
    }

    public static KeyValuePair<string, string>[] Base(string name)
    {
        return Bases[Resolve(name)];
    }

    public static StyleDictionary TokenStyles(string name)
    {
        return Styles[Resolve(name)];
    }

    private static KeyValuePair<string, string> Entry(string key, string value)
    {
        return new KeyValuePair<string, string>(key, value);
    }

    private static KeyValuePair<string, string>[] Color(string hex)
    {
        return new[] { Entry("color", hex) };
    }

    private static string Argb(string color)
    {
        return color == "white" ? "#FFffffff" : "#FF" + color.Substring(1);
    }

    private static StyleDictionary BuildStyles(string name)
    {
        var tokens = Tokens[name];
        var baseStyle = Bases[name].ToDictionary(entry => entry.Key, entry => entry.Value);
        var styles = new StyleDictionary
        {
            new Style(ScopeName.PlainText) { Foreground = Argb(baseStyle["color"]), Background = Argb(baseStyle["backgroundColor"]) },
        };

        foreach (var scope in ScopeTokens)
        {
            var token = scope.Value == "escape" && !tokens.ContainsKey("escape") ? "string" : scope.Value;
            if (!tokens.TryGetValue(token, out var declarations))
            {
                continue;
            }

            var style = new Style(scope.Key);
            foreach (var declaration in declarations)
            {
                switch (declaration.Key)
                {
                    case "color":
                        style.Foreground = Argb(declaration.Value);
                        break;
                    case "fontStyle":
                        style.Italic = declaration.Value == "italic";
                        break;
                    case "fontWeight":
                        style.Bold = declaration.Value == "bold";
                        break;
                }
            }
            styles.Add(style);
        }

        return styles;
    }
}
