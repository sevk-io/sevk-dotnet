using Sevk.Types;

namespace Sevk.Resources;

public class SegmentsResource
{
    private readonly SevkClient _client;

    public SegmentsResource(SevkClient client) => _client = client;

    /// <summary>Lists segments belonging to an audience with optional filtering.</summary>
    public async Task<ListResponse<Segment>> ListAsync(string audienceId, ListParams? p = null)
    {
        var query = "";

        if (p != null)
        {
            query = new QueryParameters()
                .AddPagination(p)
                .Build(includeQuestionMarkWhenEmpty: true);
        }

        return await _client.GetAsync<ListResponse<Segment>>($"/audiences/{audienceId}/segments{query}");
    }

    /// <summary>Creates a saved contact segment under an audience.</summary>
    public async Task<Segment> CreateAsync(string audienceId, CreateSegmentRequest req)
    {
        return await _client.PostAsync<Segment>($"/audiences/{audienceId}/segments", req);
    }

    /// <summary>Evaluates segment rules and returns matches without saving.</summary>
    public async Task<PreviewSegmentResponse> PreviewAsync(string audienceId, PreviewSegmentRequest req)
    {
        return await _client.PostAsync<PreviewSegmentResponse>($"/audiences/{audienceId}/segments/preview", req);
    }

    /// <summary>Retrieves a saved segment by id.</summary>
    public async Task<Segment> GetAsync(string id) =>
        await _client.GetAsync<Segment>($"/segments/{id}");

    /// <summary>Updates a segment while preserving omitted fields.</summary>
    public async Task<Segment> UpdateAsync(string id, UpdateSegmentRequest req)
    {
        return await _client.PutAsync<Segment>($"/segments/{id}", req);
    }

    /// <summary>Deletes a segment without deleting any contacts.</summary>
    public async Task<DeleteResponse> DeleteAsync(string id) =>
        await _client.DeleteAsync<DeleteResponse>($"/segments/{id}");

    /// <summary>Recalculates and returns the current matching contact count.</summary>
    public async Task<CalculateSegmentResponse> CalculateAsync(string id) =>
        await _client.GetAsync<CalculateSegmentResponse>($"/segments/{id}/calculate");
}
