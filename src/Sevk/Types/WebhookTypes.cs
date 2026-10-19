using System.Text.Json.Serialization;

namespace Sevk.Types;

public class CreateWebhookRequest
{
    public string Url { get; set; } = "";
    public List<WebhookEventType> Events { get; set; } = new();
    public bool? Enabled { get; set; }
}

public class UpdateWebhookRequest
{
    public string? Url { get; set; }
    public List<WebhookEventType>? Events { get; set; }
    public bool? Enabled { get; set; }
}

public class WebhookEventsResponse
{
    public List<WebhookEventType> Items { get; set; } = new();
    public Dictionary<string, WebhookEvent>? Events { get; set; }
}

public class Webhook
{
    public string Id { get; set; } = "";
    public string Url { get; set; } = "";
    public List<WebhookEventType> Events { get; set; } = new();
    public bool Enabled { get; set; }
    /// <summary>Masked placeholder (••••••••) outside the one-time create response.</summary>
    public string? Secret { get; set; }
    public int FailureCount { get; set; }
    public string? LastFailureAt { get; set; }
    public string? LastDeliveryAt { get; set; }
    public string? DisabledAt { get; set; }
    public string CreatedAt { get; set; } = "";
    public string UpdatedAt { get; set; } = "";

    [JsonPropertyName("projectId")]
    public string ProjectId { get; set; } = "";
}

/// <summary>One-time create response containing a usable whsec_ signing secret.</summary>
public class CreatedWebhook
{
    public string Id { get; set; } = "";
    public string Url { get; set; } = "";
    public List<WebhookEventType> Events { get; set; } = new();
    public bool Enabled { get; set; }
    [JsonRequired]
    public string Secret { get; set; } = "";
    public int FailureCount { get; set; }
    public string? LastFailureAt { get; set; }
    public string? LastDeliveryAt { get; set; }
    public string? DisabledAt { get; set; }
    public string CreatedAt { get; set; } = "";
    public string UpdatedAt { get; set; } = "";

    [JsonPropertyName("projectId")]
    public string ProjectId { get; set; } = "";
}

public class WebhookListResponse : ListResponse<Webhook>
{
    public Dictionary<string, WebhookEvent>? AvailableEvents { get; set; }
}

public class WebhookTestResponse
{
    public bool Success { get; set; }
    public int Status { get; set; }
    public string StatusText { get; set; } = "";
    public string Message { get; set; } = "";
}

public class RotateWebhookResponse
{
    public string Id { get; set; } = "";
    [JsonRequired]
    public string Secret { get; set; } = "";
}

public class WebhookEvent
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public WebhookEventCategory Category { get; set; }
}
