using Sevk.Types;

namespace Sevk.Resources;

public class WebhooksResource
{
    private readonly SevkClient _client;

    public WebhooksResource(SevkClient client) => _client = client;

    public async Task<RotateWebhookResponse> RotateAsync(string id) =>
        await _client.PostAsync<RotateWebhookResponse>($"/webhooks/{id}/rotate");

    /// <summary>Lists configured webhooks and their recent delivery health.</summary>
    public async Task<WebhookListResponse> ListAsync(PaginationParams? p = null)
    {
        var query = p is null
            ? ""
            : new QueryParameters()
                .AddPagination(p)
                .Build();

        return await _client.GetAsync<WebhookListResponse>($"/webhooks{query}");
    }

    /// <summary>Retrieves a webhook and its recent delivery state.</summary>
    public async Task<Webhook> GetAsync(string id) =>
        await _client.GetAsync<Webhook>($"/webhooks/{id}");

    /// <summary>Creates a webhook and returns its usable whsec_ signing secret exactly once.</summary>
    public async Task<CreatedWebhook> CreateAsync(CreateWebhookRequest req)
    {
        return await _client.PostAsync<CreatedWebhook>("/webhooks", req);
    }

    /// <summary>Updates a webhook while preserving omitted fields.</summary>
    public async Task<Webhook> UpdateAsync(string id, UpdateWebhookRequest req)
    {
        return await _client.PutAsync<Webhook>($"/webhooks/{id}", req);
    }

    /// <summary>Deletes a webhook and stops future deliveries to its URL.</summary>
    public async Task<DeleteResponse> DeleteAsync(string id) =>
        await _client.DeleteAsync<DeleteResponse>($"/webhooks/{id}");

    /// <summary>Sends a test event and returns the delivery result.</summary>
    public async Task<WebhookTestResponse> TestAsync(string id) =>
        await _client.PostAsync<WebhookTestResponse>($"/webhooks/{id}/test");

    /// <summary>Lists event names that can be assigned to a webhook.</summary>
    public async Task<WebhookEventsResponse> EventsAsync() =>
        await _client.GetAsync<WebhookEventsResponse>("/webhooks/events");
}
