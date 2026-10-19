using System.Text.Json.Serialization;

namespace Sevk.Types;

public class BroadcastBase
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public ContentStyle Style { get; set; } = ContentStyle.Markup;
    public BroadcastLifecycleStatus Status { get; set; } = BroadcastLifecycleStatus.Draft;
    public BroadcastTargetType TargetType { get; set; } = BroadcastTargetType.All;
    public string? AudienceId { get; set; }
    public string? TopicId { get; set; }
    public string? SegmentId { get; set; }
    public string? Email { get; set; }
    public string? SenderName { get; set; }
    public string? ReplyTo { get; set; }
    public bool? ReplyToInbound { get; set; }
    public string? DomainId { get; set; }
    public string? ScheduledAt { get; set; }
    public string? SentAt { get; set; }
    public string? CompletedAt { get; set; }
    public string ProjectId { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string UpdatedAt { get; set; } = "";

    [JsonPropertyName("delivered")]
    public string? Delivered { get; set; }

    [JsonPropertyName("includeListUnsubscribeHeader")]
    public bool? IncludeListUnsubscribeHeader { get; set; }

    [JsonPropertyName("openTracking")]
    public bool? OpenTracking { get; set; }

    [JsonPropertyName("clickTracking")]
    public bool? ClickTracking { get; set; }

    [JsonPropertyName("audience")]
    public BroadcastAudienceRef? Audience { get; set; }

    [JsonPropertyName("topic")]
    public BroadcastTopicRef? Topic { get; set; }

    [JsonPropertyName("segment")]
    public BroadcastSegmentRef? Segment { get; set; }

    [JsonPropertyName("domain")]
    public BroadcastDomainRef? Domain { get; set; }
}

public class BroadcastSummary : BroadcastBase
{
    public string? EstimatedCost { get; set; }
    public string? ActualCost { get; set; }
    public int? TotalContacts { get; set; }
    public int? SentCount { get; set; }
    public int? FailedCount { get; set; }
    public int? DeliveredCount { get; set; }
    public int? OpenedCount { get; set; }
    public int? ClickedCount { get; set; }
    public int? BouncedCount { get; set; }
    public int? ComplainedCount { get; set; }
    public int? UnsubscribedCount { get; set; }
}

public class BroadcastWithVariants : BroadcastBase
{
    public List<BroadcastVariant>? Variants { get; set; }
    public BroadcastAbTest? AbTest { get; set; }
}

/// <summary>Update response; experiment fields are present when the mutation touched them.</summary>
public class BroadcastUpdate : BroadcastBase
{
    public List<BroadcastVariant>? Variants { get; set; }
    public BroadcastAbTest? AbTest { get; set; }
}

public class Broadcast : BroadcastSummary
{
    public List<BroadcastVariant>? Variants { get; set; }
    public BroadcastAbTest? AbTest { get; set; }
}

public class BroadcastAudienceRef
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int ContactsCount { get; set; }
    public int SubscribedContactsCount { get; set; }
    public int BroadcastsCount { get; set; }
    public int TopicsCount { get; set; }
    public int SegmentsCount { get; set; }
}

public class BroadcastTopicRef
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int ContactsCount { get; set; }
    public int SubscribedContactsCount { get; set; }
    public int BroadcastsCount { get; set; }
}

public class BroadcastSegmentRef
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
}

public class BroadcastDomainRef
{
    public string Id { get; set; } = "";
    public string Domain { get; set; } = "";
    public string? SenderName { get; set; }
    public bool Verified { get; set; }
}

public class BroadcastVariant
{
    public string Id { get; set; } = "";
    [JsonPropertyName("broadcastId")]
    public string BroadcastId { get; set; } = "";
    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = "";
    [JsonPropertyName("updatedAt")]
    public string UpdatedAt { get; set; } = "";
    public string Label { get; set; } = "";
    public string? Subject { get; set; }
    public string? Body { get; set; }
    [JsonPropertyName("fromName")]
    public string? FromName { get; set; }
    [JsonPropertyName("totalContacts")]
    public int TotalContacts { get; set; }
    [JsonPropertyName("sentCount")]
    public int SentCount { get; set; }
    [JsonPropertyName("failedCount")]
    public int FailedCount { get; set; }
    [JsonPropertyName("deliveredCount")]
    public int DeliveredCount { get; set; }
    [JsonPropertyName("openedCount")]
    public int OpenedCount { get; set; }
    [JsonPropertyName("clickedCount")]
    public int ClickedCount { get; set; }
    [JsonPropertyName("bouncedCount")]
    public int BouncedCount { get; set; }
    [JsonPropertyName("complainedCount")]
    public int ComplainedCount { get; set; }
    [JsonPropertyName("unsubscribedCount")]
    public int UnsubscribedCount { get; set; }
    [JsonPropertyName("isWinner")]
    public bool IsWinner { get; set; }
}

