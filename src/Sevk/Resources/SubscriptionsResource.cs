using Sevk.Types;

namespace Sevk.Resources;

public class SubscriptionsResource
{
    private readonly SevkClient _client;

    public SubscriptionsResource(SevkClient client) => _client = client;

    /// <summary>Subscribes a contact and optionally adds it to a group.</summary>
    public async Task<SubscribeResponse> SubscribeAsync(SubscribeRequest req) =>
        await _client.PostAsync<SubscribeResponse>("/subscriptions/subscribe", req);

    /// <summary>Globally unsubscribes a contact and optionally attributes the action.</summary>
    public async Task<UnsubscribeResponse> UnsubscribeAsync(UnsubscribeRequest req)
    {

        return await _client.PostAsync<UnsubscribeResponse>("/subscriptions/unsubscribe", req);
    }
}
