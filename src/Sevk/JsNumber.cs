using System.Globalization;

namespace Sevk.Markup;

internal static class JsNumber
{
    public static string Format(double value)
    {
        if (value == 0)
        {
            return "0";
        }
        if (double.IsNaN(value))
        {
            return "NaN";
        }
        if (double.IsInfinity(value))
        {
            return value > 0 ? "Infinity" : "-Infinity";
        }

        var parts = Math.Abs(value).ToString("R", CultureInfo.InvariantCulture).Split('E');
        var mantissa = parts[0].Split('.');
        var fraction = mantissa.Length > 1 ? mantissa[1].TrimEnd('0') : "";
        var digits = mantissa[0] + fraction;
        var trimmed = digits.TrimStart('0');
        var exponent = parts.Length > 1 ? int.Parse(parts[1], CultureInfo.InvariantCulture) : 0;
        var point = mantissa[0].Length + exponent - (digits.Length - trimmed.Length);
        var count = trimmed.Length;

        string text;
        if (count <= point && point <= 21)
        {
            text = trimmed + new string('0', point - count);
        }
        else if (point > 0 && point <= 21)
        {
            text = trimmed.Substring(0, point) + "." + trimmed.Substring(point);
        }
        else if (point > -6 && point <= 0)
        {
            text = "0." + new string('0', -point) + trimmed;
        }
        else
        {
            var power = point - 1;
            var suffix = "e" + (power < 0 ? "-" : "+") + Math.Abs(power).ToString(CultureInfo.InvariantCulture);
            text = count == 1 ? trimmed + suffix : trimmed.Substring(0, 1) + "." + trimmed.Substring(1) + suffix;
        }

        return value < 0 ? "-" + text : text;
    }
}
