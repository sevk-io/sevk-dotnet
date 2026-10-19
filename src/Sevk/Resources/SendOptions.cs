namespace Sevk.Resources;

/// <summary>Per-call options for transactional email requests.</summary>
public class SendOptions
{
    public string? IdempotencyKey { get; set; }
}
