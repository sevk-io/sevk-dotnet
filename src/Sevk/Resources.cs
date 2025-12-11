using Sevk.Types;

namespace Sevk.Resources;

public class ContactsResource
{
    private readonly SevkClient _client;
    public ContactsResource(SevkClient client) => _client = client;

    public async Task<ListResponse<Contact>> ListAsync(ListParams? p = null)
    {
        var query = BuildQuery(p);
        return await _client.GetAsync<ListResponse<Contact>>($"/contacts{query}");
    }

    public async Task<Contact> GetAsync(string id) =>
        await _client.GetAsync<Contact>($"/contacts/{id}");

    public async Task<Contact> CreateAsync(string email, CreateContactRequest? req = null)
    {
        var body = req ?? new CreateContactRequest();
        body.Email = email;
        return await _client.PostAsync<Contact>("/contacts", body);
    }

    public async Task<Contact> UpdateAsync(string id, UpdateContactRequest req) =>
        await _client.PatchAsync<Contact>($"/contacts/{id}", req);

    public async Task DeleteAsync(string id) =>
        await _client.DeleteAsync($"/contacts/{id}");

    private string BuildQuery(ListParams? p) =>
        p == null ? "" : $"?page={p.Page ?? 1}&limit={p.Limit ?? 20}" + (p.Search != null ? $"&search={p.Search}" : "");
}

public class AudiencesResource
{
    private readonly SevkClient _client;
    public AudiencesResource(SevkClient client) => _client = client;

    public async Task<ListResponse<Audience>> ListAsync(ListParams? p = null)
    {
        var query = p == null ? "" : $"?page={p.Page ?? 1}&limit={p.Limit ?? 20}";
        return await _client.GetAsync<ListResponse<Audience>>($"/audiences{query}");
    }

    public async Task<Audience> GetAsync(string id) =>
        await _client.GetAsync<Audience>($"/audiences/{id}");

    public async Task<Audience> CreateAsync(CreateAudienceRequest req) =>
        await _client.PostAsync<Audience>("/audiences", req);

    public async Task<Audience> UpdateAsync(string id, UpdateAudienceRequest req) =>
        await _client.PatchAsync<Audience>($"/audiences/{id}", req);

    public async Task AddContactsAsync(string id, List<string> contactIds) =>
        await _client.PostAsync<object>($"/audiences/{id}/contacts", new { contactIds });

    public async Task DeleteAsync(string id) =>
        await _client.DeleteAsync($"/audiences/{id}");
}

public class TemplatesResource
{
    private readonly SevkClient _client;
    public TemplatesResource(SevkClient client) => _client = client;

    public async Task<ListResponse<Template>> ListAsync(ListParams? p = null)
    {
        var query = p == null ? "" : $"?page={p.Page ?? 1}&limit={p.Limit ?? 20}";
        return await _client.GetAsync<ListResponse<Template>>($"/templates{query}");
    }

    public async Task<Template> GetAsync(string id) =>
        await _client.GetAsync<Template>($"/templates/{id}");

    public async Task<Template> CreateAsync(CreateTemplateRequest req) =>
        await _client.PostAsync<Template>("/templates", req);

    public async Task<Template> UpdateAsync(string id, UpdateTemplateRequest req) =>
        await _client.PatchAsync<Template>($"/templates/{id}", req);

    public async Task<Template> DuplicateAsync(string id) =>
        await _client.PostAsync<Template>($"/templates/{id}/duplicate");

    public async Task DeleteAsync(string id) =>
        await _client.DeleteAsync($"/templates/{id}");
}

public class BroadcastsResource
{
    private readonly SevkClient _client;
    public BroadcastsResource(SevkClient client) => _client = client;

    public async Task<ListResponse<Broadcast>> ListAsync(ListParams? p = null)
    {
        var query = p == null ? "" : $"?page={p.Page ?? 1}&limit={p.Limit ?? 20}" + (p?.Search != null ? $"&search={p.Search}" : "");
        return await _client.GetAsync<ListResponse<Broadcast>>($"/broadcasts{query}");
    }

    public async Task<Broadcast> GetAsync(string id) =>
        await _client.GetAsync<Broadcast>($"/broadcasts/{id}");
}

public class DomainsResource
{
    private readonly SevkClient _client;
    public DomainsResource(SevkClient client) => _client = client;

    public async Task<DomainsResponse> ListAsync(bool? verified = null)
    {
        var query = verified.HasValue ? $"?verified={verified.Value.ToString().ToLower()}" : "";
        return await _client.GetAsync<DomainsResponse>($"/domains{query}");
    }
}

public class TopicsResource
{
    private readonly SevkClient _client;
    public TopicsResource(SevkClient client) => _client = client;

    public async Task<ListResponse<Topic>> ListAsync(string audienceId, ListParams? p = null)
    {
        var query = p == null ? "" : $"?page={p.Page ?? 1}&limit={p.Limit ?? 20}";
        return await _client.GetAsync<ListResponse<Topic>>($"/audiences/{audienceId}/topics{query}");
    }

    public async Task<Topic> GetAsync(string audienceId, string id) =>
        await _client.GetAsync<Topic>($"/audiences/{audienceId}/topics/{id}");

    public async Task<Topic> CreateAsync(string audienceId, CreateTopicRequest req) =>
        await _client.PostAsync<Topic>($"/audiences/{audienceId}/topics", req);

    public async Task<Topic> UpdateAsync(string audienceId, string id, UpdateTopicRequest req) =>
        await _client.PatchAsync<Topic>($"/audiences/{audienceId}/topics/{id}", req);

    public async Task DeleteAsync(string audienceId, string id) =>
        await _client.DeleteAsync($"/audiences/{audienceId}/topics/{id}");
}

public class SegmentsResource
{
    private readonly SevkClient _client;
    public SegmentsResource(SevkClient client) => _client = client;

    public async Task<ListResponse<Segment>> ListAsync(string audienceId, ListParams? p = null)
    {
        var query = p == null ? "" : $"?page={p.Page ?? 1}&limit={p.Limit ?? 20}";
        return await _client.GetAsync<ListResponse<Segment>>($"/audiences/{audienceId}/segments{query}");
    }

    public async Task<Segment> GetAsync(string audienceId, string id) =>
        await _client.GetAsync<Segment>($"/audiences/{audienceId}/segments/{id}");

    public async Task<Segment> CreateAsync(string audienceId, CreateSegmentRequest req) =>
        await _client.PostAsync<Segment>($"/audiences/{audienceId}/segments", req);

    public async Task<Segment> UpdateAsync(string audienceId, string id, UpdateSegmentRequest req) =>
        await _client.PatchAsync<Segment>($"/audiences/{audienceId}/segments/{id}", req);

    public async Task DeleteAsync(string audienceId, string id) =>
        await _client.DeleteAsync($"/audiences/{audienceId}/segments/{id}");
}

public class SubscriptionsResource
{
    private readonly SevkClient _client;
    public SubscriptionsResource(SevkClient client) => _client = client;

    public async Task<SubscribeResponse> SubscribeAsync(SubscribeRequest req) =>
        await _client.PostAsync<SubscribeResponse>("/subscriptions/subscribe", req);

    public async Task UnsubscribeAsync(UnsubscribeRequest req) =>
        await _client.PostAsync<object>("/subscriptions/unsubscribe", req);
}

public class EmailsResource
{
    private readonly SevkClient _client;
    public EmailsResource(SevkClient client) => _client = client;

    public async Task<Email> SendAsync(SendEmailRequest req) =>
        await _client.PostAsync<Email>("emails", req);
}
