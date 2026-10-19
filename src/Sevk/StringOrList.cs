using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sevk.Types;

[JsonConverter(typeof(StringOrListJsonConverter))]
public sealed class StringOrList
{
    public string? Single { get; }
    public IReadOnlyList<string>? Multiple { get; }

    private StringOrList(string single)
    {
        Single = single ?? throw new ArgumentNullException(nameof(single));
    }

    private StringOrList(IReadOnlyList<string> multiple)
    {
        Multiple = multiple ?? throw new ArgumentNullException(nameof(multiple));
    }

    public static implicit operator StringOrList(string value) => new(value);
    public static implicit operator StringOrList(string[] values) => new(values);
    public static implicit operator StringOrList(List<string> values) => new(values);
}

internal sealed class StringOrListJsonConverter : JsonConverter<StringOrList>
{
    public override StringOrList Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return reader.GetString()!;

        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected a string or string array");

        var values = new List<string>();

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("Expected a string array");
            values.Add(reader.GetString()!);
        }
        if (reader.TokenType != JsonTokenType.EndArray)
            throw new JsonException("Expected a string array");

        return values;
    }

    public override void Write(Utf8JsonWriter writer, StringOrList value, JsonSerializerOptions options)
    {
        if (value.Single is not null)
        {
            writer.WriteStringValue(value.Single);

            return;
        }
        JsonSerializer.Serialize(writer, value.Multiple, options);
    }
}
