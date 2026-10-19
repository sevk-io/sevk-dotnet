using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Sevk;

/// <summary>
/// Thrown when a webhook delivery fails verification.
/// </summary>
public class WebhookVerificationException : Exception
{
    public WebhookVerificationException(string message) : base(message) { }
    public WebhookVerificationException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Verifier for sevk webhook deliveries.
///
/// sevk uses the Svix-compatible signature scheme (svix-id, svix-timestamp,
/// svix-signature headers), so this helper or the official Svix .NET SDK can
/// be used. sevk issues canonical whsec_ base64 secrets that can be passed
/// directly to either verifier.
/// </summary>
public class WebhookVerifier
{
    private const int ToleranceSeconds = 5 * 60;

    private readonly byte[] _key;

    /// <summary>Prepares a verifier with the signing secret issued for a webhook.</summary>
    public WebhookVerifier(string secret)
    {
        if (string.IsNullOrEmpty(secret))
            throw new WebhookVerificationException("Webhook secret is required");

        if (secret.StartsWith("whsec_"))
        {
            try
            {
                _key = Convert.FromBase64String(secret.Substring("whsec_".Length));
                if (_key.Length == 0)
                    throw new WebhookVerificationException("Invalid base64 secret");
            }
            catch (FormatException e)
            {
                throw new WebhookVerificationException("Invalid base64 secret", e);
            }
        }
        else
        {
            _key = Encoding.UTF8.GetBytes(secret);
        }
    }

    /// <summary>
    /// Verifies the delivery signature and timestamp, then parses its JSON payload.
    /// </summary>
    public JsonDocument Verify(string payload, IDictionary<string, string?> headers)
    {
        var msgId = Lookup(headers, "svix-id");
        var msgTimestamp = Lookup(headers, "svix-timestamp");
        var msgSignature = Lookup(headers, "svix-signature");

        if (string.IsNullOrEmpty(msgId) || msgTimestamp is null || msgSignature is null)
            throw new WebhookVerificationException("Missing required svix headers");

        if (!IsAsciiDigits(msgTimestamp) || !long.TryParse(msgTimestamp, out var timestamp))
            throw new WebhookVerificationException("Invalid timestamp header");

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (timestamp < now - ToleranceSeconds || timestamp > now + ToleranceSeconds)
            throw new WebhookVerificationException("Timestamp outside tolerance window");

        var toSign = $"{msgId}.{msgTimestamp}.{payload}";
        byte[] digest;
        using (var hmac = new HMACSHA256(_key))
        {
            digest = hmac.ComputeHash(Encoding.UTF8.GetBytes(toSign));
        }

        var expected = "v1," + Convert.ToBase64String(digest);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);

        foreach (var candidate in msgSignature.Split(' '))
        {
            var candidateBytes = Encoding.UTF8.GetBytes(candidate);

            if (CryptographicOperations.FixedTimeEquals(candidateBytes, expectedBytes))
            {
                try
                {
                    return JsonDocument.Parse(payload);
                }
                catch (JsonException e)
                {
                    throw new WebhookVerificationException("Invalid JSON payload", e);
                }
            }
        }

        throw new WebhookVerificationException("Signature mismatch");
    }

    private static string? Lookup(IDictionary<string, string?> headers, string name)
    {
        foreach (var kvp in headers)
        {
            var normalized = kvp.Key.Replace('_', '-');
            if (normalized.StartsWith("HTTP-", StringComparison.OrdinalIgnoreCase))
                normalized = normalized[5..];
            if (string.Equals(normalized, name, StringComparison.OrdinalIgnoreCase) && kvp.Value is not null)
                return kvp.Value;
        }

        return null;
    }

    private static bool IsAsciiDigits(string value)
    {
        if (value.Length == 0) return false;
        foreach (var current in value)
        {
            if (current < '0' || current > '9') return false;
        }
        return true;
    }
}
