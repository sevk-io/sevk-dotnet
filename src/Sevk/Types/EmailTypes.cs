using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sevk.Types;

public class EmailAttachment
{
    public string Filename { get; set; } = "";
    public string Content { get; set; } = "";      // Base64 encoded
    public string ContentType { get; set; } = "";  // MIME type
    public string? Cid { get; set; }
}

public class EmailTracking
{
    public bool? Open { get; set; }
    public bool? Click { get; set; }
}

public class Email
{
    public string Id { get; set; } = "";
    public string? MessageId { get; set; }
    public string? ProviderMessageId { get; set; }
    public string? From { get; set; }
    public string? To { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? Text { get; set; }
    public EmailStatus Status { get; set; }
    public EmailType Type { get; set; }
    public string? ContactId { get; set; }
    public string? BroadcastId { get; set; }
    public string? BroadcastVariantId { get; set; }
    public string ProjectId { get; set; } = "";
    public string? OpenedAt { get; set; }
    public string? ClickedAt { get; set; }
    public string? UnsubscribedAt { get; set; }
    public string? ScheduledAt { get; set; }
    public string? CancelledAt { get; set; }
    public string CreatedAt { get; set; } = "";
    public string UpdatedAt { get; set; } = "";

    [JsonPropertyName("userAgent")]
    public string? UserAgent { get; set; }
}

[JsonConverter(typeof(SendEmailResponseJsonConverter))]
public abstract record SendEmailResponse;

public sealed record SingleEmailResponse(string Id) : SendEmailResponse;

public sealed record MultipleEmailResponse(IReadOnlyList<string> Ids) : SendEmailResponse;

internal sealed class SendEmailResponseJsonConverter : JsonConverter<SendEmailResponse>
{
    public override SendEmailResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
            throw new JsonException("Expected an object");

        var hasId = root.TryGetProperty("id", out var id);
        var hasIds = root.TryGetProperty("ids", out var ids);

        if (hasId == hasIds)
            throw new JsonException("Expected exactly one of id or ids");

        if (hasId)
        {
            if (id.ValueKind != JsonValueKind.String)
                throw new JsonException("Expected id to be a string");

            return new SingleEmailResponse(id.GetString()!);
        }

        if (ids.ValueKind != JsonValueKind.Array)
            throw new JsonException("Expected ids to be a string array");
        var values = new List<string>();

        foreach (var item in ids.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
                throw new JsonException("Expected ids to be a string array");
            values.Add(item.GetString()!);
        }

        return new MultipleEmailResponse(values);
    }

    public override void Write(Utf8JsonWriter writer, SendEmailResponse value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        switch (value)
        {
            case SingleEmailResponse single:
                writer.WriteString("id", single.Id);
                break;
            case MultipleEmailResponse multiple:
                writer.WritePropertyName("ids");
                JsonSerializer.Serialize(writer, multiple.Ids, options);
                break;
            default:
                throw new JsonException("Unknown send email response");
        }
        writer.WriteEndObject();
    }
}

public class BulkEmailError
{
    public int Index { get; set; }
    public string Email { get; set; } = "";
    public string Error { get; set; } = "";
}

public class BulkEmailResponse
{
    public int Success { get; set; }
    public int Failed { get; set; }
    public List<string> Ids { get; set; } = new();
    public List<BulkEmailError>? Errors { get; set; }
}

public class SendEmailRequest
{
    public string From { get; set; } = "";

    public StringOrList To { get; set; } = "";
    public string Subject { get; set; } = "";
    public string? Html { get; set; }
    public string? Text { get; set; }

    /// <summary>sevk Markup body. SDK renders to HTML locally before sending.</summary>
    [JsonIgnore]
    public string? Markup { get; set; }

    [JsonPropertyName("reply_to")]
    public StringOrList? ReplyTo { get; set; }

    public StringOrList? Cc { get; set; }

    public StringOrList? Bcc { get; set; }

    public List<EmailAttachment>? Attachments { get; set; }

    public EmailTracking? Tracking { get; set; }

    public Dictionary<string, string>? Headers { get; set; }

    public string? ScheduledAt { get; set; }
}

public class UpdateScheduledEmailRequest
{
    public string ScheduledAt { get; set; } = "";
}

public class ScheduledEmailMutationResponse
{
    public string Id { get; set; } = "";
}

public class BulkEmailRequest
{
    public List<BulkEmailItemRequest> Emails { get; set; } = new();
}

public class BulkEmailItemRequest
{
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public string Subject { get; set; } = "";
    public string? Html { get; set; }
    public string? Text { get; set; }

    [JsonIgnore]
    public string? Markup { get; set; }

    [JsonPropertyName("reply_to")]
    public StringOrList? ReplyTo { get; set; }

    public StringOrList? Cc { get; set; }
    public StringOrList? Bcc { get; set; }
    public List<EmailAttachment>? Attachments { get; set; }
    public EmailTracking? Tracking { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
}
