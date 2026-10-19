using Sevk.Types;

namespace Sevk.Resources;

public class UsageResource
{
    private readonly SevkClient _client;

    public UsageResource(SevkClient client) => _client = client;

    /// <summary>Returns current limits, resource counts, and email usage.</summary>
    public async Task<ProjectUsage> GetAsync() =>
        await _client.GetAsync<ProjectUsage>("/limits");
}