public class BroadcastAbTest
{
    public string Id { get; set; } = "";
    [JsonPropertyName("broadcastId")]
    public string BroadcastId { get; set; } = "";
    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = "";
    [JsonPropertyName("updatedAt")]
    public string UpdatedAt { get; set; } = "";
    [JsonPropertyName("testPercent")]
    public int TestPercent { get; set; }
    [JsonPropertyName("evalAfterMinutes")]
    public int EvalAfterMinutes { get; set; }
    [JsonPropertyName("winnerMetric")]
    public AbTestMetric WinnerMetric { get; set; }
    public AbTestStatus Status { get; set; }
    [JsonPropertyName("scheduledEvalAt")]
    public string? ScheduledEvalAt { get; set; }
    [JsonPropertyName("winnerVariantId")]
    public string? WinnerVariantId { get; set; }
    [JsonPropertyName("evaluatedAt")]
    public string? EvaluatedAt { get; set; }
}

public class ActiveBroadcast
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public BroadcastLifecycleStatus Status { get; set; }
    [JsonPropertyName("sentCount")]
    public int SentCount { get; set; }
    [JsonPropertyName("failedCount")]
    public int FailedCount { get; set; }
    [JsonPropertyName("totalContacts")]
    public int TotalContacts { get; set; }
    [JsonPropertyName("scheduledAt")]
    public string? ScheduledAt { get; set; }
    [JsonPropertyName("abTest")]
    public ActiveBroadcastAbTest? AbTest { get; set; }
}

public class ActiveBroadcastAbTest
{
    public AbTestStatus Status { get; set; }
    [JsonPropertyName("scheduledEvalAt")]
    public string? ScheduledEvalAt { get; set; }
    [JsonPropertyName("testPercent")]
    public int TestPercent { get; set; }
}

public class ActiveBroadcastListResponse
{
    public List<ActiveBroadcast> Items { get; set; } = new();
    public int Total { get; set; }
}

public class VariantInput
{
    public string Label { get; set; } = "";
    public string? Subject { get; set; }
    public string? Body { get; set; }
    [JsonPropertyName("fromName")]
    public string? FromName { get; set; }
}

public class AbTestConfigInput
{
    [JsonPropertyName("testPercent")]
    [JsonRequired]
    public int TestPercent { get; set; }
    [JsonPropertyName("evalAfterMinutes")]
    [JsonRequired]
    public int EvalAfterMinutes { get; set; }
    [JsonPropertyName("winnerMetric")]
    public AbTestMetric? WinnerMetric { get; set; }
}

public class BroadcastCancelResponse
{
    [JsonPropertyName("broadcast")]
    public BroadcastSummary? Broadcast { get; set; }

    [JsonPropertyName("cancelledJobs")]
    public int CancelledJobs { get; set; }
}

public class CreateBroadcastRequest
{
    public string Name { get; set; } = "";
    public string Subject { get; set; } = "";
    public string? Body { get; set; }
    public ContentStyle? Style { get; set; }
    public BroadcastTargetType? TargetType { get; set; }
    public string? AudienceId { get; set; }
    public string? TopicId { get; set; }
    public string? SegmentId { get; set; }
    public string SenderName { get; set; } = "";
    public string? Email { get; set; }
    public string? ReplyTo { get; set; }
    public bool? ReplyToInbound { get; set; }
    public string DomainId { get; set; } = "";
    public string? ScheduledAt { get; set; }
    public bool? IncludeListUnsubscribeHeader { get; set; }
    public bool? OpenTracking { get; set; }
    public bool? ClickTracking { get; set; }
    public List<VariantInput>? Variants { get; set; }
    [JsonPropertyName("abTest")]
    public AbTestConfigInput? AbTest { get; set; }
}

