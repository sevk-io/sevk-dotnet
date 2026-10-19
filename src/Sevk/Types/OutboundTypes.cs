using System.Text.Json.Serialization;

namespace Sevk.Types;

public class OutboundQuota
{
    [JsonPropertyName("projectId")]
    public string ProjectId { get; set; } = "";

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

    [JsonPropertyName("freeMonthlyEmails")]
    public int? FreeMonthlyEmails { get; set; }

    [JsonPropertyName("freeEmailsUsed")]
    public int FreeEmailsUsed { get; set; }

    [JsonPropertyName("rateLimit")]
    public int? RateLimit { get; set; }

    [JsonPropertyName("emailPrice")]
    public double EmailPrice { get; set; }

    [JsonPropertyName("resetAt")]
    public string ResetAt { get; set; } = "";

    [JsonPropertyName("pausedAt")]
    public string? PausedAt { get; set; }

    [JsonPropertyName("pausedReason")]
    public string? PausedReason { get; set; }

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = "";

    [JsonPropertyName("updatedAt")]
    public string UpdatedAt { get; set; } = "";
}

public class UpdateOutboundQuotaRequest
{
    [JsonPropertyName("monthlyEmailLimit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int? MonthlyEmailLimit { get; set; }
}
