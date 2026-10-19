using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sevk.Types;

public class InboundAttachment
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("filename")]
    public string Filename { get; set; } = "";

    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = "";

    [JsonPropertyName("contentId")]
    public string? ContentId { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = "";
}

public class InboundEmailReplyToEmail
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("messageId")]
    public string? MessageId { get; set; }

    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    [JsonPropertyName("to")]
    public string? To { get; set; }

    [JsonPropertyName("from")]
    public string? From { get; set; }

    [JsonPropertyName("status")]
    public EmailStatus? Status { get; set; }

    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("broadcastId")]
    public string? BroadcastId { get; set; }

    [JsonPropertyName("contactId")]
    public string? ContactId { get; set; }
}

public class InboundEmailReplyToBroadcast
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    [JsonPropertyName("status")]
    public BroadcastLifecycleStatus? Status { get; set; }

    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }
}

public class InboundReplyEmailSummary
{
    public string Id { get; set; } = "";
    public string? Subject { get; set; }
    public string? To { get; set; }
}

public class InboundReplyBroadcastSummary
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
}

public class InboundEmailSummary
{
    public string Id { get; set; } = "";
    public string? MessageId { get; set; }
    public string FromAddress { get; set; } = "";
    public string ToAddress { get; set; } = "";
    public string? Subject { get; set; }
    public InboundEmailStatus Status { get; set; }
    public bool IsSpam { get; set; }
    public bool IsVirus { get; set; }
    public double? SpamScore { get; set; }
    public long RawSize { get; set; }
    public string? DomainId { get; set; }
    public string? ReplyToEmailId { get; set; }
    public string? ReplyToBroadcastId { get; set; }
    public string ReceivedAt { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public InboundReplyEmailSummary? ReplyToEmail { get; set; }
    public InboundReplyBroadcastSummary? ReplyToBroadcast { get; set; }
}

public class InboundEmail
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("messageId")]
    public string? MessageId { get; set; }

    [JsonPropertyName("fromAddress")]
    public string FromAddress { get; set; } = "";

    [JsonPropertyName("toAddress")]
    public string ToAddress { get; set; } = "";

    [JsonPropertyName("ccAddresses")]
    public List<string>? CcAddresses { get; set; }

    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    [JsonPropertyName("status")]
    public InboundEmailStatus Status { get; set; }

    [JsonPropertyName("isSpam")]
    public bool IsSpam { get; set; }

    [JsonPropertyName("isVirus")]
    public bool IsVirus { get; set; }

    [JsonPropertyName("spamScore")]
    public double? SpamScore { get; set; }

    [JsonPropertyName("rawSize")]
    public long RawSize { get; set; }

    [JsonPropertyName("domainId")]
    public string? DomainId { get; set; }

    [JsonPropertyName("receivedAt")]
    public string ReceivedAt { get; set; } = "";

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = "";

    [JsonPropertyName("inReplyTo")]
    public string? InReplyTo { get; set; }

    [JsonPropertyName("references")]
    public List<string>? References { get; set; }

    [JsonPropertyName("bodyText")]
    public string? BodyText { get; set; }

    [JsonPropertyName("bodyHtml")]
    public string? BodyHtml { get; set; }

    [JsonPropertyName("headers")]
    public JsonElement? Headers { get; set; }

    [JsonPropertyName("projectId")]
    public string? ProjectId { get; set; }

    [JsonPropertyName("processedAt")]
    public string? ProcessedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public string? UpdatedAt { get; set; }

    [JsonPropertyName("attachments")]
    public List<InboundAttachment>? Attachments { get; set; }

    [JsonPropertyName("replyToEmailId")]
    public string? ReplyToEmailId { get; set; }

    [JsonPropertyName("replyToBroadcastId")]
    public string? ReplyToBroadcastId { get; set; }

    [JsonPropertyName("replyToEmail")]
    public InboundEmailReplyToEmail? ReplyToEmail { get; set; }

    [JsonPropertyName("replyToBroadcast")]
    public InboundEmailReplyToBroadcast? ReplyToBroadcast { get; set; }
}

