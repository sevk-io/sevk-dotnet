using System.Text.Json;

namespace Sevk.Types;

public class ListActivityParams : ListParams
{
    public ActivityCategory? Category { get; set; }
    public IReadOnlyList<ActivityAction>? Type { get; set; }
    public int? Days { get; set; }
}

public class ActivityItem
{
    public string Id { get; set; } = "";
    public ActivityType EventType { get; set; }
    public ActivityAction Action { get; set; }
    public string? Description { get; set; }
    public string? EmailId { get; set; }
    public string? EmailSubject { get; set; }
    public string? ContactId { get; set; }
    public string? ContactEmail { get; set; }
    public string? BroadcastId { get; set; }
    public string? BroadcastName { get; set; }
    public string? Link { get; set; }
    public string? UserAgent { get; set; }
    public string? Metadata { get; set; }
    public string CreatedAt { get; set; } = "";
}

public class ActivityBroadcastRef
{
    public string Id { get; set; } = "";
    public string? Name { get; set; }
}

public class ActivityEmailRef
{
    public string Id { get; set; } = "";
    public string? Subject { get; set; }
}

public class ContactActivityItem
{
    public string Id { get; set; } = "";
    public ActivityAction Action { get; set; }
    public string? Description { get; set; }
    public string CreatedAt { get; set; } = "";
    public string? UserAgent { get; set; }
    public string? Link { get; set; }
    public JsonElement? Metadata { get; set; }
    public ActivityBroadcastRef? Broadcast { get; set; }
    public ActivityEmailRef? Email { get; set; }
}

public class ActivityRates
{
    public double Open { get; set; }
    public double Click { get; set; }
    public double Bounce { get; set; }
    public double Delivery { get; set; }
}

public class ActivityStats
{
    public int TotalEmails { get; set; }
    public int SentEmails { get; set; }
    public int DeliveredEmails { get; set; }
    public int OpenedEmails { get; set; }
    public int ClickedEmails { get; set; }
    public int BouncedEmails { get; set; }
    public int MarketingEmails { get; set; }
    public int TransactionalEmails { get; set; }
    public ActivityRates Rates { get; set; } = new();
}
