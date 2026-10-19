using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Sevk.Types;

[JsonConverter(typeof(SevkEnumJsonConverter<AudienceVisibility>))]
public enum AudienceVisibility
{
    [EnumMember(Value = "PUBLIC")] Public,
    [EnumMember(Value = "PRIVATE")] Private
}

[JsonConverter(typeof(SevkEnumJsonConverter<ContentStyle>))]
public enum ContentStyle
{
    [EnumMember(Value = "HTML")] Html,
    [EnumMember(Value = "MARKUP")] Markup
}

[JsonConverter(typeof(SevkEnumJsonConverter<BroadcastLifecycleStatus>))]
public enum BroadcastLifecycleStatus
{
    [EnumMember(Value = "DRAFT")] Draft,
    [EnumMember(Value = "SCHEDULED")] Scheduled,
    [EnumMember(Value = "STARTING")] Starting,
    [EnumMember(Value = "SENDING")] Sending,
    [EnumMember(Value = "SENT")] Sent,
    [EnumMember(Value = "FAILED")] Failed,
    [EnumMember(Value = "CANCELLING")] Cancelling,
    [EnumMember(Value = "CANCELLED")] Cancelled
}

[JsonConverter(typeof(SevkEnumJsonConverter<BroadcastTargetType>))]
public enum BroadcastTargetType
{
    [EnumMember(Value = "ALL")] All,
    [EnumMember(Value = "AUDIENCE")] Audience,
    [EnumMember(Value = "TOPIC")] Topic,
    [EnumMember(Value = "SEGMENT")] Segment
}

[JsonConverter(typeof(SevkEnumJsonConverter<AbTestMetric>))]
public enum AbTestMetric
{
    [EnumMember(Value = "OPEN_RATE")] OpenRate,
    [EnumMember(Value = "CLICK_RATE")] ClickRate
}

[JsonConverter(typeof(SevkEnumJsonConverter<AbTestStatus>))]
public enum AbTestStatus
{
    [EnumMember(Value = "SCHEDULED")] Scheduled,
    [EnumMember(Value = "TESTING")] Testing,
    [EnumMember(Value = "EVALUATING")] Evaluating,
    [EnumMember(Value = "ROLLING_OUT")] RollingOut,
    [EnumMember(Value = "COMPLETED")] Completed,
    [EnumMember(Value = "FAILED")] Failed
}

[JsonConverter(typeof(SevkEnumJsonConverter<LogicalOperator>))]
public enum LogicalOperator
{
    [EnumMember(Value = "AND")] And,
    [EnumMember(Value = "OR")] Or
}

[JsonConverter(typeof(SevkEnumJsonConverter<SegmentRuleOperator>))]
public enum SegmentRuleOperator
{
    [EnumMember(Value = "equals")] Equals,
    [EnumMember(Value = "notEquals")] NotEquals,
    [EnumMember(Value = "contains")] Contains,
    [EnumMember(Value = "notContains")] NotContains,
    [EnumMember(Value = "startsWith")] StartsWith,
    [EnumMember(Value = "endsWith")] EndsWith,
    [EnumMember(Value = "greaterThan")] GreaterThan,
    [EnumMember(Value = "lessThan")] LessThan,
    [EnumMember(Value = "greaterOrEqual")] GreaterOrEqual,
    [EnumMember(Value = "lessOrEqual")] LessOrEqual,
    [EnumMember(Value = "isEmpty")] IsEmpty,
    [EnumMember(Value = "isNotEmpty")] IsNotEmpty,
    [EnumMember(Value = "in")] In,
    [EnumMember(Value = "notIn")] NotIn
}

[JsonConverter(typeof(SevkEnumJsonConverter<TlsPolicy>))]
public enum TlsPolicy
{
    [EnumMember(Value = "OPTIONAL")] Optional,
    [EnumMember(Value = "REQUIRE")] Require
}

[JsonConverter(typeof(SevkEnumJsonConverter<DnsRecordType>))]
public enum DnsRecordType
{
    [EnumMember(Value = "CNAME")] Cname,
    [EnumMember(Value = "TXT")] Txt,
    [EnumMember(Value = "MX")] Mx
}

[JsonConverter(typeof(SevkEnumJsonConverter<EmailStatus>))]
public enum EmailStatus
{
    [EnumMember(Value = "PENDING")] Pending,
    [EnumMember(Value = "SCHEDULED")] Scheduled,
    [EnumMember(Value = "SENT")] Sent,
    [EnumMember(Value = "DELIVERED")] Delivered,
    [EnumMember(Value = "BOUNCED")] Bounced,
    [EnumMember(Value = "OPENED")] Opened,
    [EnumMember(Value = "COMPLAINT")] Complaint,
    [EnumMember(Value = "FAILED")] Failed,
    [EnumMember(Value = "SUPPRESSED")] Suppressed,
    [EnumMember(Value = "CANCELLED")] Cancelled
}

