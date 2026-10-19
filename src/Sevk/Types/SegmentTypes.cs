using System.Text.Json.Serialization;

namespace Sevk.Types;

public class Segment
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public List<SegmentRule> Rules { get; set; } = new();
    public LogicalOperator Operator { get; set; } = LogicalOperator.And;
    public List<string> LimitToTopics { get; set; } = new();
    public List<SegmentGroup>? Groups { get; set; }
    public string AudienceId { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string UpdatedAt { get; set; } = "";
    public string? LastRunAt { get; set; }
    public int? LastRunCount { get; set; }
}

public class SegmentRule
{
    public string Field { get; set; } = "";
    public SegmentRuleOperator Operator { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonRequired]
    public object? Value { get; set; }
    public string? GroupId { get; set; }
    public LogicalOperator? GroupOperator { get; set; }
}

public class SegmentGroup
{
    public string Id { get; set; } = "";
    public LogicalOperator Operator { get; set; } = LogicalOperator.And;
}

public class PreviewSegmentContact
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public bool Subscribed { get; set; }
    public Dictionary<string, object>? Data { get; set; }
    public string CreatedAt { get; set; } = "";
}

public class PreviewSegmentResponse
{
    public int Count { get; set; }
    public List<PreviewSegmentContact> Contacts { get; set; } = new();
}

public class CalculateSegmentResponse
{
    public int Count { get; set; }
    public string SegmentId { get; set; } = "";
    public string SegmentName { get; set; } = "";
    public string LastRunAt { get; set; } = "";
}

public class CreateSegmentRequest
{
    public string Name { get; set; } = "";
    public List<SegmentRule> Rules { get; set; } = new();
    public LogicalOperator Operator { get; set; } = LogicalOperator.And;
    public List<string>? LimitToTopics { get; set; }
    public List<SegmentGroup>? Groups { get; set; }
}

public class UpdateSegmentRequest
{
    public string? Name { get; set; }
    public List<SegmentRule>? Rules { get; set; }
    public LogicalOperator? Operator { get; set; }
    public List<string>? LimitToTopics { get; set; }
    public List<SegmentGroup>? Groups { get; set; }
}

public class PreviewSegmentRequest
{
    public List<SegmentRule> Rules { get; set; } = new();
    public LogicalOperator Operator { get; set; } = LogicalOperator.And;
    public List<string>? LimitToTopics { get; set; }
    public List<SegmentGroup>? Groups { get; set; }
}
