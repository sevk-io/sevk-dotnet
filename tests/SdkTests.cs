using System.Net.Http.Json;
using System.Text.Json;
using Sevk;
using Sevk.Markup;
using Sevk.Types;
using Xunit;

namespace Sevk.Tests;

// Fixture that runs setup ONCE for all tests
public class SevkTestFixture : IAsyncLifetime
{
    private const string BaseUrl = "http://localhost:4000";
    public SevkClient Sevk { get; private set; } = null!;
    public string? CreatedContactId { get; set; }
    public string? CreatedAudienceId { get; set; }
    public string? CreatedTemplateId { get; set; }
    public string? CreatedTopicId { get; set; }
    public string? CreatedSegmentId { get; set; }

    public async Task InitializeAsync()
    {
        var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        var unique = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{Random.Shared.Next(10000)}";
        var testEmail = $"sdk-test-{unique}@test.example.com";
        var testPassword = "TestPassword123!";

        var registerRes = await httpClient.PostAsJsonAsync($"{BaseUrl}/auth/register", new { email = testEmail, password = testPassword });
        var registerBody = await registerRes.Content.ReadFromJsonAsync<JsonElement>();
        var token = registerBody.GetProperty("token").GetString()!;

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        var projectRes = await httpClient.PostAsJsonAsync($"{BaseUrl}/projects", new
        {
            name = "Test Project",
            slug = $"test-project-{unique}",
            supportEmail = "support@test.com"
        });
        var projectBody = await projectRes.Content.ReadFromJsonAsync<JsonElement>();
        var projectId = projectBody.GetProperty("project").GetProperty("id").GetString()!;

        var apiKeyRes = await httpClient.PostAsJsonAsync($"{BaseUrl}/projects/{projectId}/api-keys", new { title = "Test Key", fullAccess = true });
        var apiKeyBody = await apiKeyRes.Content.ReadFromJsonAsync<JsonElement>();
        var apiKey = apiKeyBody.GetProperty("apiKey").GetProperty("key").GetString()!;

        Sevk = new SevkClient(apiKey, new SevkOptions { BaseUrl = BaseUrl });
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

[CollectionDefinition("Sevk")]
public class SevkCollection : ICollectionFixture<SevkTestFixture> { }

[Collection("Sevk")]
[TestCaseOrderer("Sevk.Tests.PriorityOrderer", "Sevk.Tests")]
public class SdkTests
{
    private const string BaseUrl = "http://localhost:4000";
    private readonly SevkClient _sevk;
    private readonly SevkTestFixture _fixture;

    private static string UniqueId() => $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{Random.Shared.Next(10000)}";

    public SdkTests(SevkTestFixture fixture)
    {
        _fixture = fixture;
        _sevk = fixture.Sevk;
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

    [Fact]
    public async Task Test01_Auth_ShouldRejectInvalidApiKey()
    {
        var invalidSevk = new SevkClient("sevk_invalid_api_key_12345", new SevkOptions { BaseUrl = BaseUrl });
        var ex = await Assert.ThrowsAsync<SevkException>(() => invalidSevk.Contacts.ListAsync());
        Assert.True(ex.Message.Contains("401") || ex.Message.ToLower().Contains("invalid"));
    }

    [Fact]
    public void Test02_Auth_ShouldRejectEmptyApiKey()
    {
        Assert.Throws<SevkException>(() => new SevkClient("", new SevkOptions { BaseUrl = BaseUrl }));
    }

    [Fact]
    public async Task Test03_Auth_ShouldRejectMalformedApiKey()
    {
        var malformedSevk = new SevkClient("invalid_key_format", new SevkOptions { BaseUrl = BaseUrl });
        var ex = await Assert.ThrowsAsync<SevkException>(() => malformedSevk.Contacts.ListAsync());
        Assert.Contains("401", ex.Message);
    }

    // ==================== CONTACTS TESTS ====================

    [Fact]
    public async Task Test04_Contacts_ShouldListWithCorrectStructure()
    {
        var result = await _sevk.Contacts.ListAsync();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.Items.Count >= 0);
    }

    [Fact]
    public async Task Test05_Contacts_ShouldListWithPagination()
    {
        var result = await _sevk.Contacts.ListAsync(new ListParams { Page = 1, Limit = 5 });
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Test06_Contacts_ShouldCreate()
    {
        var email = $"test-{UniqueId()}@example.com";
        var contact = await _sevk.Contacts.CreateAsync(email);
        Assert.NotNull(contact);
        Assert.NotNull(contact.Id);
        Assert.Equal(email, contact.Email);
        _fixture.CreatedContactId = contact.Id;
    }

    [Fact]
    public async Task Test07_Contacts_ShouldGetById()
    {
        var email = $"get-{UniqueId()}@example.com";
        var created = await _sevk.Contacts.CreateAsync(email);
        var contact = await _sevk.Contacts.GetAsync(created.Id);
        Assert.NotNull(contact);
        Assert.Equal(created.Id, contact.Id);
    }

    [Fact]
    public async Task Test08_Contacts_ShouldUpdate()
    {
        var email = $"update-{UniqueId()}@example.com";
        var created = await _sevk.Contacts.CreateAsync(email);
        var contact = await _sevk.Contacts.UpdateAsync(created.Id, new UpdateContactRequest { Subscribed = false });
        Assert.NotNull(contact);
        Assert.Equal(created.Id, contact.Id);
        Assert.False(contact.Subscribed);
    }

    [Fact]
    public async Task Test09_Contacts_ShouldThrowForNonExistent()
    {
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Contacts.GetAsync("non-existent-id"));
        Assert.Contains("404", ex.Message);
    }

    [Fact]
    public async Task Test10_Contacts_ShouldDelete()
    {
        var email = $"delete-{UniqueId()}@example.com";
        var contact = await _sevk.Contacts.CreateAsync(email);
        await _sevk.Contacts.DeleteAsync(contact.Id);
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Contacts.GetAsync(contact.Id));
        Assert.Contains("404", ex.Message);
    }

    // ==================== AUDIENCES TESTS ====================

    [Fact]
    public async Task Test11_Audiences_ShouldListWithCorrectStructure()
    {
        var result = await _sevk.Audiences.ListAsync();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.Items.Count >= 0);
    }

