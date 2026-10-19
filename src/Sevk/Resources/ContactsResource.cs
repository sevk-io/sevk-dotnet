using Sevk.Types;

namespace Sevk.Resources;

public class ContactsResource
{
    private readonly SevkClient _client;

    public ContactsResource(SevkClient client) => _client = client;

    /// <summary>Lists contacts with optional pagination, search, and filters.</summary>
    public async Task<ListResponse<Contact>> ListAsync(ListContactsParams? p = null)
    {
        var query = BuildContactsQuery(p);

        return await _client.GetAsync<ListResponse<Contact>>($"/contacts{query}");
    }

    private string BuildContactsQuery(ListContactsParams? p)
    {
        if (p == null)
        {
            return "";
        }

        return new QueryParameters()
            .AddPagination(p)
            .Add("excludeAudience", p.ExcludeAudience)
            .Add("subscribed", p.Subscribed)
            .Build(includeQuestionMarkWhenEmpty: true);
    }

    /// <summary>Retrieves a contact by id.</summary>
    public async Task<Contact> GetAsync(string id) =>
        await _client.GetAsync<Contact>($"/contacts/{id}");

    /// <summary>Creates a contact and optionally attaches custom metadata.</summary>
    public async Task<Contact> CreateAsync(string email, CreateContactRequest? req = null)
    {
        var body = new CreateContactRequest
        {
            Email = email,
            Subscribed = req?.Subscribed,
            Data = req?.Data
        };

        return await _client.PostAsync<Contact>("/contacts", body);
    }

    /// <summary>Updates a contact while preserving omitted fields.</summary>
    public async Task<Contact> UpdateAsync(string id, UpdateContactRequest req) =>
        await _client.PutAsync<Contact>($"/contacts/{id}", req);

    /// <summary>Deletes a contact and removes its audience and topic memberships.</summary>
    public async Task<DeleteResponse> DeleteAsync(string id) =>
        await _client.DeleteAsync<DeleteResponse>($"/contacts/{id}");

    /// <summary>Updates contacts by email and reports entries that were not found.</summary>
    public async Task<BulkUpdateContactsResponse> BulkUpdateAsync(List<BulkUpdateContactEntry> contacts) =>
        await _client.PutAsync<BulkUpdateContactsResponse>("/contacts/bulk-update", new { contacts });

    /// <summary>Imports contacts and optionally attaches them to a group.</summary>
    public async Task<ImportContactsResponse> BulkImportAsync(ImportContactsRequest req) =>
        await _client.PostAsync<ImportContactsResponse>("/contacts/import", req);

    /// <summary>Lists delivery and engagement activity for a contact.</summary>
    public async Task<ListResponse<ContactActivityItem>> ActivityAsync(string id, PaginationParams? p = null)
    {
        var query = BuildQuery(p);

        return await _client.GetAsync<ListResponse<ContactActivityItem>>($"/contacts/{id}/activity{query}");
    }

    private string BuildQuery(PaginationParams? p)
    {
        if (p == null)
        {
            return "";
        }

        return new QueryParameters()
            .AddPagination(p)
            .Build(includeQuestionMarkWhenEmpty: true);
    }
}
