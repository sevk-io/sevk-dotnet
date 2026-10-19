using System.Text.Json.Serialization;

namespace Sevk.Types;

public class FreeEmailsBucket
{
    [JsonPropertyName("current")]
    public int Current { get; set; }

    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    [JsonPropertyName("remaining")]
    public int Remaining { get; set; }
}

public class InboundUsage
{
    [JsonPropertyName("emailsCount")]
    public int EmailsCount { get; set; }

    [JsonPropertyName("monthlyEmailLimit")]
    public int? MonthlyEmailLimit { get; set; }

    [JsonPropertyName("monthlyEmailsUsed")]
    public int MonthlyEmailsUsed { get; set; }

    [JsonPropertyName("monthlyRemaining")]
    public int? MonthlyRemaining { get; set; }

    [JsonPropertyName("freeEmails")]
    public FreeEmailsBucket FreeEmails { get; set; } = new();

    [JsonPropertyName("resetAt")]
    public string? ResetAt { get; set; }

    [JsonPropertyName("pausedAt")]
    public string? PausedAt { get; set; }

    [JsonPropertyName("pausedReason")]
    public string? PausedReason { get; set; }
}

public class OutboundUsage
{
    [JsonPropertyName("monthlyEmailLimit")]
    public int? MonthlyEmailLimit { get; set; }

    [JsonPropertyName("adminMonthlyEmailLimit")]
    public int? AdminMonthlyEmailLimit { get; set; }

    [JsonPropertyName("effectiveMonthlyEmailLimit")]
    public int? EffectiveMonthlyEmailLimit { get; set; }

    [JsonPropertyName("monthlyEmailsUsed")]
    public int MonthlyEmailsUsed { get; set; }

    [JsonPropertyName("limitEmailsUsed")]
    public int LimitEmailsUsed { get; set; }

    [JsonPropertyName("monthlyRemaining")]
    public int? MonthlyRemaining { get; set; }

    [JsonPropertyName("rateLimit")]
    public int? RateLimit { get; set; }

    [JsonPropertyName("freeEmails")]
    public FreeEmailsBucket FreeEmails { get; set; } = new();

    [JsonPropertyName("resetAt")]
    public string? ResetAt { get; set; }

    [JsonPropertyName("pausedAt")]
    public string? PausedAt { get; set; }

    [JsonPropertyName("pausedReason")]
    public string? PausedReason { get; set; }
}

public class ProjectUsage
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Balance { get; set; }

    public string? OutboundEmailPrice { get; set; }

    [JsonPropertyName("inboundEmailPrice")]
    public string? InboundEmailPrice { get; set; }

    [JsonPropertyName("monthlyOutboundEmailLimit")]
    public int? MonthlyOutboundEmailLimit { get; set; }

    [JsonPropertyName("outboundRateLimit")]
    public int? OutboundRateLimit { get; set; }

    [JsonPropertyName("freeMonthlyOutboundEmails")]
    public int? FreeMonthlyOutboundEmails { get; set; }

    [JsonPropertyName("freeMonthlyInboundEmails")]
    public int? FreeMonthlyInboundEmails { get; set; }

    public int? AudienceLimit { get; set; }
    public int? ContactLimit { get; set; }
    public int? BroadcastLimit { get; set; }
    public int? DomainLimit { get; set; }
    public int? WebhookLimit { get; set; }
    public string? StorageLimit { get; set; }
    public string StorageUsed { get; set; } = "0";
    public int AudiencesCount { get; set; }
    public int ContactsCount { get; set; }
    public int BroadcastsCount { get; set; }

    [JsonPropertyName("monthlyBroadcastsUsed")]
    public int MonthlyBroadcastsUsed { get; set; }

    public int DomainsCount { get; set; }
    public int WebhooksCount { get; set; }

    [JsonPropertyName("inboundEmailsCount")]
    public int InboundEmailsCount { get; set; }

    [JsonPropertyName("inbound")]
    public InboundUsage Inbound { get; set; } = new();

    [JsonPropertyName("outbound")]
    public OutboundUsage Outbound { get; set; } = new();
}