public class UpdateBroadcastRequest
{
    public string? Name { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public ContentStyle? Style { get; set; }
    public BroadcastTargetType? TargetType { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<string> AudienceId { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<string> TopicId { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<string> SegmentId { get; set; }
    public string? SenderName { get; set; }
    public string? Email { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<string> ReplyTo { get; set; }
    public bool? ReplyToInbound { get; set; }
    public string? DomainId { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<string> ScheduledAt { get; set; }
    public bool? IncludeListUnsubscribeHeader { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<bool?> OpenTracking { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<bool?> ClickTracking { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<List<VariantInput>> Variants { get; set; }
    [JsonPropertyName("abTest")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<AbTestConfigInput> AbTest { get; set; }
}

public class ListBroadcastsParams : ListParams
{
    public BroadcastLifecycleStatus? Status { get; set; }
}

public class ListBroadcastEmailsParams
{
    public int? Page { get; set; }
    public int? Limit { get; set; }
    public string? After { get; set; }
    public string? Before { get; set; }
}

public class BroadcastStatusWarning
{
    public BroadcastStatusWarningType Type { get; set; }
    public string Message { get; set; } = "";
    public int FailedCount { get; set; }
}

public class BroadcastStatusProgress
{
    public int Percentage { get; set; }
    public int EmailsSent { get; set; }
    public int TotalContacts { get; set; }
    public int PendingEmails { get; set; }
    public string? EstimatedTimeRemaining { get; set; }
}

public class BroadcastStatusSchedule
{
    public string? ScheduledAt { get; set; }
    public long TimeUntilSend { get; set; }
    public string WillSendIn { get; set; } = "";
    public bool IsPastDue { get; set; }
}

public class BroadcastStatusCompletion
{
    public string? CompletedAt { get; set; }
    public int TotalEmailsSent { get; set; }
    public int SuccessRate { get; set; }
}

public class BroadcastStatus
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    [JsonPropertyName("status")]
    public BroadcastLifecycleStatus Status { get; set; }
    public int TotalContacts { get; set; }
    public int EmailsSent { get; set; }
    public int FailedEmails { get; set; }
    public int PendingEmails { get; set; }
    public string CreatedAt { get; set; } = "";
    public string? SentAt { get; set; }
    public string? CompletedAt { get; set; }
    public string? ScheduledAt { get; set; }
    public string? SendingStartedAt { get; set; }
    public string? Step { get; set; }
    public string? StepLabel { get; set; }
    public int? StepIndex { get; set; }
    public int? StepTotal { get; set; }
    public double? StepProgress { get; set; }
    public BroadcastStatusWarning? Warning { get; set; }
    public BroadcastStatusProgress? Progress { get; set; }
    public BroadcastStatusSchedule? Schedule { get; set; }
    public BroadcastStatusCompletion? Completion { get; set; }
}

public class BroadcastEmail
{
    public string Id { get; set; } = "";
    public string? MessageId { get; set; }
    public string To { get; set; } = "";
    public string Subject { get; set; } = "";
    public EmailStatus Status { get; set; }
    public EmailType? Type { get; set; }
    public string? OpenedAt { get; set; }
    public string? ClickedAt { get; set; }
    public string? UnsubscribedAt { get; set; }
    public string CreatedAt { get; set; } = "";
    public BroadcastEmailContact Contact { get; set; } = new();
}

public class BroadcastEmailContact
{
    public string? Id { get; set; }
    public string Email { get; set; } = "";
}

public class BroadcastCostEstimate
{
    public double EstimatedCost { get; set; }
    public double EmailPrice { get; set; }
    public int ContactCount { get; set; }
    public double CurrentBalance { get; set; }
    public bool HasSufficientBalance { get; set; }
    public double Shortfall { get; set; }
    public int FreeEmailsAvailable { get; set; }
    public int FreeEmailsToUse { get; set; }
    public int PaidEmailsCount { get; set; }
}

public class BroadcastRecipientEstimateRequest
{
    public BroadcastTargetType TargetType { get; set; }
    public string? AudienceId { get; set; }
    public string? TopicId { get; set; }
    public string? SegmentId { get; set; }
}

public class BroadcastRecipientEstimate
{
    public int Count { get; set; }
}

public class SendTestResponse
{
    public int TestEmails { get; set; }
    public string Message { get; set; } = "";
}

public class BroadcastAnalytics
{
    public int TotalEmails { get; set; }
    public int SentEmails { get; set; }
    public int DeliveredEmails { get; set; }
    public int OpenedEmails { get; set; }
    public int ClickedEmails { get; set; }
    public int TotalClicks { get; set; }
    public int BouncedEmails { get; set; }
    public int FailedEmails { get; set; }
    public int UnsubscribedEmails { get; set; }
    public double DeliveryRate { get; set; }
    public double OpenRate { get; set; }
    public double ClickRate { get; set; }
    public double BounceRate { get; set; }
    public double UnsubscribeRate { get; set; }
}
