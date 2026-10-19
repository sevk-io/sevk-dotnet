using Sevk.Types;

namespace Sevk.Resources;

public class TemplatesResource
{
    private readonly SevkClient _client;

    public TemplatesResource(SevkClient client) => _client = client;

    /// <summary>Lists reusable email templates with optional pagination and search.</summary>
    public async Task<ListResponse<Template>> ListAsync(ListParams? p = null)
    {
        if (p == null)
        {
            return await _client.GetAsync<ListResponse<Template>>("/templates");
        }

        var query = new QueryParameters()
            .AddPagination(p)
            .Build(includeQuestionMarkWhenEmpty: true);

        return await _client.GetAsync<ListResponse<Template>>($"/templates{query}");
    }

    /// <summary>Retrieves a template by id.</summary>
    public async Task<Template> GetAsync(string id) =>
        await _client.GetAsync<Template>($"/templates/{id}");

    /// <summary>Creates a reusable email template.</summary>
    public async Task<Template> CreateAsync(CreateTemplateRequest req) =>
        await _client.PostAsync<Template>("/templates", req);

    /// <summary>Updates a template while preserving omitted fields.</summary>
    public async Task<Template> UpdateAsync(string id, UpdateTemplateRequest req) =>
        await _client.PutAsync<Template>($"/templates/{id}", req);

    /// <summary>Creates an independent copy of a template for further editing.</summary>
    public async Task<Template> DuplicateAsync(string id) =>
        await _client.PostAsync<Template>($"/templates/{id}/duplicate");

    /// <summary>Deletes a template without affecting email already sent from it.</summary>
    public async Task<DeleteResponse> DeleteAsync(string id) =>
        await _client.DeleteAsync<DeleteResponse>($"/templates/{id}");
}