public class InboundStats
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("counts")]
    [JsonRequired]
    public InboundStatsCounts Counts { get; set; } = new();

    [JsonPropertyName("received")]
    public int Received { get; set; }

    [JsonPropertyName("processed")]
    public int Processed { get; set; }

    [JsonPropertyName("bounced")]
    public int Bounced { get; set; }

    [JsonPropertyName("blocked")]
    public int Blocked { get; set; }

    [JsonPropertyName("blocklistRejected")]
    [JsonRequired]
    public int BlocklistRejected { get; set; }

    [JsonPropertyName("spam")]
    [JsonRequired]
    public int Spam { get; set; }

    [JsonPropertyName("failed")]
    public int Failed { get; set; }
}

public class InboundStatsCounts
{
    [JsonPropertyName("RECEIVED")]
    [JsonRequired]
    public int Received { get; set; }
    [JsonPropertyName("PROCESSED")]
    [JsonRequired]
    public int Processed { get; set; }
    [JsonPropertyName("BOUNCED")]
    [JsonRequired]
    public int Bounced { get; set; }
    [JsonPropertyName("BLOCKED")]
    [JsonRequired]
    public int Blocked { get; set; }
    [JsonPropertyName("FAILED")]
    [JsonRequired]
    public int Failed { get; set; }
}

public class InboundRoute
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("projectId")]
    public string ProjectId { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("pattern")]
    public string Pattern { get; set; } = "";

    [JsonPropertyName("target")]
    public string Target { get; set; } = "";

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = "";

    [JsonPropertyName("updatedAt")]
    public string UpdatedAt { get; set; } = "";
}

public class InboundBlocklistEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("projectId")]
    public string ProjectId { get; set; } = "";

    [JsonPropertyName("type")]
    public InboundBlocklistType Type { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; } = "";

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = "";
}

public class InboundQuota
{
    [JsonPropertyName("projectId")]
    public string ProjectId { get; set; } = "";

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = "";

    [JsonPropertyName("updatedAt")]
    public string UpdatedAt { get; set; } = "";

    [JsonPropertyName("monthlyEmailLimit")]
    public int? MonthlyEmailLimit { get; set; }

    [JsonPropertyName("monthlyEmailsUsed")]
    public int MonthlyEmailsUsed { get; set; }

    /// <summary>Always present; null means no free inbound tier.</summary>
    [JsonPropertyName("freeMonthlyInboundEmails")]
    public int? FreeMonthlyInboundEmails { get; set; }

    /// <summary>Free inbound emails consumed this month.</summary>
    [JsonPropertyName("freeEmailsUsed")]
    public int FreeEmailsUsed { get; set; }

    [JsonPropertyName("inboundEmailPrice")]
    public double InboundEmailPrice { get; set; }

    [JsonPropertyName("resetAt")]
    public string ResetAt { get; set; } = "";

    [JsonPropertyName("pausedAt")]
    public string? PausedAt { get; set; }

    [JsonPropertyName("pausedReason")]
    public string? PausedReason { get; set; }
}

public class InboundSecurityConfig
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("domainId")]
    public string DomainId { get; set; } = "";

    [JsonPropertyName("dmarcStrict")]
    public bool DmarcStrict { get; set; }

    [JsonPropertyName("spfRequired")]
    public bool SpfRequired { get; set; }

    [JsonPropertyName("spamThreshold")]
    [JsonRequired]
    public double? SpamThreshold { get; set; }

    [JsonPropertyName("catchAllEnabled")]
    public bool CatchAllEnabled { get; set; }

    [JsonPropertyName("acceptedRecipients")]
    public List<string> AcceptedRecipients { get; set; } = new();

    [JsonPropertyName("maxAttachmentSize")]
    public long MaxAttachmentSize { get; set; }

    [JsonPropertyName("allowedMimeTypes")]
    public List<string> AllowedMimeTypes { get; set; } = new();

    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public string? UpdatedAt { get; set; }

    [JsonPropertyName("platform")]
    [JsonRequired]
    public InboundSecurityPlatform Platform { get; set; } = new();
}

