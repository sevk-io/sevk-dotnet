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

    public async Task<BulkUpdateContactsResponse> BulkUpdateAsync(List<BulkUpdateContactEntry> updates) =>
        await _client.PatchAsync<BulkUpdateContactsResponse>("/contacts/bulk-update", new { contacts = updates });

    public async Task<ImportContactsResponse> ImportAsync(ImportContactsRequest req) =>
        await _client.PostAsync<ImportContactsResponse>("/contacts/import", req);

    public async Task<ListResponse<ContactEvent>> GetEventsAsync(string id, ListParams? p = null)
    {
        var query = BuildQuery(p);
        return await _client.GetAsync<ListResponse<ContactEvent>>($"/contacts/{id}/events{query}");
    }

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

    public async Task RemoveContactAsync(string id, string contactId) =>
        await _client.DeleteAsync($"/audiences/{id}/contacts/{contactId}");

    public async Task DeleteAsync(string id) =>
        await _client.DeleteAsync($"/audiences/{id}");

    public async Task<ListResponse<Contact>> ListContactsAsync(string id, ListParams? p = null)
    {
        var query = p == null ? "" : $"?page={p.Page ?? 1}&limit={p.Limit ?? 20}";
        return await _client.GetAsync<ListResponse<Contact>>($"/audiences/{id}/contacts{query}");
    }
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

    public async Task<BroadcastStatus> GetStatusAsync(string id) =>
        await _client.GetAsync<BroadcastStatus>($"/broadcasts/{id}/status");

    public async Task<ListResponse<BroadcastEmail>> GetEmailsAsync(string id, ListParams? p = null)
    {
        var query = p == null ? "" : $"?page={p.Page ?? 1}&limit={p.Limit ?? 20}";
        return await _client.GetAsync<ListResponse<BroadcastEmail>>($"/broadcasts/{id}/emails{query}");
    }

    public async Task<BroadcastCostEstimate> EstimateCostAsync(string id) =>
        await _client.GetAsync<BroadcastCostEstimate>($"/broadcasts/{id}/estimate-cost");

    public async Task<ListResponse<Broadcast>> ListActiveAsync() =>
        await _client.GetAsync<ListResponse<Broadcast>>("/broadcasts/active");

    public async Task<Broadcast> CreateAsync(CreateBroadcastRequest req) =>
        await _client.PostAsync<Broadcast>("/broadcasts", req);

    public async Task<Broadcast> UpdateAsync(string id, UpdateBroadcastRequest req) =>
        await _client.PatchAsync<Broadcast>($"/broadcasts/{id}", req);

    public async Task DeleteAsync(string id) =>
        await _client.DeleteAsync($"/broadcasts/{id}");

    public async Task<Broadcast> SendAsync(string id) =>
        await _client.PostAsync<Broadcast>($"/broadcasts/{id}/send");

    public async Task<Broadcast> CancelAsync(string id) =>
        await _client.PostAsync<Broadcast>($"/broadcasts/{id}/cancel");

    public async Task<object> SendTestAsync(string id, List<string> emails) =>
        await _client.PostAsync<object>($"/broadcasts/{id}/test", new { emails });

    public async Task<object> GetAnalyticsAsync(string id) =>
        await _client.GetAsync<object>($"/broadcasts/{id}/analytics");
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

    public async Task<Domain> GetAsync(string id) =>
        await _client.GetAsync<Domain>($"/domains/{id}");

    public async Task<Domain> CreateAsync(CreateDomainRequest req) =>
        await _client.PostAsync<Domain>("/domains", req);

    public async Task<Domain> UpdateAsync(string id, UpdateDomainRequest req) =>
        await _client.PatchAsync<Domain>($"/domains/{id}", req);

    public async Task DeleteAsync(string id) =>
        await _client.DeleteAsync($"/domains/{id}");

    public async Task<Domain> VerifyAsync(string id) =>
        await _client.PostAsync<Domain>($"/domains/{id}/verify");

    public async Task<DnsRecordsResponse> GetDnsRecordsAsync(string id) =>
        await _client.GetAsync<DnsRecordsResponse>($"/domains/{id}/dns-records");

    public async Task<RegionsResponse> GetRegionsAsync() =>
        await _client.GetAsync<RegionsResponse>("/domains/regions");
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

    public async Task AddContactsAsync(string audienceId, string topicId, List<string> contactIds) =>
        await _client.PostAsync<object>($"/audiences/{audienceId}/topics/{topicId}/contacts", new { contactIds });

    public async Task RemoveContactAsync(string audienceId, string topicId, string contactId) =>
        await _client.DeleteAsync($"/audiences/{audienceId}/topics/{topicId}/contacts/{contactId}");

    public async Task<ListResponse<Contact>> ListContactsAsync(string audienceId, string topicId, ListParams? p = null)
    {
        var query = p == null ? "" : $"?page={p.Page ?? 1}&limit={p.Limit ?? 20}";
        return await _client.GetAsync<ListResponse<Contact>>($"/audiences/{audienceId}/topics/{topicId}/contacts{query}");
    }
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

    public async Task<SegmentCalculateResponse> CalculateAsync(string audienceId, string id) =>
        await _client.GetAsync<SegmentCalculateResponse>($"/audiences/{audienceId}/segments/{id}/calculate");

    public async Task<SegmentCalculateResponse> PreviewAsync(string audienceId, CreateSegmentRequest req) =>
        await _client.PostAsync<SegmentCalculateResponse>($"/audiences/{audienceId}/segments/preview", req);
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

    /// <summary>
    /// Send an email with optional attachments (max 10, 10MB total)
    /// </summary>
    public async Task<Email> SendAsync(SendEmailRequest req) =>
        await _client.PostAsync<Email>("emails", req);

    /// <summary>
    /// Send multiple emails in bulk (max 100)
    /// </summary>
    public async Task<BulkEmailResponse> SendBulkAsync(BulkEmailRequest req) =>
        await _client.PostAsync<BulkEmailResponse>("emails/bulk", req);

    public async Task<Email> GetAsync(string id) =>
        await _client.GetAsync<Email>($"emails/{id}");
}