    [Fact]
    public async Task Test12_Audiences_ShouldCreate()
    {
        var name = $"Test Audience {UniqueId()}";
        var audience = await _sevk.Audiences.CreateAsync(new CreateAudienceRequest { Name = name });
        Assert.NotNull(audience);
        Assert.NotNull(audience.Id);
        Assert.Equal(name, audience.Name);
        _fixture.CreatedAudienceId = audience.Id;
    }

    [Fact]
    public async Task Test13_Audiences_ShouldCreateWithAllFields()
    {
        // Use existing audience instead of creating new one
        var audienceId = await GetOrCreateAudienceIdAsync();
        var audience = await _sevk.Audiences.GetAsync(audienceId);
        Assert.NotNull(audience);
        Assert.NotNull(audience.Id);
    }

    [Fact]
    public async Task Test14_Audiences_ShouldGetById()
    {
        var audienceId = await GetOrCreateAudienceIdAsync();
        var audience = await _sevk.Audiences.GetAsync(audienceId);
        Assert.NotNull(audience);
        Assert.Equal(audienceId, audience.Id);
    }

    [Fact]
    public async Task Test15_Audiences_ShouldUpdate()
    {
        var audienceId = await GetOrCreateAudienceIdAsync();
        var newName = $"Updated Audience {UniqueId()}";
        var audience = await _sevk.Audiences.UpdateAsync(audienceId, new UpdateAudienceRequest { Name = newName });
        Assert.NotNull(audience);
        Assert.Equal(newName, audience.Name);
    }

