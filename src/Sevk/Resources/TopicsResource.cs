using Sevk.Types;

namespace Sevk.Resources;

public class TopicsResource
{
    private readonly SevkClient _client;

    public TopicsResource(SevkClient client) => _client = client;

    /// <summary>Lists topics belonging to an audience with optional filtering.</summary>
    public async Task<ListResponse<Topic>> ListAsync(string audienceId, ListParams? p = null)
    {
        if (p == null)
        {
            return await _client.GetAsync<ListResponse<Topic>>($"/audiences/{audienceId}/topics");
        }

        var query = new QueryParameters()
            .AddPagination(p)
            .Build(includeQuestionMarkWhenEmpty: true);

        return await _client.GetAsync<ListResponse<Topic>>($"/audiences/{audienceId}/topics{query}");
    }

    /// <summary>Creates a subscription topic under an audience.</summary>
    public async Task<TopicMutationResponse> CreateAsync(string audienceId, CreateTopicRequest req) =>
        await _client.PostAsync<TopicMutationResponse>($"/audiences/{audienceId}/topics", req);

    /// <summary>Retrieves a topic by id.</summary>
    public async Task<Topic> GetAsync(string id) =>
        await _client.GetAsync<Topic>($"/topics/{id}");

    /// <summary>Updates a topic while preserving omitted fields.</summary>
    public async Task<TopicMutationResponse> UpdateAsync(string id, UpdateTopicRequest req) =>
        await _client.PutAsync<TopicMutationResponse>($"/topics/{id}", req);

    /// <summary>Deletes a topic without deleting its contacts or parent audience.</summary>
    public async Task<DeleteResponse> DeleteAsync(string id) =>
        await _client.DeleteAsync<DeleteResponse>($"/topics/{id}");

    /// <summary>Adds contacts to a topic, ignoring existing memberships.</summary>
    public async Task<AddContactsToTopicResponse> AddContactsAsync(string topicId, List<string> contactIds) =>
        await _client.PostAsync<AddContactsToTopicResponse>($"/topics/{topicId}/contacts", new { contactIds });

    /// <summary>Removes a contact from a topic without deleting it.</summary>
    public async Task<RemoveTopicContactResponse> RemoveContactAsync(string topicId, string contactId) =>
        await _client.DeleteAsync<RemoveTopicContactResponse>($"/topics/{topicId}/contacts/{contactId}");

    /// <summary>Lists contacts attached to a topic with optional filtering.</summary>
    public async Task<ListResponse<Contact>> ListContactsAsync(string topicId, ListTopicContactsParams? p = null)
    {
        var query = "";

        if (p != null)
        {
            query = new QueryParameters()
                .AddPagination(p)
                .Add("subscribed", p.Subscribed)
                .Build(includeQuestionMarkWhenEmpty: true);
        }

        return await _client.GetAsync<ListResponse<Contact>>($"/topics/{topicId}/contacts{query}");
    }
}
