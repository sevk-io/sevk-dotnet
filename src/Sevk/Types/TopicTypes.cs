using System.Text.Json.Serialization;

namespace Sevk.Types;

public class Topic
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string AudienceId { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string UpdatedAt { get; set; } = "";
    public int? ContactsCount { get; set; }
    public int? SubscribedContactsCount { get; set; }
    public int? BroadcastsCount { get; set; }
}

public class TopicMutationResponse
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string AudienceId { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string UpdatedAt { get; set; } = "";
}

public class AddContactsToTopicResponse
{
    public int Added { get; set; }
    public List<AddedContact> Contacts { get; set; } = new();
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

public class ListTopicContactsParams : ListParams
{
    public bool? Subscribed { get; set; }
}

public class RemoveTopicContactResponse
{
    [JsonPropertyName("topicId")]
    public string TopicId { get; set; } = "";
    [JsonPropertyName("contactId")]
    public string ContactId { get; set; } = "";
    [JsonPropertyName("removed")]
    public bool Removed { get; set; }
}
