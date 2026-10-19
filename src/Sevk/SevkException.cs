using System.Text.Json;

namespace Sevk;

/// <summary>
/// Exception thrown by sevk SDK
/// </summary>
public class SevkException : Exception
{
    public int StatusCode { get; }
    public string Code { get; }

    /// <summary>Field-level validation errors (422), field name -> messages.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Fields { get; }

    /// <summary>Seconds to wait before retrying, from the Retry-After header on a 429.</summary>
    public int? RetryAfter { get; }
    public string? ResponseBody { get; }
    public JsonElement? Details { get; }

    public SevkException(
        string message,
        int statusCode = 0,
        string code = "UNKNOWN_ERROR",
        IReadOnlyDictionary<string, IReadOnlyList<string>>? fields = null,
        int? retryAfter = null,
        string? responseBody = null,
        JsonElement? details = null,
        Exception? innerException = null) : base(message, innerException)
    {
        StatusCode = statusCode;
        Code = code;
        Fields = fields ?? new Dictionary<string, IReadOnlyList<string>>();
        RetryAfter = retryAfter;
        ResponseBody = responseBody;
        Details = details;
    }

    public bool IsNotFound => StatusCode == 404;
    public bool IsUnauthorized => StatusCode == 401;
    public bool IsForbidden => StatusCode == 403;
    public bool IsConflict => StatusCode == 409;
    public bool IsBadRequest => StatusCode == 400;
    public bool IsValidationError =>
        StatusCode == 422 ||
        (StatusCode == 400 && (Code == "VALIDATION_ERROR" || Fields.Count > 0));
    public bool IsRateLimited => StatusCode == 429;
    public bool IsServerError => StatusCode >= 500 && StatusCode < 600;
}
