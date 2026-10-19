using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sevk.Resources;
using Sevk.Types;

namespace Sevk;

/// <summary>
/// Main sevk SDK client
/// </summary>
public class SevkClient : IDisposable
{
    private static readonly ProductInfoHeaderValue UserAgent = new(
        "sevk-dotnet",
        typeof(SevkClient).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion.Split('+')[0]);

    private readonly HttpClient _httpClient;
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
    public ActivityResource Activity { get; }
    public InboundResource Inbound { get; }
    public OutboundResource Outbound { get; }
    public UsageResource Usage { get; }

    /// <summary>Creates an API client and exposes each resource through its matching property.</summary>
    public SevkClient(string apiKey, SevkOptions? options = null)
    {
        var clientOptions = options ?? new SevkOptions();

        _baseUrl = clientOptions.BaseUrl.TrimEnd('/') + "/";
        _httpClient = new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = false
        })
        {
            Timeout = TimeSpan.FromMilliseconds(clientOptions.Timeout)
        };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.UserAgent.Add(UserAgent);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        _jsonOptions.Converters.Add(new JsonDataDictionaryConverter(_jsonOptions));

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
        Activity = new ActivityResource(this);
        Inbound = new InboundResource(this);
        Outbound = new OutboundResource(this);
        Usage = new UsageResource(this);
    }

    private string BuildUrl(string path)
    {
        var normalizedPath = path.StartsWith("/") ? path.TrimStart('/') : path;

        return _baseUrl + normalizedPath;
    }

    internal Task<T> GetAsync<T>(string path) =>
        RequestAsync<T>(HttpMethod.Get, path);

    internal Task<T> PostAsync<T>(string path, object? body = null, IDictionary<string, string>? headers = null) =>
        RequestAsync<T>(HttpMethod.Post, path, body, headers);

    internal Task<T> PutAsync<T>(string path, object body) =>
        RequestAsync<T>(HttpMethod.Put, path, body);

    internal Task<T> PatchAsync<T>(string path, object body) =>
        RequestAsync<T>(HttpMethod.Patch, path, body);

    internal Task<T> DeleteAsync<T>(string path) =>
        RequestAsync<T>(HttpMethod.Delete, path);

    private async Task<T> RequestAsync<T>(HttpMethod method, string path, object? body = null, IDictionary<string, string>? headers = null)
    {
        using var request = new HttpRequestMessage(method, BuildUrl(path));
        if (body != null || method == HttpMethod.Put || method == HttpMethod.Patch)
            request.Content = new StringContent(SerializeBody(body), Encoding.UTF8, "application/json");

        if (headers != null)
        {
            foreach (var header in headers)
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        using var response = await SendAsync(request);

        return await HandleResponse<T>(response);
    }

    private string SerializeBody(object? body)
    {
        try
        {
            return JsonSerializer.Serialize(body, _jsonOptions);
        }
        catch (Exception error) when (error is JsonException or ArgumentException or NotSupportedException)
        {
            throw new SevkException("Request body must be JSON serializable", code: "SERIALIZATION_ERROR", innerException: error);
        }
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        try
        {
            return await _httpClient.SendAsync(request);
        }
        catch (OperationCanceledException error)
        {
            throw new SevkException(error.Message, code: error.InnerException is TimeoutException ? "TIMEOUT" : "REQUEST_CANCELLED", innerException: error);
        }
        catch (HttpRequestException error)
        {
            throw new SevkException(error.Message, code: "NETWORK_ERROR", innerException: error);
        }
    }

    private async Task<T> HandleResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw BuildError((int)response.StatusCode, response.Headers, content);
        }

        if (string.IsNullOrWhiteSpace(content) && response.StatusCode != HttpStatusCode.NoContent)
            throw new SevkException(
                "Expected a JSON response body but received an empty response",
                (int)response.StatusCode,
                "INVALID_RESPONSE",
                responseBody: content
            );

        try
        {
            var payload = response.StatusCode == HttpStatusCode.NoContent ? "{}" : content;
            var result = JsonSerializer.Deserialize<T>(payload, _jsonOptions);
            if (result is null)
                throw InvalidResponse((int)response.StatusCode, content);
            return result;
        }
        catch (JsonException)
        {
            throw InvalidResponse((int)response.StatusCode, content);
        }
    }

    private static SevkException InvalidResponse(int status, string content) => new(
        "Expected a JSON response body but received empty or malformed JSON",
        status,
        "INVALID_RESPONSE",
        responseBody: content
    );

    internal static SevkException BuildError(int status, HttpResponseHeaders headers, string content)
    {
        var message = string.IsNullOrEmpty(content) ? $"HTTP {status}" : content;
        var code = "UNKNOWN_ERROR";
        IReadOnlyDictionary<string, IReadOnlyList<string>>? fields = null;
        JsonElement? details = null;
        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("error", out var err) &&
                err.ValueKind == JsonValueKind.Object)
                root = err;

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
                    message = m.GetString() ?? message;

                if (root.TryGetProperty("code", out var c) && c.ValueKind == JsonValueKind.String)
                    code = c.GetString() ?? code;

                if (root.TryGetProperty("details", out var d))
                    details = d.Clone();
                fields = ParseFields(root);
            }
        }
        catch (JsonException)
        {
        }

        int? retryAfter = null;

        if (headers?.RetryAfter?.Delta is { } delta &&
            delta.TotalSeconds is >= 0 and <= int.MaxValue)
            retryAfter = (int)delta.TotalSeconds;

        return new SevkException(message, status, code, fields, retryAfter, content, details);
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> ParseFields(JsonElement root)
    {
        var result = new Dictionary<string, IReadOnlyList<string>>();

        if (!root.TryGetProperty("fields", out var f) || f.ValueKind != JsonValueKind.Object)
            return result;

        foreach (var prop in f.EnumerateObject())
        {
            var list = new List<string>();

            if (prop.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in prop.Value.EnumerateArray())
                    list.Add(item.ValueKind == JsonValueKind.String ? item.GetString() ?? "" : item.GetRawText());
            }
            else
            {
                list.Add(prop.Value.ValueKind == JsonValueKind.String ? prop.Value.GetString() ?? "" : prop.Value.GetRawText());
            }
            result[prop.Name] = list;
        }

        return result;
    }

    /// <summary>Releases connections held by the underlying HTTP client.</summary>
    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
