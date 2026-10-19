using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sevk;

internal sealed class JsonDataDictionaryConverter : JsonConverter<Dictionary<string, object>>
{
    private readonly JsonSerializerOptions _options;

    internal JsonDataDictionaryConverter(JsonSerializerOptions options)
    {
        _options = new JsonSerializerOptions(options)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };
    }

    public override Dictionary<string, object>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, _options);

    public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, _options);
}
