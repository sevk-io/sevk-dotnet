using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sevk.Types;

public class Contact
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public bool Subscribed { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Audience
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? UsersCanSee { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Template
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Broadcast
{
    public string Id { get; set; } = "";
    public string? Subject { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Domain
{
    public string Id { get; set; } = "";
    [JsonPropertyName("domain")]
    public string DomainName { get; set; } = "";
    public string? Email { get; set; }
    public string? From { get; set; }
    public string? SenderName { get; set; }
    public string? Region { get; set; }
    public bool Verified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Topic
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string AudienceId { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Segment
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string AudienceId { get; set; } = "";
    public List<SegmentRule>? Rules { get; set; }
    public string Operator { get; set; } = "AND";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SegmentRule
{
    public string Field { get; set; } = "";
    public string Operator { get; set; } = "";
    public string Value { get; set; } = "";
}

public class SegmentCalculateResponse
{
    public int Count { get; set; }
    public List<Contact>? Contacts { get; set; }
}

public class EmailAttachment
{
    public string Filename { get; set; } = "";
    public string Content { get; set; } = "";      // Base64 encoded
    public string ContentType { get; set; } = "";  // MIME type
}

public class Email
{
    public string? Id { get; set; }
    public List<string>? Ids { get; set; }
    public string? MessageId { get; set; }
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

// List responses
public class ListResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
}

public class DomainsResponse
{
    public List<Domain> Items { get; set; } = new();
}

public class DnsRecord
{
    public string Type { get; set; } = "";
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
    public int? Priority { get; set; }
    public string? Status { get; set; }
}

public class DnsRecordsResponse
{
    public List<DnsRecord> Items { get; set; } = new();
}

public class SubscribeResponse
{
    public Contact Contact { get; set; } = new();
}

// Request types
public class CreateContactRequest
{
    public string Email { get; set; } = "";
    public bool? Subscribed { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class UpdateContactRequest
{
    public bool? Subscribed { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class CreateAudienceRequest
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? UsersCanSee { get; set; }
}

public class UpdateAudienceRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? UsersCanSee { get; set; }
}

public class CreateTemplateRequest
{
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
}

public class UpdateTemplateRequest
{
    public string? Title { get; set; }
    public string? Content { get; set; }
}

public class CreateTopicRequest
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}

public class UpdateTopicRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class CreateSegmentRequest
{
    public string Name { get; set; } = "";
    public List<SegmentRule> Rules { get; set; } = new();
    public string Operator { get; set; } = "AND";
}

public class UpdateSegmentRequest
{
    public string? Name { get; set; }
    public List<SegmentRule>? Rules { get; set; }
    public string? Operator { get; set; }
}

public class SubscribeRequest
{
    public string Email { get; set; } = "";
    public string AudienceId { get; set; } = "";
    public List<string>? TopicIds { get; set; }
}

public class UnsubscribeRequest
{
    public string Email { get; set; } = "";
    public string? AudienceId { get; set; }
}

public class CreateBroadcastRequest
{
    public string Name { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public string? Style { get; set; }
    public string? TargetType { get; set; }
    public string? AudienceId { get; set; }
    public string? TopicId { get; set; }
    public string? SegmentId { get; set; }
    public string SenderName { get; set; } = "";
    public string? SenderEmail { get; set; }
    public string DomainId { get; set; } = "";
    public string? ScheduledAt { get; set; }
}

public class UpdateBroadcastRequest
{
    public string? Name { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? Style { get; set; }
    public string? TargetType { get; set; }
    public string? AudienceId { get; set; }
    public string? TopicId { get; set; }
    public string? SegmentId { get; set; }
    public string? SenderName { get; set; }
    public string? DomainId { get; set; }
    public string? ScheduledAt { get; set; }
}

public class CreateDomainRequest
{
    [JsonPropertyName("domain")]
    public string Domain { get; set; } = "";
    public string? Email { get; set; }
    public string? From { get; set; }
    public string? SenderName { get; set; }
    public string? Region { get; set; }
}

public class UpdateDomainRequest
{
    public string? Name { get; set; }
    public string? Region { get; set; }
}

public class CreateWebhookRequest
{
    public string Url { get; set; } = "";
    public List<string> Events { get; set; } = new();
    public bool? Enabled { get; set; }
}

public class UpdateWebhookRequest
{
    public string? Url { get; set; }
    public List<string>? Events { get; set; }
    public bool? Enabled { get; set; }
}

public class SendEmailRequest
{
    public string To { get; set; } = "";
    public string From { get; set; } = "";
    public string Subject { get; set; } = "";
    public string? Html { get; set; }
    public string? Text { get; set; }
    public string? ReplyTo { get; set; }
    public List<EmailAttachment>? Attachments { get; set; }
}

public class BulkEmailRequest
{
    public List<SendEmailRequest> Emails { get; set; } = new();
}

public class BulkUpdateContactEntry
{
    public string Id { get; set; } = "";
    public string? Email { get; set; }
    public bool? Subscribed { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class BulkUpdateContactsResponse
{
    public int Updated { get; set; }
}

public class ImportContactEntry
{
    public string Email { get; set; } = "";
    public bool? Subscribed { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

public class ImportContactsRequest
{
    public List<ImportContactEntry> Contacts { get; set; } = new();
    public string? AudienceId { get; set; }
}

public class ImportContactsResponse
{
    public int Imported { get; set; }
    public int Failed { get; set; }
    public List<ImportContactError>? Errors { get; set; }
}

public class ImportContactError
{
    public int Row { get; set; }
    public string Error { get; set; } = "";
}

public class ContactEvent
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public string? Action { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Region
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Label { get; set; }
}

public class RegionsResponse
{
    public List<string> Items { get; set; } = new();
}

public class WebhookEventsResponse
{
    public List<string> Items { get; set; } = new();
    public Dictionary<string, WebhookEvent>? Events { get; set; }
}

public class ListParams
{
    public int? Page { get; set; }
    public int? Limit { get; set; }
    public string? Search { get; set; }
}

public class BroadcastStatus
{
    public string Id { get; set; } = "";
    public string Status { get; set; } = "";
    public int Total { get; set; }
    public int Sent { get; set; }
    public int Delivered { get; set; }
    public int Failed { get; set; }
}

public class BroadcastEmail
{
    public string Id { get; set; } = "";
    public string To { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class BroadcastCostEstimate
{
    public int Recipients { get; set; }
    public double EstimatedCost { get; set; }
    public string? Currency { get; set; }
}

public class Webhook
{
    public string Id { get; set; } = "";
    public string Url { get; set; } = "";
    public List<string> Events { get; set; } = new();
    public bool Enabled { get; set; }
    public string? Secret { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class WebhookTestResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class WebhookEvent
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? Category { get; set; }
}

public class Event
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public string? Action { get; set; }
    public string? Description { get; set; }
    public JsonElement? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EventStats
{
    public int Total { get; set; }
    public int Sent { get; set; }
    public int Delivered { get; set; }
    public int Opened { get; set; }
    public int Clicked { get; set; }
    public int Bounced { get; set; }
    public int Complained { get; set; }
}