public class WebhooksResource
{
    private readonly SevkClient _client;
    public WebhooksResource(SevkClient client) => _client = client;

    public async Task<ListResponse<Webhook>> ListAsync(ListParams? p = null)
    {
        var query = p == null ? "" : $"?page={p.Page ?? 1}&limit={p.Limit ?? 20}" + (p?.Search != null ? $"&search={p.Search}" : "");
        return await _client.GetAsync<ListResponse<Webhook>>($"/webhooks{query}");
    }

    public async Task<Webhook> GetAsync(string id) =>
        await _client.GetAsync<Webhook>($"/webhooks/{id}");

    public async Task<Webhook> CreateAsync(CreateWebhookRequest req) =>
        await _client.PostAsync<Webhook>("/webhooks", req);

    public async Task<Webhook> UpdateAsync(string id, UpdateWebhookRequest req) =>
        await _client.PatchAsync<Webhook>($"/webhooks/{id}", req);

    public async Task DeleteAsync(string id) =>
        await _client.DeleteAsync($"/webhooks/{id}");

    public async Task<WebhookTestResponse> TestAsync(string id) =>
        await _client.PostAsync<WebhookTestResponse>($"/webhooks/{id}/test");

    public async Task<WebhookEventsResponse> ListEventsAsync() =>
        await _client.GetAsync<WebhookEventsResponse>("/webhooks/events");
}

public class EventsResource
{
    private readonly SevkClient _client;
    public EventsResource(SevkClient client) => _client = client;

    public async Task<ListResponse<Event>> ListAsync(ListParams? p = null, string? type = null, string? from = null, string? to = null)
    {
        var query = p == null ? "" : $"?page={p.Page ?? 1}&limit={p.Limit ?? 20}";
        if (type != null) query += (query == "" ? "?" : "&") + $"type={type}";
        if (from != null) query += (query == "" ? "?" : "&") + $"from={from}";
        if (to != null) query += (query == "" ? "?" : "&") + $"to={to}";
        return await _client.GetAsync<ListResponse<Event>>($"/events{query}");
    }

    public async Task<EventStats> StatsAsync() =>
        await _client.GetAsync<EventStats>("/events/stats");
}
