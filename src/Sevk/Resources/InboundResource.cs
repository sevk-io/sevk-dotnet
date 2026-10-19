using Sevk.Types;

namespace Sevk.Resources;

public class InboundResource
{
    private readonly SevkClient _client;

    public InboundResource(SevkClient client) => _client = client;

    /// <summary>Lists received email with optional pagination and filtering.</summary>
    public async Task<InboundListResponse> ListAsync(ListInboundParams? p = null)
    {
        if (p == null)
        {
            return await _client.GetAsync<InboundListResponse>("/inbound");
        }

        var query = new QueryParameters()
            .AddPagination(p)
            .Add("status", p.Status)
            .Add("domainId", p.DomainId)
            .Add("broadcastId", p.BroadcastId)
            .Add("isSpam", p.IsSpam)
            .Add("isVirus", p.IsVirus)
            .Build();

        return await _client.GetAsync<InboundListResponse>($"/inbound{query}");
    }

    /// <summary>Retrieves a received email with parsed content and attachments.</summary>
    public async Task<InboundEmail> GetAsync(string id) =>
        await _client.GetAsync<InboundEmail>($"/inbound/{id}");

    /// <summary>Deletes a received email and its stored attachments.</summary>
    public async Task<DeleteResponse> DeleteAsync(string id) =>
        await _client.DeleteAsync<DeleteResponse>($"/inbound/{id}");

    /// <summary>Returns inbound processing statistics, optionally for one domain.</summary>
    public async Task<InboundStats> StatsAsync(string? domainId = null)
    {
        var query = domainId is null ? "" : $"?domainId={Uri.EscapeDataString(domainId)}";

        return await _client.GetAsync<InboundStats>($"/inbound/stats{query}");
    }

    /// <summary>Creates a short-lived download URL for an inbound attachment.</summary>
    public async Task<InboundAttachmentDownload> DownloadAttachmentAsync(string emailId, string attachmentId) =>
        await _client.GetAsync<InboundAttachmentDownload>($"/inbound/{emailId}/attachments/{attachmentId}/download");

    /// <summary>Lists rules used to store or forward matching inbound email.</summary>
    public async Task<InboundRoutesListResponse> ListRoutesAsync(PaginationParams? p = null)
    {
        var query = p is null
            ? ""
            : new QueryParameters()
                .AddPagination(p)
                .Build();

        return await _client.GetAsync<InboundRoutesListResponse>(
            $"/inbound-settings/routes{query}"
        );
    }

    /// <summary>Retrieves an inbound routing rule by id.</summary>
    public async Task<InboundRoute> GetRouteAsync(string id) =>
        await _client.GetAsync<InboundRoute>($"/inbound-settings/routes/{id}");

    /// <summary>Creates an inbound routing rule.</summary>
    public async Task<InboundRoute> CreateRouteAsync(CreateInboundRouteRequest req) =>
        await _client.PostAsync<InboundRoute>("/inbound-settings/routes", req);

    /// <summary>Updates an inbound routing rule while preserving omitted fields.</summary>
    public async Task<InboundRoute> UpdateRouteAsync(string id, UpdateInboundRouteRequest req) =>
        await _client.PutAsync<InboundRoute>($"/inbound-settings/routes/{id}", req);

    /// <summary>Deletes an inbound routing rule without deleting stored email.</summary>
    public async Task<DeleteResponse> DeleteRouteAsync(string id) =>
        await _client.DeleteAsync<DeleteResponse>($"/inbound-settings/routes/{id}");

    /// <summary>Lists blocked addresses using optional offset or cursor pagination.</summary>
    public async Task<InboundBlocklistListResponse> ListBlocklistAsync(PaginationParams? p = null)
    {
        var query = p is null
            ? ""
            : new QueryParameters()
                .AddPagination(p)
                .Build();

        return await _client.GetAsync<InboundBlocklistListResponse>(
            $"/inbound-settings/blocklist{query}"
        );
    }

    /// <summary>Adds an address to the inbound blocklist.</summary>
    public async Task<InboundBlocklistEntry> AddBlocklistEntryAsync(AddInboundBlocklistRequest req) =>
        await _client.PostAsync<InboundBlocklistEntry>("/inbound-settings/blocklist", req);

    /// <summary>Removes an address from the inbound blocklist.</summary>
    public async Task<DeleteResponse> RemoveBlocklistEntryAsync(string id) =>
        await _client.DeleteAsync<DeleteResponse>($"/inbound-settings/blocklist/{id}");

    /// <summary>Returns inbound quota settings and current usage counters.</summary>
    public async Task<InboundQuota> GetQuotaAsync() =>
        await _client.GetAsync<InboundQuota>("/inbound-settings/quota");

    /// <summary>Updates the inbound monthly limit while preserving omitted fields.</summary>
    public async Task<InboundQuota> UpdateQuotaAsync(UpdateInboundQuotaRequest req) =>
        await _client.PutAsync<InboundQuota>("/inbound-settings/quota", req);

    /// <summary>Clears a quota pause without resetting current-period usage.</summary>
    public async Task<InboundQuota> ResetQuotaAsync() =>
        await _client.PostAsync<InboundQuota>("/inbound-settings/quota/reset");

    /// <summary>Returns effective inbound security settings for a domain.</summary>
    public async Task<InboundSecurityConfig> GetSecurityConfigAsync(string domainId) =>
        await _client.GetAsync<InboundSecurityConfig>($"/inbound-settings/security/{domainId}");

    /// <summary>Updates inbound security settings for a domain.</summary>
    public async Task<InboundSecurityConfig> UpdateSecurityConfigAsync(string domainId, UpdateInboundSecurityRequest req) =>
        await _client.PutAsync<InboundSecurityConfig>($"/inbound-settings/security/{domainId}", req);
}