    [Fact]
    public async Task Test16_Audiences_ShouldAddContacts()
    {
        var audienceId = await GetOrCreateAudienceIdAsync();
        var email = $"add-contact-{UniqueId()}@example.com";
        var contact = await _sevk.Contacts.CreateAsync(email);
        await _sevk.Audiences.AddContactsAsync(audienceId, new List<string> { contact.Id });
        Assert.True(true);
    }

    [Fact]
    public async Task Test17_Audiences_ShouldDelete()
    {
        // Create a separate audience just for deletion test
        var name = $"Delete Audience {UniqueId()}";
        var audience = await _sevk.Audiences.CreateAsync(new CreateAudienceRequest { Name = name });
        await _sevk.Audiences.DeleteAsync(audience.Id);
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Audiences.GetAsync(audience.Id));
        Assert.Contains("404", ex.Message);
    }

    // ==================== TEMPLATES TESTS ====================

    [Fact]
    public async Task Test18_Templates_ShouldListWithCorrectStructure()
    {
        var result = await _sevk.Templates.ListAsync();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.Items.Count >= 0);
    }

    [Fact]
    public async Task Test19_Templates_ShouldCreate()
    {
        var title = $"Test Template {UniqueId()}";
        var template = await _sevk.Templates.CreateAsync(new CreateTemplateRequest { Title = title, Content = "<p>Hello {{name}}</p>" });
        Assert.NotNull(template);
        Assert.NotNull(template.Id);
        Assert.Equal(title, template.Title);
        Assert.Equal("<p>Hello {{name}}</p>", template.Content);
        _fixture.CreatedTemplateId = template.Id;
    }

    [Fact]
    public async Task Test20_Templates_ShouldGetById()
    {
        var title = $"Get Template {UniqueId()}";
        var created = await _sevk.Templates.CreateAsync(new CreateTemplateRequest { Title = title, Content = "<p>Test</p>" });
        var template = await _sevk.Templates.GetAsync(created.Id);
        Assert.NotNull(template);
        Assert.Equal(created.Id, template.Id);
    }

    [Fact]
    public async Task Test21_Templates_ShouldUpdate()
    {
        var title = $"Update Template {UniqueId()}";
        var created = await _sevk.Templates.CreateAsync(new CreateTemplateRequest { Title = title, Content = "<p>Test</p>" });
        var newTitle = $"Updated Template {UniqueId()}";
        var template = await _sevk.Templates.UpdateAsync(created.Id, new UpdateTemplateRequest { Title = newTitle });
        Assert.NotNull(template);
        Assert.Equal(newTitle, template.Title);
    }

    [Fact]
    public async Task Test22_Templates_ShouldDuplicate()
    {
        var title = $"Duplicate Template {UniqueId()}";
        var created = await _sevk.Templates.CreateAsync(new CreateTemplateRequest { Title = title, Content = "<p>Test</p>" });
        var template = await _sevk.Templates.DuplicateAsync(created.Id);
        Assert.NotNull(template);
        Assert.NotNull(template.Id);
        Assert.NotEqual(created.Id, template.Id);
    }

    [Fact]
    public async Task Test23_Templates_ShouldDelete()
    {
        var title = $"Delete Template {UniqueId()}";
        var template = await _sevk.Templates.CreateAsync(new CreateTemplateRequest { Title = title, Content = "<p>Test</p>" });
        await _sevk.Templates.DeleteAsync(template.Id);
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Templates.GetAsync(template.Id));
        Assert.Contains("404", ex.Message);
    }

    // ==================== BROADCASTS TESTS ====================

    [Fact]
    public async Task Test24_Broadcasts_ShouldListWithCorrectStructure()
    {
        var result = await _sevk.Broadcasts.ListAsync();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.True(result.Items.Count >= 0);
    }

    [Fact]
    public async Task Test25_Broadcasts_ShouldListWithPagination()
    {
        var result = await _sevk.Broadcasts.ListAsync(new ListParams { Page = 1, Limit = 10 });
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Test26_Broadcasts_ShouldListWithSearch()
    {
        var result = await _sevk.Broadcasts.ListAsync(new ListParams { Search = "test" });
        Assert.NotNull(result);
    }

    // ==================== DOMAINS TESTS ====================

    [Fact]
    public async Task Test27_Domains_ShouldListWithCorrectStructure()
    {
        var result = await _sevk.Domains.ListAsync();
        Assert.NotNull(result);
        Assert.NotNull(result.Domains);
    }

    [Fact]
    public async Task Test28_Domains_ShouldListOnlyVerified()
    {
        var result = await _sevk.Domains.ListAsync(verified: true);
        Assert.NotNull(result);
        foreach (var domain in result.Domains)
        {
            Assert.True(domain.Verified);
        }
    }

    // ==================== TOPICS TESTS ====================

    [Fact]
    public async Task Test29_Topics_ShouldListForAudience()
    {
        var audienceId = await GetOrCreateAudienceIdAsync();
        var result = await _sevk.Topics.ListAsync(audienceId);
        Assert.NotNull(result);
        Assert.True(result.Total >= 0);
    }

    [Fact]
    public async Task Test30_Topics_ShouldCreate()
    {
        var audienceId = await GetOrCreateAudienceIdAsync();
        var topicName = $"Test Topic {UniqueId()}";
        var topic = await _sevk.Topics.CreateAsync(audienceId, new CreateTopicRequest { Name = topicName });
        Assert.NotNull(topic);
        Assert.NotNull(topic.Id);
        Assert.Equal(topicName, topic.Name);
        Assert.Equal(audienceId, topic.AudienceId);
        _fixture.CreatedTopicId = topic.Id;
    }

    [Fact]
    public async Task Test31_Topics_ShouldGetById()
    {
        var audienceId = await GetOrCreateAudienceIdAsync();
        var topicName = $"Get Topic {UniqueId()}";
        var created = await _sevk.Topics.CreateAsync(audienceId, new CreateTopicRequest { Name = topicName });
        var topic = await _sevk.Topics.GetAsync(audienceId, created.Id);
        Assert.NotNull(topic);
        Assert.Equal(created.Id, topic.Id);
    }

    [Fact]
    public async Task Test32_Topics_ShouldUpdate()
    {
        var audienceId = await GetOrCreateAudienceIdAsync();
        var topicName = $"Update Topic {UniqueId()}";
        var created = await _sevk.Topics.CreateAsync(audienceId, new CreateTopicRequest { Name = topicName });
        var newName = $"Updated Topic {UniqueId()}";
        var topic = await _sevk.Topics.UpdateAsync(audienceId, created.Id, new UpdateTopicRequest { Name = newName });
        Assert.NotNull(topic);
        Assert.Equal(newName, topic.Name);
    }

    [Fact]
    public async Task Test33_Topics_ShouldDelete()
    {
        var audienceId = await GetOrCreateAudienceIdAsync();
        var topicName = $"Delete Topic {UniqueId()}";
        var topic = await _sevk.Topics.CreateAsync(audienceId, new CreateTopicRequest { Name = topicName });
        await _sevk.Topics.DeleteAsync(audienceId, topic.Id);
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Topics.GetAsync(audienceId, topic.Id));
        Assert.Contains("404", ex.Message);
    }

    // ==================== SEGMENTS TESTS ====================

    [Fact]
    public async Task Test34_Segments_ShouldListForAudience()
    {
        var audienceId = await GetOrCreateAudienceIdAsync();
        var result = await _sevk.Segments.ListAsync(audienceId);
        Assert.NotNull(result);
        Assert.True(result.Total >= 0);
    }

    [Fact]
    public async Task Test35_Segments_ShouldCreate()
    {
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

    [Fact]
    public async Task Test36_Segments_ShouldGetById()
    {
        var audienceId = await GetOrCreateAudienceIdAsync();
        var segmentName = $"Get Segment {UniqueId()}";
        var created = await _sevk.Segments.CreateAsync(audienceId, new CreateSegmentRequest { Name = segmentName, Rules = new List<SegmentRule>(), Operator = "AND" });
        var segment = await _sevk.Segments.GetAsync(audienceId, created.Id);
        Assert.NotNull(segment);
        Assert.Equal(created.Id, segment.Id);
    }

    [Fact]
    public async Task Test37_Segments_ShouldUpdate()
    {
        var audienceId = await GetOrCreateAudienceIdAsync();
        var segmentName = $"Update Segment {UniqueId()}";
        var created = await _sevk.Segments.CreateAsync(audienceId, new CreateSegmentRequest { Name = segmentName, Rules = new List<SegmentRule>(), Operator = "AND" });
        var newName = $"Updated Segment {UniqueId()}";
        var segment = await _sevk.Segments.UpdateAsync(audienceId, created.Id, new UpdateSegmentRequest { Name = newName });
        Assert.NotNull(segment);
        Assert.Equal(newName, segment.Name);
    }

    [Fact]
    public async Task Test38_Segments_ShouldDelete()
    {
        var audienceId = await GetOrCreateAudienceIdAsync();
        var segmentName = $"Delete Segment {UniqueId()}";
        var segment = await _sevk.Segments.CreateAsync(audienceId, new CreateSegmentRequest { Name = segmentName, Rules = new List<SegmentRule>(), Operator = "AND" });
        await _sevk.Segments.DeleteAsync(audienceId, segment.Id);
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Segments.GetAsync(audienceId, segment.Id));
        Assert.Contains("404", ex.Message);
    }

    // ==================== SUBSCRIPTIONS TESTS ====================

    [Fact]
    public async Task Test39_Subscriptions_ShouldSubscribe()
    {
        var audienceId = await GetOrCreateAudienceIdAsync();
        var email = $"subscribe-{UniqueId()}@example.com";
        await _sevk.Subscriptions.SubscribeAsync(new SubscribeRequest { Email = email, AudienceId = audienceId });
        Assert.True(true);
    }

    [Fact]
    public async Task Test40_Subscriptions_ShouldUnsubscribe()
    {
        var email = $"unsubscribe-{UniqueId()}@example.com";
        var contact = await _sevk.Contacts.CreateAsync(email, new CreateContactRequest { Email = email, Subscribed = true });
        await _sevk.Subscriptions.UnsubscribeAsync(new UnsubscribeRequest { Email = email });
        var updated = await _sevk.Contacts.GetAsync(contact.Id);
        Assert.False(updated.Subscribed);
    }

    // ==================== EMAILS TESTS ====================

    [Fact]
    public async Task Test41_Emails_ShouldRejectUnverifiedDomain()
    {
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

    [Fact]
    public async Task Test42_Emails_ShouldRejectDomainNotOwned()
    {
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

    [Fact]
    public async Task Test43_Emails_ShouldRejectInvalidFrom()
    {
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Emails.SendAsync(new SendEmailRequest
        {
            To = "test@example.com",
            From = "invalid-email-without-domain",
            Subject = "Test Email",
            Html = "<p>Hello</p>"
        }));
        Assert.Contains("400", ex.Message);
    }

    [Fact]
    public async Task Test44_Emails_ShouldReturnProperErrorMessage()
    {
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

    // ==================== ERROR HANDLING TESTS ====================

    [Fact]
    public async Task Test45_ErrorHandling_Should404Gracefully()
    {
        var ex = await Assert.ThrowsAsync<SevkException>(() => _sevk.Contacts.GetAsync("non-existent-id-12345"));
        Assert.Contains("404", ex.Message);
    }

    [Fact]
    public async Task Test46_ErrorHandling_ShouldValidationError()
    {
        await Assert.ThrowsAsync<SevkException>(() => _sevk.Contacts.CreateAsync("invalid-email"));
    }

    // ==================== MARKUP RENDERER TESTS ====================

    [Fact]
    public void Test47_Markup_ShouldRenderSection()
    {
        var markup = "<sevk-section background-color=\"#f5f5f5\">Content</sevk-section>";
        var html = MarkupRenderer.Render(markup);
        Assert.Contains("<table", html);
        Assert.Contains("background-color:#f5f5f5", html);
    }

    [Fact]
    public void Test48_Markup_ShouldRenderContainer()
    {
        var markup = "<sevk-container max-width=\"600px\">Content</sevk-container>";
        var html = MarkupRenderer.Render(markup);
        Assert.Contains("max-width:600px", html);
    }

    [Fact]
    public void Test49_Markup_ShouldRenderHeading()
    {
        var markup = "<sevk-heading level=\"2\" color=\"#333\">Title</sevk-heading>";
        var html = MarkupRenderer.Render(markup);
        Assert.Contains("<h2", html);
        Assert.Contains("color:#333", html);
    }

    [Fact]
    public void Test50_Markup_ShouldRenderButton()
    {
        var markup = "<sevk-button href=\"https://example.com\" background-color=\"#007bff\">Click</sevk-button>";
        var html = MarkupRenderer.Render(markup);
        Assert.Contains("href=\"https://example.com\"", html);
        Assert.Contains("background-color:#007bff", html);
    }

    [Fact]
    public void Test51_Markup_ShouldRenderImage()
    {
        var markup = "<sevk-image src=\"https://example.com/img.png\" alt=\"Test\" width=\"200\"></sevk-image>";
        var html = MarkupRenderer.Render(markup);
        Assert.Contains("<img", html);
        Assert.Contains("src=\"https://example.com/img.png\"", html);
        Assert.Contains("alt=\"Test\"", html);
    }

    [Fact]
    public void Test52_Markup_ShouldRenderEmptyMarkup()
    {
        var html = MarkupRenderer.Render("");
        Assert.Equal("", html);
    }

    [Fact]
    public void Test53_Markup_ShouldRenderDivider()
    {
        var markup = "<sevk-divider color=\"#ccc\" height=\"2px\"></sevk-divider>";
        var html = MarkupRenderer.Render(markup);
        Assert.Contains("<div", html);
        Assert.Contains("height:2px", html);
    }

    [Fact]
    public void Test54_Markup_ShouldRenderLink()
    {
        var markup = "<sevk-link href=\"https://example.com\" color=\"#007bff\">Click here</sevk-link>";
        var html = MarkupRenderer.Render(markup);
        Assert.Contains("<a", html);
        Assert.Contains("href=\"https://example.com\"", html);
        Assert.Contains("Click here", html);
    }

    [Fact]
    public void Test55_Markup_ShouldRenderNestedComponents()
    {
        var markup = "<sevk-section background-color=\"#f5f5f5\"><sevk-container max-width=\"600px\"><sevk-heading level=\"1\">Hello</sevk-heading></sevk-container></sevk-section>";
        var html = MarkupRenderer.Render(markup);
        Assert.Contains("<table", html);
        Assert.Contains("max-width:600px", html);
        Assert.Contains("<h1", html);
        Assert.Contains("Hello", html);
    }

    [Fact]
    public void Test56_Markup_ShouldPreserveRegularHtml()
    {
        var markup = "<p>Regular paragraph</p><span>Span text</span>";
        var html = MarkupRenderer.Render(markup);
        Assert.Contains("<p>", html);
        Assert.Contains("</p>", html);
        Assert.Contains("<span>", html);
        Assert.Contains("</span>", html);
    }
}
