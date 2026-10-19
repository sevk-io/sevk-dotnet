using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Sevk.Markup;

public static partial class Renderer
{
    private const int MaxIfPasses = 2000;
    private const int MaxEachItems = 1000;
    private const int MaxEachOutput = 2_000_000;
    private const long MaxTemplateWork = 20_000_000;
    private const string OutputExceeded = "Email block exceeds the maximum template output size";

    private static readonly Regex IfPattern = new(@"\{%#if\s+([^%]+)%\}((?:(?!\{%#if\s)[\s\S])*?)\{%/if%\}", RegexOptions.Compiled);
    private static readonly Regex EachOpenPattern = new(@"\{%#each\s+(\w+)(?:\s+as\s+(\w+))?%\}", RegexOptions.Compiled);
    private static readonly Regex FallbackPattern = new(@"\{%(\w+)\s*\?\?([^%]+)%\}", RegexOptions.Compiled);
    private static readonly Regex ValuePattern = new(@"\{%(\w+)%\}", RegexOptions.Compiled);
    private static readonly Regex EqualsPattern = new(@"^(\w+)\s*==\s*""([^""]*)""$", RegexOptions.Compiled);
    private static readonly Regex NotEqualsPattern = new(@"^(\w+)\s*!=\s*""([^""]*)""$", RegexOptions.Compiled);

    private sealed class OutputMeter
    {
        private long rendered;

        private long itemOut;

        public string Charge(string value)
        {
            if (rendered + value.Length > MaxEachOutput)
            {
                throw new ArgumentException(OutputExceeded);
            }
            rendered += value.Length;
            return value;
        }

        public void StartItem()
        {
            itemOut = 0;
        }

        public string ChargeItem(string value)
        {
            if (rendered + itemOut + value.Length > MaxEachOutput)
            {
                throw new ArgumentException(OutputExceeded);
            }
            itemOut += value.Length;
            return value;
        }

        public void FinishItem(int length)
        {
            rendered += length;
            if (rendered > MaxEachOutput)
            {
                throw new ArgumentException(OutputExceeded);
            }
        }
    }

    private static bool EvaluateCondition(string expr, Dictionary<string, object> config)
    {
        var trimmed = expr.Trim();

        if (trimmed.Contains("||"))
        {
            return trimmed.Split("||").Any(part => EvaluateCondition(part, config));
        }

        if (trimmed.Contains("&&"))
        {
            return trimmed.Split("&&").All(part => EvaluateCondition(part, config));
        }

        var eqMatch = EqualsPattern.Match(trimmed);

        if (eqMatch.Success)
        {
            return TemplateText(config.GetValueOrDefault(eqMatch.Groups[1].Value)) == eqMatch.Groups[2].Value;
        }

        var neqMatch = NotEqualsPattern.Match(trimmed);

        if (neqMatch.Success)
        {
            return TemplateText(config.GetValueOrDefault(neqMatch.Groups[1].Value)) != neqMatch.Groups[2].Value;
        }

        return IsTruthy(config.GetValueOrDefault(trimmed));
    }

    private static bool IsTruthy(object? val)
    {
        switch (val)
        {
            case null:
                return false;
            case string s:
                return s.Length > 0;
            case bool b:
                return b;
            case int i:
                return i != 0;
            case long l:
                return l != 0;
            case double d:
                return d != 0;
            case float f:
                return f != 0;
            case JsonElement je:
                switch (je.ValueKind)
                {
                    case JsonValueKind.Null:
                    case JsonValueKind.Undefined:
                    case JsonValueKind.False:
                        return false;
                    case JsonValueKind.Number:
                        return je.GetDouble() != 0;
                    case JsonValueKind.String:
                        return je.GetString()!.Length > 0;
                    case JsonValueKind.Array:
                        return je.GetArrayLength() > 0;
                    default:
                        return true;
                }
            case ICollection col:
                return col.Count > 0;
            default:
                return true;
        }
    }

    private static bool IsMissing(object? val)
    {
        return val switch
        {
            null => true,
            string s => s.Length == 0,
            JsonElement je => je.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
                || (je.ValueKind == JsonValueKind.String && je.GetString()!.Length == 0),
            _ => false,
        };
    }

    private static string TemplateText(object? val)
    {
        switch (val)
        {
            case null:
                return "";
            case string s:
                return s;
            case bool b:
                return b ? "true" : "false";
            case double d:
                return NumberText(d);
            case float f:
                return NumberText(f);
            case JsonElement je:
                switch (je.ValueKind)
                {
                    case JsonValueKind.String:
                        return je.GetString() ?? "";
                    case JsonValueKind.Number:
                        return JsNumber.Format(je.GetDouble());
                    case JsonValueKind.True:
                        return "true";
                    case JsonValueKind.False:
                        return "false";
                    case JsonValueKind.Array:
                        return string.Join(",", je.EnumerateArray().Select(item => item.ValueKind == JsonValueKind.Object ? "[object Object]" : TemplateText(item)));
                    case JsonValueKind.Object:
                        return "[object Object]";
                    default:
                        return "";
                }
            case IDictionary:
                return "[object Object]";
            case IEnumerable items:
                return string.Join(",", items.Cast<object?>().Select(item => item is IDictionary ? "[object Object]" : TemplateText(item)));
            default:
                return Convert.ToString(val, CultureInfo.InvariantCulture) ?? "";
        }
    }

    private static string NumberText(double number)
    {
        return JsNumber.Format(number);
    }

