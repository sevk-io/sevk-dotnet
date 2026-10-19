using Sevk.Types;
using Sevk.Markup;

namespace Sevk.Resources;

public class EmailsResource
{
    private readonly SevkClient _client;

    public EmailsResource(SevkClient client) => _client = client;

    /// <summary>Sends one transactional email. Attachments are limited to 10 files and 10 MB total.</summary>
    public async Task<SendEmailResponse> SendAsync(SendEmailRequest req) =>
        await SendAsync(req, null);

    /// <summary>Sends one transactional email with optional retry idempotency.</summary>
    public async Task<SendEmailResponse> SendAsync(SendEmailRequest req, SendOptions? options)
    {
        var wire = new SendEmailRequest
        {
            From = req.From,
            To = req.To,
            Subject = req.Subject,
            Html = !string.IsNullOrEmpty(req.Markup) ? Renderer.Render(req.Markup) : req.Html,
            Text = req.Text,
            ReplyTo = req.ReplyTo,
            Cc = req.Cc,
            Bcc = req.Bcc,
            Attachments = req.Attachments,
            Tracking = req.Tracking,
            Headers = req.Headers,
            ScheduledAt = req.ScheduledAt
        };

        return await _client.PostAsync<SendEmailResponse>("/emails", wire, IdempotencyHeaders(options));
    }

    /// <summary>Sends up to 100 transactional emails and returns per-item results.</summary>
    public async Task<BulkEmailResponse> SendBulkAsync(BulkEmailRequest req) =>
        await SendBulkAsync(req, null);

    /// <summary>Sends a batch with optional retry idempotency.</summary>
    public async Task<BulkEmailResponse> SendBulkAsync(BulkEmailRequest req, SendOptions? options)
    {
        if (req.Emails == null)
        {
            return await _client.PostAsync<BulkEmailResponse>("/emails/bulk", req, IdempotencyHeaders(options));
        }

        var wire = new BulkEmailRequest();

        for (var index = 0; index < req.Emails.Count; index++)
        {
            var email = req.Emails[index];
            if (email == null)
            {
                wire.Emails.Add(null!);
                continue;
            }

            var wireEmail = new BulkEmailItemRequest
            {
                From = email.From,
                To = email.To,
                Subject = email.Subject,
                Html = !string.IsNullOrEmpty(email.Markup) ? Renderer.Render(email.Markup) : email.Html,
                Text = email.Text,
                ReplyTo = email.ReplyTo,
                Cc = email.Cc,
                Bcc = email.Bcc,
                Attachments = email.Attachments,
                Tracking = email.Tracking,
                Headers = email.Headers
            };
            wire.Emails.Add(wireEmail);
        }

        return await _client.PostAsync<BulkEmailResponse>("/emails/bulk", wire, IdempotencyHeaders(options));
    }

    /// <summary>Retrieves a sent email and its current delivery state.</summary>
    public async Task<Email> GetAsync(string id) =>
        await _client.GetAsync<Email>($"/emails/{id}");

    /// <summary>moves an unattempted scheduled email to a new delivery time.</summary>
    public async Task<ScheduledEmailMutationResponse> UpdateAsync(string id, UpdateScheduledEmailRequest req) =>
        await _client.PatchAsync<ScheduledEmailMutationResponse>($"/emails/{id}", req);

    /// <summary>cancels an unattempted scheduled email.</summary>
    public async Task<ScheduledEmailMutationResponse> CancelAsync(string id) =>
        await _client.PostAsync<ScheduledEmailMutationResponse>($"/emails/{id}/cancel");

    private static IDictionary<string, string>? IdempotencyHeaders(SendOptions? options)
    {
        if (options == null || options.IdempotencyKey == null)
        {
            return null;
        }

        return new Dictionary<string, string> { { "Idempotency-Key", options.IdempotencyKey! } };
    }

}
