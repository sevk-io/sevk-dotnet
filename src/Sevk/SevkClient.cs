using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sevk.Resources;

namespace Sevk;

/// <summary>
/// Options for configuring the Sevk client
/// </summary>
public class SevkOptions
{
    public string BaseUrl { get; set; } = "https://api.sevk.io";
    public int Timeout { get; set; } = 30000;
}

/// <summary>
/// Exception thrown by Sevk SDK
/// </summary>
public class SevkException : Exception
{
    public int StatusCode { get; }

    public SevkException(string message, int statusCode = 0) : base(message)
    {
        StatusCode = statusCode;
    }

    public bool IsNotFound => StatusCode == 404;
    public bool IsUnauthorized => StatusCode == 401;
    public bool IsForbidden => StatusCode == 403;
    public bool IsValidationError => StatusCode == 400 || StatusCode == 422;
}

/// <summary>
/// Main Sevk SDK client
/// </summary>
public class SevkClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly SevkOptions _options;
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    public ContactsResource Contacts { get; }
    public AudiencesResource Audiences { get; }
    public TemplatesResource Templates { get; }
    public BroadcastsResource Broadcasts { get; }
    public DomainsResource Domains { get; }
    public TopicsResource Topics { get; }
    public SegmentsResource Segments { get; }
    public SubscriptionsResource Subscriptions { get; }
    public EmailsResource Emails { get; }
    public WebhooksResource Webhooks { get; }
    public EventsResource Events { get; }

    public SevkClient(string apiKey, SevkOptions? options = null)
    {
        if (string.IsNullOrEmpty(apiKey))
            throw new SevkException("API key is required", 401);

        _apiKey = apiKey;
        _options = options ?? new SevkOptions();

        // Ensure BaseUrl ends with / for proper path combining
        _baseUrl = _options.BaseUrl.EndsWith("/") ? _options.BaseUrl : _options.BaseUrl + "/";
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMilliseconds(_options.Timeout)
        };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        Contacts = new ContactsResource(this);
        Audiences = new AudiencesResource(this);
        Templates = new TemplatesResource(this);
        Broadcasts = new BroadcastsResource(this);
        Domains = new DomainsResource(this);
        Topics = new TopicsResource(this);
        Segments = new SegmentsResource(this);
        Subscriptions = new SubscriptionsResource(this);
        Emails = new EmailsResource(this);
        Webhooks = new WebhooksResource(this);
        Events = new EventsResource(this);
    }

    private string BuildUrl(string path)
    {
        var normalizedPath = path.StartsWith("/") ? path.TrimStart('/') : path;
        return _baseUrl + normalizedPath;
    }

    internal async Task<T> GetAsync<T>(string path)
    {
        var response = await _httpClient.GetAsync(BuildUrl(path));
        return await HandleResponse<T>(response);
    }

    internal async Task<T> PostAsync<T>(string path, object? body = null)
    {
        var content = body != null
            ? new StringContent(JsonSerializer.Serialize(body, _jsonOptions), Encoding.UTF8, "application/json")
            : null;
        var response = await _httpClient.PostAsync(BuildUrl(path), content);
        return await HandleResponse<T>(response);
    }

    internal async Task<T> PatchAsync<T>(string path, object body)
    {
        var content = new StringContent(JsonSerializer.Serialize(body, _jsonOptions), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl(path)) { Content = content };
        var response = await _httpClient.SendAsync(request);
        return await HandleResponse<T>(response);
    }

    internal async Task DeleteAsync(string path)
    {
        var response = await _httpClient.DeleteAsync(BuildUrl(path));
        await HandleResponse(response);
    }

    private async Task<T> HandleResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new SevkException($"{(int)response.StatusCode}: {content}", (int)response.StatusCode);
        }

        if (string.IsNullOrEmpty(content))
            return default!;

        return JsonSerializer.Deserialize<T>(content, _jsonOptions)!;
    }

    private async Task HandleResponse(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new SevkException($"{(int)response.StatusCode}: {content}", (int)response.StatusCode);
        }
    }

    /// <summary>
    /// Get project usage and limits
    /// </summary>
    public async Task<JsonDocument> GetUsageAsync()
    {
        return await GetAsync<JsonDocument>("/limits");
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
