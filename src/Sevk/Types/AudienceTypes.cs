using System.Text.Json.Serialization;

namespace Sevk.Types;

public class Audience
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public AudienceVisibility Visibility { get; set; } = AudienceVisibility.Public;
    public string ProjectId { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string UpdatedAt { get; set; } = "";
    public int? ContactsCount { get; set; }
    public int? SubscribedContactsCount { get; set; }
    public int? BroadcastsCount { get; set; }
    public int? TopicsCount { get; set; }
    public int? SegmentsCount { get; set; }
}

public class AudienceMutationResponse
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public AudienceVisibility Visibility { get; set; } = AudienceVisibility.Public;
    public string ProjectId { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string UpdatedAt { get; set; } = "";
}

public class AddedContact
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public bool Subscribed { get; set; }
    public string CreatedAt { get; set; } = "";
}

public class AddContactsToAudienceResponse
{
    public int Added { get; set; }
    public List<AddedContact> Contacts { get; set; } = new();
}

public class CreateAudienceRequest
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public AudienceVisibility? Visibility { get; set; }
}

public class UpdateAudienceRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public AudienceVisibility? Visibility { get; set; }
}

public class RemoveAudienceContactResponse
{
    [JsonPropertyName("audienceId")]
    public string AudienceId { get; set; } = "";
    [JsonPropertyName("contactId")]
    public string ContactId { get; set; } = "";
    [JsonPropertyName("removed")]
    public bool Removed { get; set; }
}
