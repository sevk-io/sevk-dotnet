using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sevk.Types;

public class Contact
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public bool Subscribed { get; set; }
    public Dictionary<string, JsonElement>? Data { get; set; }
    public string CreatedAt { get; set; } = "";
    public string UpdatedAt { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public string? LastEventAt { get; set; }

    [JsonPropertyName("unsubscribedAt")]
    public string? UnsubscribedAt { get; set; }

    [JsonPropertyName("unsubscribedBy")]
    public ContactUnsubscribeActor? UnsubscribedBy { get; set; }

    [JsonPropertyName("resubscriptionLocked")]
    public bool ResubscriptionLocked { get; set; }

    [JsonPropertyName("bounceCount")]
    public int BounceCount { get; set; }

    [JsonPropertyName("lastBouncedAt")]
    public string? LastBouncedAt { get; set; }
}

public class CreateContactRequest
{
    public string Email { get; set; } = "";
    public bool? Subscribed { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}

public class UpdateContactRequest
{
    public string? Email { get; set; }
    public bool? Subscribed { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<Dictionary<string, object>> Data { get; set; }
}

public class BulkUpdateContactEntry
{
    public string Email { get; set; } = "";
    public bool? Subscribed { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<Dictionary<string, object>> Data { get; set; }
}

public class BulkUpdateContactsResponse
{
    public int Updated { get; set; }
    public int NotFound { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> UpdatedEmails { get; set; } = new();
    public List<string> NotFoundEmails { get; set; } = new();
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
    public string? TopicId { get; set; }
}

public class ImportContactsResponseContact
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public bool Subscribed { get; set; }
}

public class ImportContactsResponse
{
    public int Created { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<ImportContactsResponseContact> Contacts { get; set; } = new();
}
