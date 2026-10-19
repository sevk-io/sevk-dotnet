using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sevk.Types;

[JsonConverter(typeof(OptionalJsonConverterFactory))]
public readonly struct Optional<T> : IEquatable<Optional<T>>
{
    /// <summary>Indicates whether the caller explicitly supplied this value.</summary>
    public bool IsSet { get; }
    /// <summary>Gets the supplied value, including an explicitly supplied null.</summary>
    public T? Value { get; }

    /// <summary>Marks a value as explicitly supplied for request serialization.</summary>
    public Optional(T? value)
    {
        IsSet = true;
        Value = value;
    }

    /// <summary>Wraps an explicitly supplied value.</summary>
    public static implicit operator Optional<T>(T? value) => new(value);

    /// <summary>Compares both presence and the wrapped value.</summary>
    public bool Equals(Optional<T> other) =>
        IsSet == other.IsSet && EqualityComparer<T?>.Default.Equals(Value, other.Value);

    public override bool Equals(object? obj) => obj is Optional<T> other && Equals(other);

    public override int GetHashCode() => IsSet ? Value?.GetHashCode() ?? 1 : 0;
}

internal sealed class OptionalJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(OptionalJsonConverter<>).MakeGenericType(typeToConvert.GetGenericArguments()[0]);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

internal sealed class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
{
    public override bool HandleNull => true;

    public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(JsonSerializer.Deserialize<T>(ref reader, options));

    public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
    {
        if (value.Value is null)
        {
            writer.WriteNullValue();

            return;
        }
        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
