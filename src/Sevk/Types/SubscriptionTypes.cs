namespace Sevk.Types;

public class SubscribeResponseContact
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public bool Subscribed { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

public class SubscribeResponseAudience
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
}

public class SubscribeResponse
{
    public SubscribeResponseContact Contact { get; set; } = new();
    public SubscribeResponseAudience? Audience { get; set; }
    public bool? AlreadySubscribed { get; set; }
    public string? Message { get; set; }
}

public class UnsubscribeResponseContact
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public bool Subscribed { get; set; }
}

public class UnsubscribeResponse
{
    public UnsubscribeResponseContact Contact { get; set; } = new();
    public bool? AlreadyUnsubscribed { get; set; }
}

public class SubscribeRequest
{
    public string Email { get; set; } = "";
    public string? AudienceId { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

public class UnsubscribeRequest
{
    public string? Email { get; set; }
    public string? ContactId { get; set; }
    public string? BroadcastId { get; set; }
}
