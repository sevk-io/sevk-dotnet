using Sevk.Types;

namespace Sevk.Resources;

public class ActivityResource
{
    private readonly SevkClient _client;

    public ActivityResource(SevkClient client) => _client = client;

    /// <summary>Lists project activity with optional pagination and filtering.</summary>
    public async Task<ListResponse<ActivityItem>> ListAsync(ListActivityParams? p = null)
    {
        if (p == null)
        {
            return await _client.GetAsync<ListResponse<ActivityItem>>("/activity");
        }

        var query = new QueryParameters()
            .AddPagination(p, includeSearch: false)
            .Add("category", p.Category)
            .Add("type", p.Type is null ? null : string.Join(",", p.Type.Select(value => value.ToString().ToUpperInvariant())))
            .Add("days", p.Days)
            .Add("search", p.Search)
            .Build(includeQuestionMarkWhenEmpty: true);

        return await _client.GetAsync<ListResponse<ActivityItem>>($"/activity{query}");
    }

    /// <summary>Returns aggregate counts and rates for recent project activity.</summary>
    public async Task<ActivityStats> StatsAsync() =>
        await _client.GetAsync<ActivityStats>("/activity/stats");
}
