using Sevk.Types;

namespace Sevk.Resources;

public class AudiencesResource
{
    private readonly SevkClient _client;

    public AudiencesResource(SevkClient client) => _client = client;

    /// <summary>Lists audiences with optional pagination and search.</summary>
    public async Task<ListResponse<Audience>> ListAsync(ListParams? p = null)
    {
        if (p == null)
        {
            return await _client.GetAsync<ListResponse<Audience>>("/audiences");
        }

        var query = new QueryParameters()
            .AddPagination(p)
            .Build(includeQuestionMarkWhenEmpty: true);

        return await _client.GetAsync<ListResponse<Audience>>($"/audiences{query}");
    }

    /// <summary>Retrieves an audience by id.</summary>
    public async Task<Audience> GetAsync(string id) =>
        await _client.GetAsync<Audience>($"/audiences/{id}");

    /// <summary>Creates an audience for organizing contacts, topics, and segments.</summary>
    public async Task<AudienceMutationResponse> CreateAsync(CreateAudienceRequest req) =>
        await _client.PostAsync<AudienceMutationResponse>("/audiences", req);

    /// <summary>Updates an audience while preserving omitted fields.</summary>
    public async Task<AudienceMutationResponse> UpdateAsync(string id, UpdateAudienceRequest req) =>
        await _client.PutAsync<AudienceMutationResponse>($"/audiences/{id}", req);

    /// <summary>Adds contacts to an audience, ignoring existing memberships.</summary>
    public async Task<AddContactsToAudienceResponse> AddContactsAsync(string id, List<string> contactIds) =>
        await _client.PostAsync<AddContactsToAudienceResponse>($"/audiences/{id}/contacts", new { contactIds });

    /// <summary>Removes a contact from an audience without deleting it.</summary>
    public async Task<RemoveAudienceContactResponse> RemoveContactAsync(string id, string contactId) =>
        await _client.DeleteAsync<RemoveAudienceContactResponse>($"/audiences/{id}/contacts/{contactId}");

    /// <summary>Deletes an audience without deleting its contacts.</summary>
    public async Task<DeleteResponse> DeleteAsync(string id) =>
        await _client.DeleteAsync<DeleteResponse>($"/audiences/{id}");

    /// <summary>Lists contacts attached to an audience with optional filtering.</summary>
    public async Task<ListResponse<Contact>> ListContactsAsync(string id, ListAudienceContactsParams? p = null)
    {
        var query = BuildAudienceContactsQuery(p);

        return await _client.GetAsync<ListResponse<Contact>>($"/audiences/{id}/contacts{query}");
    }

    private string BuildAudienceContactsQuery(ListAudienceContactsParams? p)
    {
        if (p == null)
        {
            return "";
        }

        return new QueryParameters()
            .AddPagination(p)
            .Add("subscribed", p.Subscribed)
            .Add("excludeTopicId", p.ExcludeTopicId)
            .Build(includeQuestionMarkWhenEmpty: true);
    }
}
