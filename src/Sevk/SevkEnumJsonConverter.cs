using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sevk.Types;

internal sealed class SevkEnumJsonConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
{
    private static readonly Dictionary<TEnum, string> Names = typeof(TEnum)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .ToDictionary(
            field => (TEnum)field.GetValue(null)!,
            field => field.GetCustomAttribute<EnumMemberAttribute>()?.Value ?? field.Name);

    private static readonly Dictionary<string, TEnum> Values = Names
        .ToDictionary(pair => pair.Value, pair => pair.Key, StringComparer.OrdinalIgnoreCase);

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var text = reader.GetString()!;
            if (Values.TryGetValue(text, out var value) || Enum.TryParse(text, true, out value))
                return value;
        }
        else if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var number))
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), number);
        }

        throw new JsonException($"Invalid {typeof(TEnum).Name} value");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        if (Names.TryGetValue(value, out var name))
            writer.WriteStringValue(name);
        else
            writer.WriteNumberValue(Convert.ToInt32(value));
    }
}
