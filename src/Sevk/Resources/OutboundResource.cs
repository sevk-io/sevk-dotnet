using Sevk.Types;

namespace Sevk.Resources;

public class OutboundResource
{
    private readonly SevkClient _client;

    public OutboundResource(SevkClient client) => _client = client;

    /// <summary>Returns outbound quota settings and current usage counters.</summary>
    public async Task<OutboundQuota> GetQuotaAsync() =>
        await _client.GetAsync<OutboundQuota>("/outbound-settings/quota");

    /// <summary>Updates the monthly cap and clears a cap-exhausted pause when possible.</summary>
    public async Task<OutboundQuota> UpdateQuotaAsync(UpdateOutboundQuotaRequest req) =>
        await _client.PutAsync<OutboundQuota>("/outbound-settings/quota", req);

    /// <summary>Clears an outbound pause while preserving usage and the configured cap.</summary>
    public async Task<OutboundQuota> ResetQuotaAsync() =>
        await _client.PostAsync<OutboundQuota>("/outbound-settings/quota/reset");

    /// <summary>Resets custom-limit usage while preserving the cap and monthly usage.</summary>
    public async Task<OutboundQuota> ResetLimitAsync() =>
        await _client.PostAsync<OutboundQuota>("/outbound-settings/quota/reset-limit");

    /// <summary>Pauses transactional sending until the quota is reset.</summary>
    public async Task<OutboundQuota> PauseQuotaAsync() =>
        await _client.PostAsync<OutboundQuota>("/outbound-settings/quota/pause");
}
