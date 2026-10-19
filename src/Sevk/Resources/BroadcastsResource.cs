using Sevk.Types;

namespace Sevk.Resources;

public class BroadcastsResource
{
    private readonly SevkClient _client;

    public BroadcastsResource(SevkClient client) => _client = client;

    /// <summary>Lists broadcasts with optional pagination, search, and status filters.</summary>
    public async Task<ListResponse<BroadcastSummary>> ListAsync(ListBroadcastsParams? p = null)
    {
        var query = BuildBroadcastsQuery(p);

        return await _client.GetAsync<ListResponse<BroadcastSummary>>($"/broadcasts{query}");
    }

    private static string BuildBroadcastsQuery(ListBroadcastsParams? p)
    {
        if (p == null)
        {
            return "";
        }

        return new QueryParameters()
            .AddPagination(p)
            .Add("status", p.Status)
            .Build(includeQuestionMarkWhenEmpty: true);
    }

    /// <summary>Retrieves a broadcast by id and includes its delivery state.</summary>
    public async Task<Broadcast> GetAsync(string id) =>
        await _client.GetAsync<Broadcast>($"/broadcasts/{id}");

    /// <summary>Returns live scheduling and delivery progress for a broadcast.</summary>
    public async Task<BroadcastStatus> StatusAsync(string id) =>
        await _client.GetAsync<BroadcastStatus>($"/broadcasts/{id}/status");

    /// <summary>Lists email deliveries created for a broadcast.</summary>
    public async Task<ListResponse<BroadcastEmail>> EmailsAsync(string id, ListBroadcastEmailsParams? p = null)
    {
        var query = p is null
            ? ""
            : new QueryParameters()
                .AddPagination(p.Page, p.Limit, p.After, p.Before)
                .Build();

        return await _client.GetAsync<ListResponse<BroadcastEmail>>($"/broadcasts/{id}/emails{query}");
    }

    /// <summary>Estimates delivery cost without sending the broadcast.</summary>
    public async Task<BroadcastCostEstimate> EstimateCostAsync(string id) =>
        await _client.GetAsync<BroadcastCostEstimate>($"/broadcasts/{id}/estimate-cost");

    /// <summary>Resolves the subscribed recipient count for a broadcast target.</summary>
    public async Task<BroadcastRecipientEstimate> EstimateRecipientsAsync(BroadcastRecipientEstimateRequest req) =>
        await _client.PostAsync<BroadcastRecipientEstimate>("/broadcasts/estimate-recipients", req);

    /// <summary>Lists broadcasts that are currently scheduled or processing.</summary>
    public async Task<ActiveBroadcastListResponse> ActiveAsync() =>
        await _client.GetAsync<ActiveBroadcastListResponse>("/broadcasts/active");

    /// <summary>Creates a draft or scheduled broadcast for a target group.</summary>
    public async Task<BroadcastWithVariants> CreateAsync(CreateBroadcastRequest req)
    {

        return await _client.PostAsync<BroadcastWithVariants>("/broadcasts", req);
    }

    /// <summary>Updates a broadcast while preserving omitted fields.</summary>
    public async Task<BroadcastUpdate> UpdateAsync(string id, UpdateBroadcastRequest req)
    {

        return await _client.PutAsync<BroadcastUpdate>($"/broadcasts/{id}", req);
    }

    /// <summary>Deletes a broadcast that has not been sent.</summary>
    public async Task<DeleteResponse> DeleteAsync(string id) =>
        await _client.DeleteAsync<DeleteResponse>($"/broadcasts/{id}");

    /// <summary>Queues a prepared broadcast for delivery.</summary>
    public async Task<BroadcastSummary> SendAsync(string id) =>
        await _client.PostAsync<BroadcastSummary>($"/broadcasts/{id}/send");

    /// <summary>Cancels a broadcast when its state still allows cancellation.</summary>
    public async Task<BroadcastCancelResponse> CancelAsync(string id) =>
        await _client.PostAsync<BroadcastCancelResponse>($"/broadcasts/{id}/cancel");

    /// <summary>Sends a broadcast to test recipients without starting the campaign.</summary>
    public async Task<SendTestResponse> TestAsync(string id, List<string> emails) =>
        await TestAsync(id, emails, null);

    /// <summary>Sends a broadcast to test recipients with optional retry idempotency.</summary>
    public async Task<SendTestResponse> TestAsync(string id, List<string> emails, BroadcastTestOptions? options)
    {
        IDictionary<string, string>? headers = options?.IdempotencyKey == null
            ? null
            : new Dictionary<string, string> { { "Idempotency-Key", options!.IdempotencyKey! } };

        return await _client.PostAsync<SendTestResponse>($"/broadcasts/{id}/test", new { emails }, headers);
    }

    /// <summary>Returns aggregate delivery and engagement analytics for a broadcast.</summary>
    public async Task<BroadcastAnalytics> AnalyticsAsync(string id) =>
        await _client.GetAsync<BroadcastAnalytics>($"/broadcasts/{id}/analytics");

}

/// <summary>Per-call options for a retry-safe broadcast test send.</summary>
public class BroadcastTestOptions
{
    public string? IdempotencyKey { get; set; }
}
