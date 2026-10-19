using Sevk.Types;

namespace Sevk.Resources;

public class DomainsResource
{
    private readonly SevkClient _client;

    public DomainsResource(SevkClient client) => _client = client;

    /// <summary>Lists sending domains, optionally filtered by verification state.</summary>
    public async Task<DomainsResponse> ListAsync(ListDomainsParams? p = null)
    {
        var query = p is null
            ? ""
            : new QueryParameters()
                .AddPagination(p.Page, p.Limit, p.After, p.Before)
                .Add("verified", p.Verified)
                .Build();

        return await _client.GetAsync<DomainsResponse>($"/domains{query}");
    }
    public async Task<DomainsResponse> ListAsync(bool verified) =>
        await ListAsync(new ListDomainsParams { Verified = verified });

    /// <summary>Retrieves a domain and its current verification state.</summary>
    public async Task<DomainRead> GetAsync(string id) =>
        await _client.GetAsync<DomainRead>($"/domains/{id}");

    /// <summary>registers a domain; dns material requires domain:dns-read.</summary>
    public async Task<DomainRead> CreateAsync(CreateDomainRequest req) =>
        await _client.PostAsync<DomainRead>("/domains", req);

    /// <summary>Updates domain sending, tracking, TLS, or inbound settings.</summary>
    public async Task<DomainRead> UpdateAsync(string id, UpdateDomainRequest req) =>
        await _client.PutAsync<DomainRead>($"/domains/{id}", req);

    /// <summary>Deletes a domain after the API confirms it is safe to remove.</summary>
    public async Task<DeleteResponse> DeleteAsync(string id) =>
        await _client.DeleteAsync<DeleteResponse>($"/domains/{id}");

    /// <summary>Rechecks sending DNS records and returns the updated state.</summary>
    public async Task<DomainRead> VerifyAsync(string id) =>
        await _client.PostAsync<DomainRead>($"/domains/{id}/verify");

    /// <summary>Checks inbound MX records and returns diagnostic details.</summary>
    public async Task<InboundVerifyResult> VerifyInboundAsync(string id) =>
        await _client.PostAsync<InboundVerifyResult>($"/domains/{id}/inbound/verify");

    /// <summary>Returns DNS records required for sending and inbound mail.</summary>
    public async Task<DnsRecordsResponse> DnsRecordsAsync(string id) =>
        await _client.GetAsync<DnsRecordsResponse>($"/domains/{id}/dns-records");

    /// <summary>Lists region identifiers accepted when registering a domain.</summary>
    public async Task<RegionsResponse> RegionsAsync() =>
        await _client.GetAsync<RegionsResponse>("/domains/regions");
}
