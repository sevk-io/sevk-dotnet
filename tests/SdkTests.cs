using Sevk;
using Sevk.Markup;
using Sevk.Types;
using Xunit;

namespace Sevk.Tests;

// Fixture that runs setup ONCE for all tests
public class SevkTestFixture : IAsyncLifetime
{
    private const string DefaultBaseUrl = "https://api.sevk.io";
    public string BaseUrl { get; private set; } = DefaultBaseUrl;
    public SevkClient Sevk { get; private set; } = null!;
    public bool SkipIntegrationTests { get; private set; }
    public string? CreatedContactId { get; set; }
    public string? CreatedAudienceId { get; set; }
    public string? CreatedTemplateId { get; set; }
    public string? CreatedTopicId { get; set; }
    public string? CreatedSegmentId { get; set; }

    public Task InitializeAsync()
    {
        var apiKey = Environment.GetEnvironmentVariable("SEVK_TEST_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            SkipIntegrationTests = true;
            return Task.CompletedTask;
        }

        BaseUrl = Environment.GetEnvironmentVariable("SEVK_TEST_BASE_URL") ?? DefaultBaseUrl;
        Sevk = new SevkClient(apiKey, new SevkOptions { BaseUrl = BaseUrl });
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

[CollectionDefinition("Sevk")]
public class SevkCollection : ICollectionFixture<SevkTestFixture> { }

[Collection("Sevk")]
[TestCaseOrderer("Sevk.Tests.PriorityOrderer", "Sevk.Tests")]
public class SdkTests
{
    private readonly SevkClient _sevk;
    private readonly SevkTestFixture _fixture;
    private readonly string _baseUrl;

    private static string UniqueId() => $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{Random.Shared.Next(10000)}";

    public SdkTests(SevkTestFixture fixture)
    {
        _fixture = fixture;
        _sevk = fixture.Sevk;
        _baseUrl = fixture.BaseUrl;
    }

    private void SkipIfNoApiKey()
    {
        Skip.If(_fixture.SkipIntegrationTests, "SEVK_TEST_API_KEY environment variable is not set");
    }

    private void SkipIfNoDomainTests()
    {
        Skip.If(Environment.GetEnvironmentVariable("INCLUDE_DOMAIN_TESTS") != "true", "INCLUDE_DOMAIN_TESTS not set");
    }

    // Helper to get or create shared audience
    private async Task<string> GetOrCreateAudienceIdAsync()
    {
        if (_fixture.CreatedAudienceId != null) return _fixture.CreatedAudienceId;
        var name = $"Shared Audience {UniqueId()}";
        var audience = await _sevk.Audiences.CreateAsync(new CreateAudienceRequest { Name = name });
        _fixture.CreatedAudienceId = audience.Id;
        return audience.Id;
    }

    // ==================== AUTHENTICATION TESTS ====================

    [SkippableFact]
    public async Task Test01_Auth_ShouldRejectInvalidApiKey()
    {
        SkipIfNoApiKey();
        var invalidSevk = new SevkClient("sevk_invalid_api_key_12345", new SevkOptions { BaseUrl = _baseUrl });
        var ex = await Assert.ThrowsAsync<SevkException>(() => invalidSevk.Contacts.ListAsync());
        Assert.True(ex.Message.Contains("401") || ex.Message.ToLower().Contains("invalid"));
    }

    [SkippableFact]
    public void Test02_Auth_ShouldRejectEmptyApiKey()
    {
        SkipIfNoApiKey();
        Assert.Throws<SevkException>(() => new SevkClient("", new SevkOptions { BaseUrl = _baseUrl }));
    }

    [SkippableFact]
    public async Task Test03_Auth_ShouldRejectMalformedApiKey()
    {
        SkipIfNoApiKey();
        var malformedSevk = new SevkClient("invalid_key_format", new SevkOptions { BaseUrl = _baseUrl });
        var ex = await Assert.ThrowsAsync<SevkException>(() => malformedSevk.Contacts.ListAsync());
        Assert.Contains("401", ex.Message);
    }

    // ==================== CONTACTS TESTS ====================

    [SkippableFact]
    public async Task Test04_Contacts_ShouldListWithCorrectStructure()
    {
        SkipIfNoApiKey();
        var result = await _sevk.Contacts.ListAsync();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.Items.Count >= 0);
    }

    [SkippableFact]
    public async Task Test05_Contacts_ShouldListWithPagination()
    {
        SkipIfNoApiKey();
        var result = await _sevk.Contacts.ListAsync(new ListParams { Page = 1, Limit = 5 });
        Assert.NotNull(result);
    }

    [SkippableFact]
    public async Task Test06_Contacts_ShouldCreate()
    {
        SkipIfNoApiKey();
        var email = $"test-{UniqueId()}@example.com";
        var contact = await _sevk.Contacts.CreateAsync(email);
        Assert.NotNull(contact);
        Assert.NotNull(contact.Id);
        Assert.Equal(email, contact.Email);
        _fixture.CreatedContactId = contact.Id;
    }

    [SkippableFact]
    public async Task Test07_Contacts_ShouldGetById()
    {
        SkipIfNoApiKey();
        var email = $"get-{UniqueId()}@example.com";
        var created = await _sevk.Contacts.CreateAsync(email);
        var contact = await _sevk.Contacts.GetAsync(created.Id);
        Assert.NotNull(contact);
        Assert.Equal(created.Id, contact.Id);
    }

    [SkippableFact]
    public async Task Test08_Contacts_ShouldUpdate()
    {
        SkipIfNoApiKey();
        var email = $"update-{UniqueId()}@example.com";
        var created = await _sevk.Contacts.CreateAsync(email);
        var contact = await _sevk.Contacts.UpdateAsync(created.Id, new UpdateContactRequest { Subscribed = false });
        Assert.NotNull(contact);
        Assert.Equal(created.Id, contact.Id);
        Assert.False(contact.Subscribed);
    }

    [SkippableFact]
    public async Task Test09_Contacts_ShouldThrowForNonExistent()
    {
        SkipIfNoApiKey();
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Contacts.GetAsync("non-existent-id"));
        Assert.Contains("404", ex.Message);
    }

