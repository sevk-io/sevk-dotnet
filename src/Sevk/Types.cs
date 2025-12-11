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
    public string Name { get; set; } = "";
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

public class Email
{
    public string Id { get; set; } = "";
    public string? MessageId { get; set; }
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
    public List<Domain> Domains { get; set; } = new();
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

public class SendEmailRequest
{
    public string To { get; set; } = "";
    public string From { get; set; } = "";
    public string Subject { get; set; } = "";
    public string? Html { get; set; }
    public string? Text { get; set; }
    public string? ReplyTo { get; set; }
}

public class ListParams
{
    public int? Page { get; set; }
    public int? Limit { get; set; }
    public string? Search { get; set; }
}