[JsonConverter(typeof(SevkEnumJsonConverter<EmailType>))]
public enum EmailType
{
    [EnumMember(Value = "MARKETING")] Marketing,
    [EnumMember(Value = "TRANSACTIONAL")] Transactional
}

[JsonConverter(typeof(SevkEnumJsonConverter<InboundEmailStatus>))]
public enum InboundEmailStatus
{
    [EnumMember(Value = "RECEIVED")] Received,
    [EnumMember(Value = "PROCESSED")] Processed,
    [EnumMember(Value = "BOUNCED")] Bounced,
    [EnumMember(Value = "BLOCKED")] Blocked,
    [EnumMember(Value = "FAILED")] Failed
}

[JsonConverter(typeof(SevkEnumJsonConverter<InboundBlocklistType>))]
public enum InboundBlocklistType
{
    [EnumMember(Value = "EMAIL")] Email,
    [EnumMember(Value = "DOMAIN")] Domain,
    [EnumMember(Value = "IP")] Ip
}

[JsonConverter(typeof(SevkEnumJsonConverter<ContactUnsubscribeActor>))]
public enum ContactUnsubscribeActor
{
    [EnumMember(Value = "USER")] User,
    [EnumMember(Value = "API")] Api,
    [EnumMember(Value = "ADMIN")] Admin,
    [EnumMember(Value = "BOUNCE")] Bounce,
    [EnumMember(Value = "COMPLAINT")] Complaint
}

[JsonConverter(typeof(SevkEnumJsonConverter<ActivityAction>))]
public enum ActivityAction
{
    [EnumMember(Value = "REQUESTED")] Requested,
    [EnumMember(Value = "SENT")] Sent,
    [EnumMember(Value = "DELIVERED")] Delivered,
    [EnumMember(Value = "OPENED")] Opened,
    [EnumMember(Value = "CLICKED")] Clicked,
    [EnumMember(Value = "BOUNCED")] Bounced,
    [EnumMember(Value = "COMPLAINT")] Complaint,
    [EnumMember(Value = "FAILED")] Failed,
    [EnumMember(Value = "SUBSCRIBED")] Subscribed,
    [EnumMember(Value = "UNSUBSCRIBED")] Unsubscribed,
    [EnumMember(Value = "DELAYED")] Delayed,
    [EnumMember(Value = "REJECTED")] Rejected
}

[JsonConverter(typeof(SevkEnumJsonConverter<WebhookEventCategory>))]
public enum WebhookEventCategory
{
    [EnumMember(Value = "email")] Email,
    [EnumMember(Value = "subscription")] Subscription,
    [EnumMember(Value = "broadcast")] Broadcast
}

[JsonConverter(typeof(SevkEnumJsonConverter<WebhookEventType>))]
public enum WebhookEventType
{
    [EnumMember(Value = "email.sent")] EmailSent,
    [EnumMember(Value = "email.delivered")] EmailDelivered,
    [EnumMember(Value = "email.opened")] EmailOpened,
    [EnumMember(Value = "email.clicked")] EmailClicked,
    [EnumMember(Value = "email.bounced")] EmailBounced,
    [EnumMember(Value = "email.complained")] EmailComplained,
    [EnumMember(Value = "email.rejected")] EmailRejected,
    [EnumMember(Value = "email.delayed")] EmailDelayed,
    [EnumMember(Value = "email.rendering_failed")] EmailRenderingFailed,
    [EnumMember(Value = "contact.subscribed")] ContactSubscribed,
    [EnumMember(Value = "contact.unsubscribed")] ContactUnsubscribed,
    [EnumMember(Value = "broadcast.started")] BroadcastStarted,
    [EnumMember(Value = "broadcast.finished")] BroadcastFinished,
    [EnumMember(Value = "email.received")] EmailReceived
}

[JsonConverter(typeof(SevkEnumJsonConverter<BroadcastStatusWarningType>))]
public enum BroadcastStatusWarningType
{
    [EnumMember(Value = "FAILED_EMAILS")] FailedEmails
}

[JsonConverter(typeof(SevkEnumJsonConverter<ActivityCategory>))]
public enum ActivityCategory
{
    [EnumMember(Value = "EMAIL")] Email,
    [EnumMember(Value = "SUBSCRIPTION")] Subscription,
    [EnumMember(Value = "all")] All
}

[JsonConverter(typeof(SevkEnumJsonConverter<ActivityType>))]
public enum ActivityType
{
    [EnumMember(Value = "EMAIL")] Email,
    [EnumMember(Value = "SUBSCRIPTION")] Subscription
}