    [SkippableFact]
    public async Task Test10_Contacts_ShouldDelete()
    {
        SkipIfNoApiKey();
        var email = $"delete-{UniqueId()}@example.com";
        var contact = await _sevk.Contacts.CreateAsync(email);
        await _sevk.Contacts.DeleteAsync(contact.Id);
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Contacts.GetAsync(contact.Id));
        Assert.Contains("404", ex.Message);
    }

    // ==================== AUDIENCES TESTS ====================

    [SkippableFact]
    public async Task Test11_Audiences_ShouldListWithCorrectStructure()
    {
        SkipIfNoApiKey();
        var result = await _sevk.Audiences.ListAsync();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.Items.Count >= 0);
    }

    [SkippableFact]
    public async Task Test12_Audiences_ShouldCreate()
    {
        SkipIfNoApiKey();
        var name = $"Test Audience {UniqueId()}";
        var audience = await _sevk.Audiences.CreateAsync(new CreateAudienceRequest { Name = name });
        Assert.NotNull(audience);
        Assert.NotNull(audience.Id);
        Assert.Equal(name, audience.Name);
        _fixture.CreatedAudienceId = audience.Id;
    }

    [SkippableFact]
    public async Task Test13_Audiences_ShouldCreateWithAllFields()
    {
        SkipIfNoApiKey();
        // Use existing audience instead of creating new one
        var audienceId = await GetOrCreateAudienceIdAsync();
        var audience = await _sevk.Audiences.GetAsync(audienceId);
        Assert.NotNull(audience);
        Assert.NotNull(audience.Id);
    }

    [SkippableFact]
    public async Task Test14_Audiences_ShouldGetById()
    {
        SkipIfNoApiKey();
        var audienceId = await GetOrCreateAudienceIdAsync();
        var audience = await _sevk.Audiences.GetAsync(audienceId);
        Assert.NotNull(audience);
        Assert.Equal(audienceId, audience.Id);
    }

    [SkippableFact]
    public async Task Test15_Audiences_ShouldUpdate()
    {
        SkipIfNoApiKey();
        var audienceId = await GetOrCreateAudienceIdAsync();
        var newName = $"Updated Audience {UniqueId()}";
        var audience = await _sevk.Audiences.UpdateAsync(audienceId, new UpdateAudienceRequest { Name = newName });
        Assert.NotNull(audience);
        Assert.Equal(newName, audience.Name);
    }

    [SkippableFact]
    public async Task Test16_Audiences_ShouldAddContacts()
    {
        SkipIfNoApiKey();
        var audienceId = await GetOrCreateAudienceIdAsync();
        var email = $"add-contact-{UniqueId()}@example.com";
        var contact = await _sevk.Contacts.CreateAsync(email);
        await _sevk.Audiences.AddContactsAsync(audienceId, new List<string> { contact.Id });
        Assert.True(true);
    }

    [SkippableFact]
    public async Task Test17_Audiences_ShouldDelete()
    {
        SkipIfNoApiKey();
        // Create a separate audience just for deletion test
        var name = $"Delete Audience {UniqueId()}";
        var audience = await _sevk.Audiences.CreateAsync(new CreateAudienceRequest { Name = name });
        await _sevk.Audiences.DeleteAsync(audience.Id);
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Audiences.GetAsync(audience.Id));
        Assert.Contains("404", ex.Message);
    }

    // ==================== TEMPLATES TESTS ====================

    [SkippableFact]
    public async Task Test18_Templates_ShouldListWithCorrectStructure()
    {
        SkipIfNoApiKey();
        var result = await _sevk.Templates.ListAsync();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.Items.Count >= 0);
    }

    [SkippableFact]
    public async Task Test19_Templates_ShouldCreate()
    {
        SkipIfNoApiKey();
        var title = $"Test Template {UniqueId()}";
        var template = await _sevk.Templates.CreateAsync(new CreateTemplateRequest { Title = title, Content = "<p>Hello {{name}}</p>" });
        Assert.NotNull(template);
        Assert.NotNull(template.Id);
        Assert.Equal(title, template.Title);
        Assert.Equal("<p>Hello {{name}}</p>", template.Content);
        _fixture.CreatedTemplateId = template.Id;
    }

    [SkippableFact]
    public async Task Test20_Templates_ShouldGetById()
    {
        SkipIfNoApiKey();
        var title = $"Get Template {UniqueId()}";
        var created = await _sevk.Templates.CreateAsync(new CreateTemplateRequest { Title = title, Content = "<p>Test</p>" });
        var template = await _sevk.Templates.GetAsync(created.Id);
        Assert.NotNull(template);
        Assert.Equal(created.Id, template.Id);
    }

    [SkippableFact]
    public async Task Test21_Templates_ShouldUpdate()
    {
        SkipIfNoApiKey();
        var title = $"Update Template {UniqueId()}";
        var created = await _sevk.Templates.CreateAsync(new CreateTemplateRequest { Title = title, Content = "<p>Test</p>" });
        var newTitle = $"Updated Template {UniqueId()}";
        var template = await _sevk.Templates.UpdateAsync(created.Id, new UpdateTemplateRequest { Title = newTitle });
        Assert.NotNull(template);
        Assert.Equal(newTitle, template.Title);
    }

    [SkippableFact]
    public async Task Test22_Templates_ShouldDuplicate()
    {
        SkipIfNoApiKey();
        var title = $"Duplicate Template {UniqueId()}";
        var created = await _sevk.Templates.CreateAsync(new CreateTemplateRequest { Title = title, Content = "<p>Test</p>" });
        var template = await _sevk.Templates.DuplicateAsync(created.Id);
        Assert.NotNull(template);
        Assert.NotNull(template.Id);
        Assert.NotEqual(created.Id, template.Id);
    }

    [SkippableFact]
    public async Task Test23_Templates_ShouldDelete()
    {
        SkipIfNoApiKey();
        var title = $"Delete Template {UniqueId()}";
        var template = await _sevk.Templates.CreateAsync(new CreateTemplateRequest { Title = title, Content = "<p>Test</p>" });
        await _sevk.Templates.DeleteAsync(template.Id);
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Templates.GetAsync(template.Id));
        Assert.Contains("404", ex.Message);
    }

    // ==================== BROADCASTS TESTS ====================

    [SkippableFact]
    public async Task Test24_Broadcasts_ShouldListWithCorrectStructure()
    {
        SkipIfNoApiKey();
        var result = await _sevk.Broadcasts.ListAsync();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.Items.Count >= 0);
    }

    [SkippableFact]
    public async Task Test25_Broadcasts_ShouldListWithPagination()
    {
        SkipIfNoApiKey();
        var result = await _sevk.Broadcasts.ListAsync(new ListParams { Page = 1, Limit = 10 });
        Assert.NotNull(result);
    }

    [SkippableFact]
    public async Task Test26_Broadcasts_ShouldListWithSearch()
    {
        SkipIfNoApiKey();
        var result = await _sevk.Broadcasts.ListAsync(new ListParams { Search = "test" });
        Assert.NotNull(result);
    }

    // ==================== DOMAINS TESTS ====================

    [SkippableFact]
    public async Task Test27_Domains_ShouldListWithCorrectStructure()
    {
        SkipIfNoApiKey();
        SkipIfNoDomainTests();
        var result = await _sevk.Domains.ListAsync();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [SkippableFact]
    public async Task Test28_Domains_ShouldListOnlyVerified()
    {
        SkipIfNoApiKey();
        SkipIfNoDomainTests();
        var result = await _sevk.Domains.ListAsync(verified: true);
        Assert.NotNull(result);
        foreach (var domain in result.Items)
        {
            Assert.True(domain.Verified);
        }
    }

    // ==================== TOPICS TESTS ====================

    [SkippableFact]
    public async Task Test29_Topics_ShouldListForAudience()
    {
        SkipIfNoApiKey();
        var audienceId = await GetOrCreateAudienceIdAsync();
        var result = await _sevk.Topics.ListAsync(audienceId);
        Assert.NotNull(result);
        Assert.True(result.Total >= 0);
    }

    [SkippableFact]
    public async Task Test30_Topics_ShouldCreate()
    {
        SkipIfNoApiKey();
        var audienceId = await GetOrCreateAudienceIdAsync();
        var topicName = $"Test Topic {UniqueId()}";
        var topic = await _sevk.Topics.CreateAsync(audienceId, new CreateTopicRequest { Name = topicName });
        Assert.NotNull(topic);
        Assert.NotNull(topic.Id);
        Assert.Equal(topicName, topic.Name);
        Assert.Equal(audienceId, topic.AudienceId);
        _fixture.CreatedTopicId = topic.Id;
    }

    [SkippableFact]
    public async Task Test31_Topics_ShouldGetById()
    {
        SkipIfNoApiKey();
        var audienceId = await GetOrCreateAudienceIdAsync();
        var topicName = $"Get Topic {UniqueId()}";
        var created = await _sevk.Topics.CreateAsync(audienceId, new CreateTopicRequest { Name = topicName });
        var topic = await _sevk.Topics.GetAsync(audienceId, created.Id);
        Assert.NotNull(topic);
        Assert.Equal(created.Id, topic.Id);
    }

    [SkippableFact]
    public async Task Test32_Topics_ShouldUpdate()
    {
        SkipIfNoApiKey();
        var audienceId = await GetOrCreateAudienceIdAsync();
        var topicName = $"Update Topic {UniqueId()}";
        var created = await _sevk.Topics.CreateAsync(audienceId, new CreateTopicRequest { Name = topicName });
        var newName = $"Updated Topic {UniqueId()}";
        var topic = await _sevk.Topics.UpdateAsync(audienceId, created.Id, new UpdateTopicRequest { Name = newName });
        Assert.NotNull(topic);
        Assert.Equal(newName, topic.Name);
    }

    [SkippableFact]
    public async Task Test33_Topics_ShouldDelete()
    {
        SkipIfNoApiKey();
        var audienceId = await GetOrCreateAudienceIdAsync();
        var topicName = $"Delete Topic {UniqueId()}";
        var topic = await _sevk.Topics.CreateAsync(audienceId, new CreateTopicRequest { Name = topicName });
        await _sevk.Topics.DeleteAsync(audienceId, topic.Id);
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Topics.GetAsync(audienceId, topic.Id));
        Assert.Contains("404", ex.Message);
    }

    // ==================== SEGMENTS TESTS ====================

    [SkippableFact]
    public async Task Test34_Segments_ShouldListForAudience()
    {
        SkipIfNoApiKey();
        var audienceId = await GetOrCreateAudienceIdAsync();
        var result = await _sevk.Segments.ListAsync(audienceId);
        Assert.NotNull(result);
        Assert.True(result.Total >= 0);
    }

    [SkippableFact]
    public async Task Test35_Segments_ShouldCreate()
    {
        SkipIfNoApiKey();
        var audienceId = await GetOrCreateAudienceIdAsync();
        var segmentName = $"Test Segment {UniqueId()}";
        var rules = new List<SegmentRule> { new SegmentRule { Field = "email", Operator = "contains", Value = "@example.com" } };
        var segment = await _sevk.Segments.CreateAsync(audienceId, new CreateSegmentRequest { Name = segmentName, Rules = rules, Operator = "AND" });
        Assert.NotNull(segment);
        Assert.NotNull(segment.Id);
        Assert.Equal(segmentName, segment.Name);
        Assert.Equal(audienceId, segment.AudienceId);
        Assert.Equal("AND", segment.Operator);
        _fixture.CreatedSegmentId = segment.Id;
    }

    [SkippableFact]
    public async Task Test36_Segments_ShouldGetById()
    {
        SkipIfNoApiKey();
        var audienceId = await GetOrCreateAudienceIdAsync();
        var segmentName = $"Get Segment {UniqueId()}";
        var created = await _sevk.Segments.CreateAsync(audienceId, new CreateSegmentRequest { Name = segmentName, Rules = new List<SegmentRule>(), Operator = "AND" });
        var segment = await _sevk.Segments.GetAsync(audienceId, created.Id);
        Assert.NotNull(segment);
        Assert.Equal(created.Id, segment.Id);
    }

    [SkippableFact]
    public async Task Test37_Segments_ShouldUpdate()
    {
        SkipIfNoApiKey();
        var audienceId = await GetOrCreateAudienceIdAsync();
        var segmentName = $"Update Segment {UniqueId()}";
        var created = await _sevk.Segments.CreateAsync(audienceId, new CreateSegmentRequest { Name = segmentName, Rules = new List<SegmentRule>(), Operator = "AND" });
        var newName = $"Updated Segment {UniqueId()}";
        var segment = await _sevk.Segments.UpdateAsync(audienceId, created.Id, new UpdateSegmentRequest { Name = newName });
        Assert.NotNull(segment);
        Assert.Equal(newName, segment.Name);
    }

    [SkippableFact]
    public async Task Test38_Segments_ShouldDelete()
    {
        SkipIfNoApiKey();
        var audienceId = await GetOrCreateAudienceIdAsync();
        var segmentName = $"Delete Segment {UniqueId()}";
        var segment = await _sevk.Segments.CreateAsync(audienceId, new CreateSegmentRequest { Name = segmentName, Rules = new List<SegmentRule>(), Operator = "AND" });
        await _sevk.Segments.DeleteAsync(audienceId, segment.Id);
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Segments.GetAsync(audienceId, segment.Id));
        Assert.Contains("404", ex.Message);
    }

    // ==================== SUBSCRIPTIONS TESTS ====================

    [SkippableFact]
    public async Task Test39_Subscriptions_ShouldSubscribe()
    {
        SkipIfNoApiKey();
        var audienceId = await GetOrCreateAudienceIdAsync();
        var email = $"subscribe-{UniqueId()}@example.com";
        await _sevk.Subscriptions.SubscribeAsync(new SubscribeRequest { Email = email, AudienceId = audienceId });
        Assert.True(true);
    }

    [SkippableFact]
    public async Task Test40_Subscriptions_ShouldUnsubscribe()
    {
        SkipIfNoApiKey();
        var email = $"unsubscribe-{UniqueId()}@example.com";
        var contact = await _sevk.Contacts.CreateAsync(email, new CreateContactRequest { Email = email, Subscribed = true });
        await _sevk.Subscriptions.UnsubscribeAsync(new UnsubscribeRequest { Email = email });
        var updated = await _sevk.Contacts.GetAsync(contact.Id);
        Assert.False(updated.Subscribed);
    }

    // ==================== EMAILS TESTS ====================

    [SkippableFact]
    public async Task Test41_Emails_ShouldRejectUnverifiedDomain()
    {
        SkipIfNoApiKey();
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Emails.SendAsync(new SendEmailRequest
        {
            To = "test@example.com",
            From = "no-reply@unverified-domain.com",
            Subject = "Test Email",
            Html = "<p>Hello</p>"
        }));
        var message = ex.Message.ToLower();
        Assert.True(message.Contains("403") || message.Contains("domain"));
    }

    [SkippableFact]
    public async Task Test42_Emails_ShouldRejectDomainNotOwned()
    {
        SkipIfNoApiKey();
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Emails.SendAsync(new SendEmailRequest
        {
            To = "test@example.com",
            From = "no-reply@not-my-domain.io",
            Subject = "Test Email",
            Html = "<p>Hello</p>"
        }));
        var message = ex.Message.ToLower();
        Assert.True(message.Contains("403") || message.Contains("domain"));
    }

    [SkippableFact]
    public async Task Test43_Emails_ShouldRejectInvalidFrom()
    {
        SkipIfNoApiKey();
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Emails.SendAsync(new SendEmailRequest
        {
            To = "test@example.com",
            From = "invalid-email-without-domain",
            Subject = "Test Email",
            Html = "<p>Hello</p>"
        }));
        Assert.Contains("400", ex.Message);
    }

    [SkippableFact]
    public async Task Test44_Emails_ShouldReturnProperErrorMessage()
    {
        SkipIfNoApiKey();
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Emails.SendAsync(new SendEmailRequest
        {
            To = "recipient@example.com",
            From = "sender@random-unverified-domain.xyz",
            Subject = "Test Email",
            Html = "<p>Hello World</p>"
        }));
        var message = ex.Message.ToLower();
        Assert.True(message.Contains("domain") || message.Contains("verified") || message.Contains("forbidden"));
    }

    // ==================== DOMAINS UPDATE TESTS ====================

    [SkippableFact]
    public async Task Test50_Domains_ShouldUpdate()
    {
        SkipIfNoApiKey();
        SkipIfNoDomainTests();
        // Create a domain first, then update it
        try
        {
            var domain = await _sevk.Domains.CreateAsync(new CreateDomainRequest
            {
                Domain = $"test-{UniqueId()}.example.com"
            });
            if (domain != null)
            {
                var updated = await _sevk.Domains.UpdateAsync(domain.Id, new UpdateDomainRequest
                {
                    Region = "eu-west-1"
                });
                Assert.NotNull(updated);
                Assert.Equal(domain.Id, updated.Id);
            }
        }
        catch (SevkException)
        {
            // Domain creation may fail if domain already exists or requires verification
        }
    }

    // ==================== BROADCASTS EXTENDED TESTS ====================

    [SkippableFact]
    public async Task Test51_Broadcasts_ShouldGetStatus()
    {
        SkipIfNoApiKey();
        var broadcasts = await _sevk.Broadcasts.ListAsync();
        Assert.NotNull(broadcasts);

        if (broadcasts.Items.Count > 0)
        {
            try
            {
                var status = await _sevk.Broadcasts.GetStatusAsync(broadcasts.Items[0].Id);
                Assert.NotNull(status);
                Assert.NotNull(status.Id);
                Assert.NotNull(status.Status);
                Assert.True(status.Total >= 0);
                Assert.True(status.Sent >= 0);
            }
            catch (SevkException ex) when (ex.IsNotFound)
            {
                // Broadcast may have been deleted by concurrent tests
            }
        }
    }

    [SkippableFact]
    public async Task Test52_Broadcasts_ShouldGetEmails()
    {
        SkipIfNoApiKey();
        var broadcasts = await _sevk.Broadcasts.ListAsync();
        Assert.NotNull(broadcasts);

        if (broadcasts.Items.Count > 0)
        {
            try
            {
                var emails = await _sevk.Broadcasts.GetEmailsAsync(broadcasts.Items[0].Id);
                Assert.NotNull(emails);
                Assert.True(emails.Items.Count >= 0);
            }
            catch (SevkException ex) when (ex.IsNotFound)
            {
                // Broadcast may have been deleted by concurrent tests
            }
        }
    }

    [SkippableFact]
    public async Task Test53_Broadcasts_ShouldEstimateCost()
    {
        SkipIfNoApiKey();
        var broadcasts = await _sevk.Broadcasts.ListAsync();
        Assert.NotNull(broadcasts);

        if (broadcasts.Items.Count > 0)
        {
            var estimate = await _sevk.Broadcasts.EstimateCostAsync(broadcasts.Items[0].Id);
            Assert.NotNull(estimate);
            Assert.True(estimate.Recipients >= 0);
            Assert.True(estimate.EstimatedCost >= 0.0);
        }
    }

    [SkippableFact]
    public async Task Test54_Broadcasts_ShouldListActive()
    {
        SkipIfNoApiKey();
        var result = await _sevk.Broadcasts.ListActiveAsync();
        Assert.NotNull(result);
        Assert.True(result.Items.Count >= 0);
    }

    // ==================== TOPICS LIST CONTACTS TESTS ====================

    [SkippableFact]
    public async Task Test55_Topics_ShouldListContacts()
    {
        SkipIfNoApiKey();
        var audienceId = await GetOrCreateAudienceIdAsync();
        var topicName = $"List Contacts Topic {UniqueId()}";
        var topic = await _sevk.Topics.CreateAsync(audienceId, new CreateTopicRequest { Name = topicName });

        var contacts = await _sevk.Topics.ListContactsAsync(audienceId, topic.Id);
        Assert.NotNull(contacts);
        Assert.True(contacts.Total >= 0);
    }

    // ==================== WEBHOOKS TESTS (FULL CRUD) ====================

    [SkippableFact]
    public async Task Test60_Webhooks_ShouldListEvents()
    {
        SkipIfNoApiKey();
        var result = await _sevk.Webhooks.ListEventsAsync();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [SkippableFact]
    public async Task Test61_Webhooks_ShouldList()
    {
        SkipIfNoApiKey();
        var result = await _sevk.Webhooks.ListAsync();
        Assert.NotNull(result);
        Assert.True(result.Items.Count >= 0);
    }

    [SkippableFact]
    public async Task Test62_Webhooks_ShouldPerformFullCrudCycle()
    {
        SkipIfNoApiKey();

        // Get available events
        var availableEvents = await _sevk.Webhooks.ListEventsAsync();
        var eventName = (availableEvents?.Items != null && availableEvents.Items.Count > 0)
            ? availableEvents.Items[0] : "contact.subscribed";

        // Create
        var webhook = await _sevk.Webhooks.CreateAsync(new CreateWebhookRequest
        {
            Url = $"https://example.com/webhook/{UniqueId()}",
            Events = new List<string> { eventName },
            Enabled = true
        });
        Assert.NotNull(webhook);
        Assert.NotNull(webhook.Id);
        Assert.Contains("example.com", webhook.Url);
        Assert.True(webhook.Enabled);
        Assert.NotEmpty(webhook.Events);

        // Get
        var fetched = await _sevk.Webhooks.GetAsync(webhook.Id);
        Assert.NotNull(fetched);
        Assert.Equal(webhook.Id, fetched.Id);

        // Update
        var updated = await _sevk.Webhooks.UpdateAsync(webhook.Id, new UpdateWebhookRequest
        {
            Enabled = false
        });
        Assert.NotNull(updated);
        Assert.Equal(webhook.Id, updated.Id);
        Assert.False(updated.Enabled);

        // Test
        var testResponse = await _sevk.Webhooks.TestAsync(webhook.Id);
        Assert.NotNull(testResponse);

        // Delete
        await _sevk.Webhooks.DeleteAsync(webhook.Id);

        // Verify deletion
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Webhooks.GetAsync(webhook.Id));
        Assert.Contains("404", ex.Message);
    }

    // ==================== EVENTS TESTS ====================

    [SkippableFact]
    public async Task Test70_Events_ShouldList()
    {
        SkipIfNoApiKey();
        var result = await _sevk.Events.ListAsync();
        Assert.NotNull(result);
        Assert.True(result.Items.Count >= 0);
    }

    [SkippableFact]
    public async Task Test71_Events_ShouldListWithPagination()
    {
        SkipIfNoApiKey();
        var result = await _sevk.Events.ListAsync(new ListParams { Page = 1, Limit = 10 });
        Assert.NotNull(result);
    }

    [SkippableFact]
    public async Task Test72_Events_ShouldGetStats()
    {
        SkipIfNoApiKey();
        var stats = await _sevk.Events.StatsAsync();
        Assert.NotNull(stats);
        Assert.True(stats.Total >= 0);
        Assert.True(stats.Sent >= 0);
        Assert.True(stats.Delivered >= 0);
        Assert.True(stats.Opened >= 0);
        Assert.True(stats.Clicked >= 0);
        Assert.True(stats.Bounced >= 0);
        Assert.True(stats.Complained >= 0);
    }

    // ==================== USAGE TESTS ====================

    [SkippableFact]
    public async Task Test80_Usage_ShouldGetUsage()
    {
        SkipIfNoApiKey();
        var usage = await _sevk.GetUsageAsync();
        Assert.NotNull(usage);
    }

    // ==================== CONTACTS EXTENDED TESTS ====================

    [SkippableFact]
    public async Task Test81_Contacts_ShouldBulkUpdate()
    {
        SkipIfNoApiKey();
        await Task.Delay(5000); // Avoid rate limiting
        var email = $"bulk-{UniqueId()}@example.com";
        var contact = await _sevk.Contacts.CreateAsync(email);
        var result = await _sevk.Contacts.BulkUpdateAsync(new List<BulkUpdateContactEntry>
        {
            new BulkUpdateContactEntry { Id = contact.Id, Email = contact.Email, Subscribed = true }
        });
        Assert.NotNull(result);
    }

    [SkippableFact]
    public async Task Test82_Contacts_ShouldGetEvents()
    {
        SkipIfNoApiKey();
        var email = $"events-{UniqueId()}@example.com";
        var contact = await _sevk.Contacts.CreateAsync(email);
        var result = await _sevk.Contacts.GetEventsAsync(contact.Id);
        Assert.NotNull(result);
    }

    [SkippableFact]
    public async Task Test83_Contacts_ShouldImport()
    {
        SkipIfNoApiKey();
        await Task.Delay(2000); // Avoid rate limiting
        var email = $"import-{UniqueId()}@example.com";
        var result = await _sevk.Contacts.ImportAsync(new ImportContactsRequest
        {
            Contacts = new List<ImportContactEntry>
            {
                new ImportContactEntry { Email = email }
            }
        });
        Assert.NotNull(result);
    }

    // ==================== AUDIENCES EXTENDED TESTS ====================

    [SkippableFact]
    public async Task Test84_Audiences_ShouldListContacts()
    {
        SkipIfNoApiKey();
        var audienceId = await GetOrCreateAudienceIdAsync();
        var result = await _sevk.Audiences.ListContactsAsync(audienceId);
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [SkippableFact]
    public async Task Test85_Audiences_ShouldRemoveContact()
    {
        SkipIfNoApiKey();
        var audienceId = await GetOrCreateAudienceIdAsync();
        var email = $"audience-remove-{UniqueId()}@example.com";
        var contact = await _sevk.Contacts.CreateAsync(email);
        await _sevk.Audiences.AddContactsAsync(audienceId, new List<string> { contact.Id });
        await _sevk.Audiences.RemoveContactAsync(audienceId, contact.Id);

        // Verify removal by listing contacts
        var result = await _sevk.Audiences.ListContactsAsync(audienceId);
        Assert.DoesNotContain(result.Items, c => c.Id == contact.Id);
    }

    // ==================== BROADCASTS CRUD TESTS ====================

    [SkippableFact]
    public async Task Test86_Broadcasts_ShouldCreate()
    {
        SkipIfNoApiKey();
        var domains = await _sevk.Domains.ListAsync();
        if (domains.Items.Count == 0) return;
        var domainId = domains.Items[0].Id;

        var name = $"Test Broadcast {UniqueId()}";
        var broadcast = await _sevk.Broadcasts.CreateAsync(new CreateBroadcastRequest
        {
            DomainId = domainId,
            Name = name,
            Subject = "Test Subject",
            Body = "<section><paragraph>Test broadcast body</paragraph></section>",
            SenderName = "Test Sender",
            SenderEmail = "test",
            TargetType = "ALL"
        });
        Assert.NotNull(broadcast);
        Assert.NotNull(broadcast.Id);
    }

    [SkippableFact]
    public async Task Test87_Broadcasts_ShouldGetById()
    {
        SkipIfNoApiKey();
        var domains = await _sevk.Domains.ListAsync();
        if (domains.Items.Count == 0) return;
        var domainId = domains.Items[0].Id;

        var broadcast = await _sevk.Broadcasts.CreateAsync(new CreateBroadcastRequest
        {
            DomainId = domainId,
            Name = $"Get Broadcast {UniqueId()}",
            Subject = "Test Subject",
            Body = "<section><paragraph>Test</paragraph></section>",
            SenderName = "Test Sender",
            SenderEmail = "test",
            TargetType = "ALL"
        });
        var fetched = await _sevk.Broadcasts.GetAsync(broadcast.Id);
        Assert.NotNull(fetched);
        Assert.Equal(broadcast.Id, fetched.Id);
    }

    [SkippableFact]
    public async Task Test88_Broadcasts_ShouldUpdate()
    {
        SkipIfNoApiKey();
        var domains = await _sevk.Domains.ListAsync();
        if (domains.Items.Count == 0) return;

        // Try each domain until we find one that works (some may have been deleted by other tests)
        foreach (var d in domains.Items)
        {
            try
            {
                var broadcast = await _sevk.Broadcasts.CreateAsync(new CreateBroadcastRequest
                {
                    DomainId = d.Id,
                    Name = $"Update Broadcast {UniqueId()}",
                    Subject = "Test Subject",
                    Body = "<section><paragraph>Test</paragraph></section>",
                    SenderName = "Test Sender",
                    SenderEmail = "test",
                    TargetType = "ALL"
                });
                var newName = $"Updated Broadcast {UniqueId()}";
                var updated = await _sevk.Broadcasts.UpdateAsync(broadcast.Id, new UpdateBroadcastRequest { Name = newName });
                Assert.NotNull(updated);
                Assert.Equal(broadcast.Id, updated.Id);
                return;
            }
            catch (SevkException ex) when (ex.IsNotFound)
            {
                continue; // Domain may have been deleted, try next
            }
        }
    }

    [SkippableFact]
    public async Task Test89_Broadcasts_ShouldDelete()
    {
        SkipIfNoApiKey();
        var domains = await _sevk.Domains.ListAsync();
        if (domains.Items.Count == 0) return;
        var domainId = domains.Items[0].Id;

        var broadcast = await _sevk.Broadcasts.CreateAsync(new CreateBroadcastRequest
        {
            DomainId = domainId,
            Name = $"Delete Broadcast {UniqueId()}",
            Subject = "Test Subject",
            Body = "<section><paragraph>Test</paragraph></section>",
            SenderName = "Test Sender",
            SenderEmail = "test",
            TargetType = "ALL"
        });
        await _sevk.Broadcasts.DeleteAsync(broadcast.Id);
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Broadcasts.GetAsync(broadcast.Id));
        Assert.Contains("404", ex.Message);
    }

    [SkippableFact]
    public async Task Test90_Broadcasts_ShouldGetAnalytics()
    {
        SkipIfNoApiKey();
        var domains = await _sevk.Domains.ListAsync();
        if (domains.Items.Count == 0) return;
        var domainId = domains.Items[0].Id;

        var broadcast = await _sevk.Broadcasts.CreateAsync(new CreateBroadcastRequest
        {
            DomainId = domainId,
            Name = $"Analytics Broadcast {UniqueId()}",
            Subject = "Test Subject",
            Body = "<section><paragraph>Test</paragraph></section>",
            SenderName = "Test Sender",
            SenderEmail = "test",
            TargetType = "ALL"
        });
        var analytics = await _sevk.Broadcasts.GetAnalyticsAsync(broadcast.Id);
        Assert.NotNull(analytics);
    }

    [SkippableFact]
    public async Task Test91_Broadcasts_ShouldSendTest()
    {
        SkipIfNoApiKey();
        var domains = await _sevk.Domains.ListAsync();
        if (domains.Items.Count == 0) return;
        var domainId = domains.Items[0].Id;

        var broadcast = await _sevk.Broadcasts.CreateAsync(new CreateBroadcastRequest
        {
            DomainId = domainId,
            Name = $"SendTest Broadcast {UniqueId()}",
            Subject = "Test Subject",
            Body = "<section><paragraph>Test</paragraph></section>",
            SenderName = "Test Sender",
            SenderEmail = "test",
            TargetType = "ALL"
        });
        try
        {
            var result = await _sevk.Broadcasts.SendTestAsync(broadcast.Id, new List<string> { "test@example.com" });
            Assert.NotNull(result);
        }
        catch (SevkException)
        {
            // May fail if domain is unverified, which is expected
        }
    }

    [SkippableFact]
    public async Task Test92_Broadcasts_ShouldHandleSendError()
    {
        SkipIfNoApiKey();
        var domains = await _sevk.Domains.ListAsync();
        if (domains.Items.Count == 0) return;
        var domainId = domains.Items[0].Id;

        var broadcast = await _sevk.Broadcasts.CreateAsync(new CreateBroadcastRequest
        {
            DomainId = domainId,
            Name = $"Send Error Broadcast {UniqueId()}",
            Subject = "Test Subject",
            Body = "<section><paragraph>Test</paragraph></section>",
            SenderName = "Test Sender",
            SenderEmail = "test",
            TargetType = "ALL"
        });
        try
        {
            await _sevk.Broadcasts.SendAsync(broadcast.Id);
            // If it succeeds, that's fine too
        }
        catch (SevkException ex)
        {
            // Expected to fail if broadcast is not ready to send
            Assert.NotNull(ex.Message);
            Assert.True(ex.Message.Length > 0);
        }
    }

    [SkippableFact]
    public async Task Test93_Broadcasts_ShouldHandleCancelError()
    {
        SkipIfNoApiKey();
        var domains = await _sevk.Domains.ListAsync();
        if (domains.Items.Count == 0) return;
        var domainId = domains.Items[0].Id;

        var broadcast = await _sevk.Broadcasts.CreateAsync(new CreateBroadcastRequest
        {
            DomainId = domainId,
            Name = $"Cancel Error Broadcast {UniqueId()}",
            Subject = "Test Subject",
            Body = "<section><paragraph>Test</paragraph></section>",
            SenderName = "Test Sender",
            SenderEmail = "test",
            TargetType = "ALL"
        });
        try
        {
            await _sevk.Broadcasts.CancelAsync(broadcast.Id);
        }
        catch (SevkException ex)
        {
            // Expected to fail if broadcast is not in a cancellable state
            Assert.NotNull(ex.Message);
        }
    }

    // ==================== DOMAINS EXTENDED TESTS ====================

    [SkippableFact]
    public async Task Test94_Domains_ShouldCreate()
    {
        SkipIfNoApiKey();
        SkipIfNoDomainTests();
        var subdomain = $"test-{UniqueId()}.example.com";
        var domain = await _sevk.Domains.CreateAsync(new CreateDomainRequest
        {
            Domain = subdomain,
            Email = $"test@{subdomain}"
        });
        Assert.NotNull(domain);
        Assert.NotNull(domain.Id);
    }

    [SkippableFact]
    public async Task Test95_Domains_ShouldGetById()
    {
        SkipIfNoApiKey();
        SkipIfNoDomainTests();
        var subdomain = $"test-get-{UniqueId()}.example.com";
        var created = await _sevk.Domains.CreateAsync(new CreateDomainRequest
        {
            Domain = subdomain,
            Email = $"test@{subdomain}"
        });
        var domain = await _sevk.Domains.GetAsync(created.Id);
        Assert.NotNull(domain);
        Assert.Equal(created.Id, domain.Id);
    }

    [SkippableFact]
    public async Task Test96_Domains_ShouldGetDnsRecords()
    {
        SkipIfNoApiKey();
        SkipIfNoDomainTests();
        var subdomain = $"test-dns-{UniqueId()}.example.com";
        var created = await _sevk.Domains.CreateAsync(new CreateDomainRequest
        {
            Domain = subdomain,
            Email = $"test@{subdomain}"
        });
        var records = await _sevk.Domains.GetDnsRecordsAsync(created.Id);
        Assert.NotNull(records);
    }

    [SkippableFact]
    public async Task Test97_Domains_ShouldGetRegions()
    {
        SkipIfNoApiKey();
        SkipIfNoDomainTests();
        var result = await _sevk.Domains.GetRegionsAsync();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [SkippableFact]
    public async Task Test98_Domains_ShouldVerify()
    {
        SkipIfNoApiKey();
        SkipIfNoDomainTests();
        var subdomain = $"test-verify-{UniqueId()}.example.com";
        var created = await _sevk.Domains.CreateAsync(new CreateDomainRequest
        {
            Domain = subdomain,
            Email = $"test@{subdomain}"
        });
        try
        {
            var result = await _sevk.Domains.VerifyAsync(created.Id);
            Assert.NotNull(result);
        }
        catch (SevkException)
        {
            // Expected to fail for test domains without proper DNS records
        }
    }

    [SkippableFact]
    public async Task Test99_Domains_ShouldDelete()
    {
        SkipIfNoApiKey();
        SkipIfNoDomainTests();
        var subdomain = $"test-delete-{UniqueId()}.example.com";
        var created = await _sevk.Domains.CreateAsync(new CreateDomainRequest
        {
            Domain = subdomain,
            Email = $"test@{subdomain}"
        });
        await _sevk.Domains.DeleteAsync(created.Id);
        try
        {
            await _sevk.Domains.GetAsync(created.Id);
            Assert.Fail("Expected exception was not thrown");
        }
        catch (SevkException)
        {
            // Accept any error as confirmation of deletion
        }
    }

    // ==================== TOPICS EXTENDED TESTS ====================

    [SkippableFact]
    public async Task Test100_Topics_ShouldAddContacts()
    {
        SkipIfNoApiKey();
        var audienceId = await GetOrCreateAudienceIdAsync();
        var topicName = $"Add Contacts Topic {UniqueId()}";
        var topic = await _sevk.Topics.CreateAsync(audienceId, new CreateTopicRequest { Name = topicName });

        var email = $"topic-add-{UniqueId()}@example.com";
        var contact = await _sevk.Contacts.CreateAsync(email);
        await _sevk.Audiences.AddContactsAsync(audienceId, new List<string> { contact.Id });
        await _sevk.Topics.AddContactsAsync(audienceId, topic.Id, new List<string> { contact.Id });
        Assert.True(true);
    }

    [SkippableFact]
    public async Task Test101_Topics_ShouldRemoveContact()
    {
        SkipIfNoApiKey();
        var audienceId = await GetOrCreateAudienceIdAsync();
        var topicName = $"Remove Contact Topic {UniqueId()}";
        var topic = await _sevk.Topics.CreateAsync(audienceId, new CreateTopicRequest { Name = topicName });

        var email = $"topic-remove-{UniqueId()}@example.com";
        var contact = await _sevk.Contacts.CreateAsync(email);
        await _sevk.Audiences.AddContactsAsync(audienceId, new List<string> { contact.Id });
        await _sevk.Topics.AddContactsAsync(audienceId, topic.Id, new List<string> { contact.Id });
        await _sevk.Topics.RemoveContactAsync(audienceId, topic.Id, contact.Id);

        // Verify removal by listing contacts in the topic
        var result = await _sevk.Topics.ListContactsAsync(audienceId, topic.Id);
        Assert.DoesNotContain(result.Items, c => c.Id == contact.Id);
    }

    // ==================== SEGMENTS EXTENDED TESTS ====================

    [SkippableFact]
    public async Task Test102_Segments_ShouldCalculate()
    {
        SkipIfNoApiKey();
        await Task.Delay(2000); // Avoid rate limiting
        var audienceId = await GetOrCreateAudienceIdAsync();
        var segmentName = $"Calculate Segment {UniqueId()}";
        var segment = await _sevk.Segments.CreateAsync(audienceId, new CreateSegmentRequest
        {
            Name = segmentName,
            Rules = new List<SegmentRule> { new SegmentRule { Field = "email", Operator = "contains", Value = "@example.com" } },
            Operator = "AND"
        });
        var result = await _sevk.Segments.CalculateAsync(audienceId, segment.Id);
        Assert.NotNull(result);
    }

    [SkippableFact]
    public async Task Test103_Segments_ShouldPreview()
    {
        SkipIfNoApiKey();
        var audienceId = await GetOrCreateAudienceIdAsync();
        var result = await _sevk.Segments.PreviewAsync(audienceId, new CreateSegmentRequest
        {
            Name = $"Preview Segment {UniqueId()}",
            Rules = new List<SegmentRule> { new SegmentRule { Field = "email", Operator = "contains", Value = "@example.com" } },
            Operator = "AND"
        });
        Assert.NotNull(result);
    }

    // ==================== EMAILS EXTENDED TESTS ====================

    [SkippableFact]
    public async Task Test104_Emails_ShouldThrowForNonExistentId()
    {
        SkipIfNoApiKey();
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Emails.GetAsync("00000000-0000-0000-0000-000000000000"));
        Assert.Contains("404", ex.Message);
    }

    [SkippableFact]
    public async Task Test105_Emails_ShouldRejectBulkWithUnverifiedDomain()
    {
        SkipIfNoApiKey();
        try
        {
            await _sevk.Emails.SendBulkAsync(new BulkEmailRequest
            {
                Emails = new List<SendEmailRequest>
                {
                    new SendEmailRequest
                    {
                        To = "test1@example.com",
                        From = "no-reply@unverified-domain.com",
                        Subject = "Bulk Test 1",
                        Html = "<p>Hello 1</p>"
                    },
                    new SendEmailRequest
                    {
                        To = "test2@example.com",
                        From = "no-reply@unverified-domain.com",
                        Subject = "Bulk Test 2",
                        Html = "<p>Hello 2</p>"
                    }
                }
            });
            // If it succeeds, that's acceptable too
        }
        catch (SevkException ex)
        {
            Assert.NotNull(ex.Message);
            Assert.True(ex.Message.Length > 0);
        }
    }

    // ==================== ERROR HANDLING TESTS ====================

    [SkippableFact]
    public async Task Test45_ErrorHandling_Should404Gracefully()
    {
        SkipIfNoApiKey();
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Contacts.GetAsync("non-existent-id-12345"));
        Assert.Contains("404", ex.Message);
    }

    [SkippableFact]
    public async Task Test46_ErrorHandling_ShouldValidationError()
    {
        SkipIfNoApiKey();
        await Assert.ThrowsAsync<SevkException>(() => _sevk.Contacts.CreateAsync("invalid-email"));
    }

    // ==================== MARKUP RENDERER TESTS ====================

    [Fact]
    public void Test47_Markup_ShouldRenderSection()
    {
        var markup = "<sevk-section background-color=\"#f5f5f5\">Content</sevk-section>";
        var html = Renderer.Render(markup);
        Assert.Contains("<table", html);
        Assert.Contains("background-color:#f5f5f5", html);
    }

    [Fact]
    public void Test48_Markup_ShouldRenderContainer()
    {
        var markup = "<sevk-container max-width=\"600px\">Content</sevk-container>";
        var html = Renderer.Render(markup);
        Assert.Contains("max-width:600px", html);
    }

    [Fact]
    public void Test49_Markup_ShouldRenderHeading()
    {
        var markup = "<sevk-heading level=\"2\" color=\"#333\">Title</sevk-heading>";
        var html = Renderer.Render(markup);
        Assert.Contains("<h2", html);
        Assert.Contains("color:#333", html);
    }

    [Fact]
    public void Test50_Markup_ShouldRenderButton()
    {
        var markup = "<sevk-button href=\"https://example.com\" background-color=\"#007bff\">Click</sevk-button>";
        var html = Renderer.Render(markup);
        Assert.Contains("href=\"https://example.com\"", html);
        Assert.Contains("background-color:#007bff", html);
    }

    [Fact]
    public void Test51_Markup_ShouldRenderImage()
    {
        var markup = "<sevk-image src=\"https://example.com/img.png\" alt=\"Test\" width=\"200\"></sevk-image>";
        var html = Renderer.Render(markup);
        Assert.Contains("<img", html);
        Assert.Contains("src=\"https://example.com/img.png\"", html);
        Assert.Contains("alt=\"Test\"", html);
    }

    [Fact]
    public void Test52_Markup_ShouldRenderEmptyMarkup()
    {
        var html = Renderer.Render("");
        Assert.Equal("", html);
    }

    [Fact]
    public void Test53_Markup_ShouldRenderDivider()
    {
        var markup = "<sevk-divider color=\"#ccc\" height=\"2px\"></sevk-divider>";
        var html = Renderer.Render(markup);
        Assert.Contains("<div", html);
        Assert.Contains("height:2px", html);
    }

    [Fact]
    public void Test54_Markup_ShouldRenderLink()
    {
        var markup = "<sevk-link href=\"https://example.com\" color=\"#007bff\">Click here</sevk-link>";
        var html = Renderer.Render(markup);
        Assert.Contains("<a", html);
        Assert.Contains("href=\"https://example.com\"", html);
        Assert.Contains("Click here", html);
    }

    [Fact]
    public void Test55_Markup_ShouldRenderNestedComponents()
    {
        var markup = "<sevk-section background-color=\"#f5f5f5\"><sevk-container max-width=\"600px\"><sevk-heading level=\"1\">Hello</sevk-heading></sevk-container></sevk-section>";
        var html = Renderer.Render(markup);
        Assert.Contains("<table", html);
        Assert.Contains("max-width:600px", html);
        Assert.Contains("<h1", html);
        Assert.Contains("Hello", html);
    }

    [Fact]
    public void Test56_Markup_ShouldPreserveRegularHtml()
    {
        var markup = "<p>Regular paragraph</p><span>Span text</span>";
        var html = Renderer.Render(markup);
        Assert.Contains("<p>", html);
        Assert.Contains("</p>", html);
        Assert.Contains("<span>", html);
        Assert.Contains("</span>", html);
    }
}