public class InboundSecurityPlatform
{
    [JsonPropertyName("maxAttachmentSizeCap")]
    public long MaxAttachmentSizeCap { get; set; }
}

public class InboundListResponse
{
    [JsonPropertyName("items")]
    public List<InboundEmailSummary> Items { get; set; } = new();

    [JsonPropertyName("total")]
    public int? Total { get; set; }

    [JsonPropertyName("page")]
    public int? Page { get; set; }

    [JsonPropertyName("totalPages")]
    public int? TotalPages { get; set; }

    [JsonPropertyName("hasMore")]
    public bool? HasMore { get; set; }

    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; set; }

    [JsonPropertyName("prevCursor")]
    public string? PrevCursor { get; set; }
}

public class InboundAttachmentDownload
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = "";

    [JsonPropertyName("filename")]
    public string Filename { get; set; } = "";

    [JsonPropertyName("mimeType")]
    public string MimeType { get; set; } = "";
}

public class ListInboundParams : ListParams
{
    public InboundEmailStatus? Status { get; set; }
    public string? DomainId { get; set; }
    public string? BroadcastId { get; set; }
    public bool? IsSpam { get; set; }
    public bool? IsVirus { get; set; }
}

public class CreateInboundRouteRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("pattern")]
    public string Pattern { get; set; } = "";

    [JsonPropertyName("target")]
    public string Target { get; set; } = "";

    [JsonPropertyName("priority")]
    public int? Priority { get; set; }

    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }
}

public class UpdateInboundRouteRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("pattern")]
    public string? Pattern { get; set; }

    [JsonPropertyName("target")]
    public string? Target { get; set; }

    [JsonPropertyName("priority")]
    public int? Priority { get; set; }

    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }
}

public class AddInboundBlocklistRequest
{
    [JsonPropertyName("type")]
    public InboundBlocklistType Type { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; } = "";

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

public class UpdateInboundQuotaRequest
{
    [JsonPropertyName("monthlyEmailLimit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<int?> MonthlyEmailLimit { get; set; }
}

public class UpdateInboundSecurityRequest
{
    [JsonPropertyName("dmarcStrict")]
    public bool? DmarcStrict { get; set; }

    [JsonPropertyName("spfRequired")]
    public bool? SpfRequired { get; set; }

    [JsonPropertyName("spamThreshold")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<double?> SpamThreshold { get; set; }

    [JsonPropertyName("catchAllEnabled")]
    public bool? CatchAllEnabled { get; set; }

    [JsonPropertyName("acceptedRecipients")]
    public List<string>? AcceptedRecipients { get; set; }

    [JsonPropertyName("maxAttachmentSize")]
    public long? MaxAttachmentSize { get; set; }

    [JsonPropertyName("allowedMimeTypes")]
    public List<string>? AllowedMimeTypes { get; set; }
}

public class InboundRoutesListResponse
{
    [JsonPropertyName("items")]
    public List<InboundRoute> Items { get; set; } = new();

    [JsonPropertyName("total")]
    public int? Total { get; set; }

    [JsonPropertyName("page")]
    public int? Page { get; set; }

    [JsonPropertyName("totalPages")]
    public int? TotalPages { get; set; }

    [JsonPropertyName("hasMore")]
    public bool? HasMore { get; set; }

    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; set; }

    [JsonPropertyName("prevCursor")]
    public string? PrevCursor { get; set; }
}

public class InboundBlocklistListResponse
{
    [JsonPropertyName("items")]
    public List<InboundBlocklistEntry> Items { get; set; } = new();

    [JsonPropertyName("total")]
    public int? Total { get; set; }

    [JsonPropertyName("page")]
    public int? Page { get; set; }

    [JsonPropertyName("totalPages")]
    public int? TotalPages { get; set; }

    [JsonPropertyName("hasMore")]
    public bool? HasMore { get; set; }

    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; set; }

    [JsonPropertyName("prevCursor")]
    public string? PrevCursor { get; set; }
}