    private static string RenderTemplate(string template, Dictionary<string, object> config)
    {
        var meter = new OutputMeter();
        var result = RenderConditionals(template, config);
        result = RenderLoops(result, config, meter);

        result = FallbackPattern.Replace(result, match =>
        {
            var value = config.GetValueOrDefault(match.Groups[1].Value);

            return meter.Charge(IsMissing(value) ? match.Groups[2].Value.Trim() : TemplateText(value));
        });

        return ValuePattern.Replace(result, match => meter.Charge(TemplateText(config.GetValueOrDefault(match.Groups[1].Value))));
    }

    private static string RenderConditionals(string template, Dictionary<string, object> config)
    {
        var result = template;
        long work = 0;

        for (var pass = 0; pass < MaxIfPasses; pass++)
        {
            work += result.Length;
            if (work > MaxTemplateWork)
            {
                throw new ArgumentException(RenderBudget.TooComplex);
            }

            var match = IfPattern.Match(result);
            if (!match.Success)
            {
                break;
            }

            var body = match.Groups[2].Value;
            var truthy = EvaluateCondition(match.Groups[1].Value, config);
            var elseIndex = body.IndexOf("{%else%}", StringComparison.Ordinal);
            var replacement = elseIndex < 0
                ? (truthy ? body : "")
                : (truthy ? body.Substring(0, elseIndex) : body.Substring(elseIndex + "{%else%}".Length));

            result = result.Substring(0, match.Index) + replacement + result.Substring(match.Index + match.Length);
        }

        return result;
    }

    private static string RenderLoops(string template, Dictionary<string, object> config, OutputMeter meter)
    {
        if (!template.Contains("{%/each%}"))
        {
            return template;
        }

        var output = new StringBuilder();
        var from = 0;
        long work = 0;

        while (from <= template.Length)
        {
            var opening = EachOpenPattern.Match(template, from);
            if (!opening.Success)
            {
                output.Append(template, from, template.Length - from);
                break;
            }

            var bodyStart = opening.Index + opening.Length;
            var closeIndex = template.IndexOf("{%/each%}", bodyStart, StringComparison.Ordinal);
            if (closeIndex < 0)
            {
                output.Append(template, from, template.Length - from);
                break;
            }

            output.Append(template, from, opening.Index - from);
            var items = LoopItems(config.GetValueOrDefault(opening.Groups[1].Value));

            if (items.Count > 0)
            {
                if (items.Count > MaxEachItems)
                {
                    throw new ArgumentException($"Email block exceeds the maximum of {MaxEachItems} loop items");
                }

                var alias = opening.Groups[2].Success ? opening.Groups[2].Value : "this";
                var propertyPattern = new Regex(@"\{%" + Regex.Escape(alias) + @"\.(\w+)%\}");
                var body = template.Substring(bodyStart, closeIndex - bodyStart);

                foreach (var item in items)
                {
                    work += body.Length + 16;
                    if (work > MaxTemplateWork)
                    {
                        throw new ArgumentException(RenderBudget.TooComplex);
                    }
                    output.Append(RenderLoopItem(body, item, config, propertyPattern, meter));
                }
            }

            from = closeIndex + "{%/each%}".Length;
        }

        return output.ToString();
    }

    private static string RenderLoopItem(string body, object? item, Dictionary<string, object> config, Regex propertyPattern, OutputMeter meter)
    {
        meter.StartItem();
        var rendered = propertyPattern.Replace(body, match => meter.ChargeItem(TemplateText(ItemProperty(item, match.Groups[1].Value))));
        rendered = ValuePattern.Replace(rendered, match => meter.ChargeItem(TemplateText(config.GetValueOrDefault(match.Groups[1].Value))));
        meter.FinishItem(rendered.Length);

        return rendered;
    }

    private static List<object?> LoopItems(object? value)
    {
        return value switch
        {
            JsonElement je when je.ValueKind == JsonValueKind.Array => je.EnumerateArray().Cast<object?>().ToList(),
            string => new List<object?>(),
            IDictionary => new List<object?>(),
            IEnumerable items => items.Cast<object?>().ToList(),
            _ => new List<object?>(),
        };
    }

    private static object? ItemProperty(object? item, string property)
    {
        return item switch
        {
            JsonElement je when je.ValueKind == JsonValueKind.Object => je.TryGetProperty(property, out var value) ? value : null,
            JsonElement je when property == "length" && je.ValueKind == JsonValueKind.String => je.GetString()!.Length,
            JsonElement je when property == "length" && je.ValueKind == JsonValueKind.Array => je.GetArrayLength(),
            IDictionary<string, object> dictionary => dictionary.TryGetValue(property, out var value) ? value : null,
            string text when property == "length" => text.Length,
            _ => null,
        };
    }

    private static string ProcessBlockTag(Dictionary<string, string> attrs, string inner)
    {
        var template = !string.IsNullOrWhiteSpace(inner) ? inner.Trim() : (attrs.GetValueOrDefault("template") ?? "");

        if (string.IsNullOrEmpty(template))
        {
            return "";
        }

        var configStr = Regex.Replace(
            attrs.GetValueOrDefault("config") ?? "{}",
            @"&(?:amp|quot|apos|lt|gt|#39|#x27|#34|#x22);",
            match => match.Value.ToLowerInvariant() switch
            {
                "&amp;" => "&",
                "&quot;" or "&#34;" or "&#x22;" => "\"",
                "&apos;" or "&#39;" or "&#x27;" => "'",
                "&lt;" => "<",
                "&gt;" => ">",
                _ => match.Value,
            },
            RegexOptions.IgnoreCase);
        Dictionary<string, object>? config;
        try { config = JsonSerializer.Deserialize<Dictionary<string, object>>(configStr); }
        catch
        {
            try { config = JsonSerializer.Deserialize<Dictionary<string, object>>(configStr.Replace("'", "\"")); }
            catch { config = new Dictionary<string, object>(); }
        }
        return RenderTemplate(template, config ?? new Dictionary<string, object>());
    }
}
