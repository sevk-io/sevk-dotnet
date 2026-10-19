using System.Text.Json.Serialization;

namespace Sevk.Types;

/// <summary>non-dns domain metadata returned by list operations.</summary>
public class DomainSummary
{
    public string Id { get; set; } = "";
    [JsonPropertyName("domain")]
    public string DomainName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? SenderName { get; set; }
    public string Region { get; set; } = "";
    public bool Verified { get; set; }
    public bool ClickTracking { get; set; }
    public bool OpenTracking { get; set; }
    [JsonPropertyName("tlsPolicy")]
    public TlsPolicy? TlsPolicy { get; set; }
    [JsonPropertyName("inboundEnabled")]
    public bool? InboundEnabled { get; set; }
    [JsonPropertyName("inboundVerified")]
    public bool? InboundVerified { get; set; }
    public string InboundPrefix { get; set; } = "";
    public string CreatedAt { get; set; } = "";
    public string UpdatedAt { get; set; } = "";
    [JsonPropertyName("projectId")]
    public string ProjectId { get; set; } = "";
}

/// <summary>domain metadata with capability-gated dns material.</summary>
public class DomainRead : DomainSummary
{
    [JsonPropertyName("verificationToken")]
    public string? VerificationToken { get; set; }
    public List<string>? DkimTokens { get; set; }
    public List<DnsRecord>? DnsRecords { get; set; }
}

public class InboundMxRecord
{
    [JsonPropertyName("exchange")]
    public string Exchange { get; set; } = "";

    [JsonPropertyName("priority")]
    public int Priority { get; set; }
}

public class InboundVerifyResult : DomainRead
{
    [JsonPropertyName("inboundMxResolved")]
    public List<InboundMxRecord> InboundMxResolved { get; set; } = new();

    [JsonPropertyName("inboundExpectedTarget")]
    public string InboundExpectedTarget { get; set; } = "";
}

public class DomainsResponse : ListResponse<DomainSummary>
{
}

public class DnsRecord
{
    public DnsRecordType Type { get; set; }
    public string Name { get; set; } = "";
    public string Fqdn { get; set; } = "";
    public string Value { get; set; } = "";
}

public class DnsRecordsResponse
{
    public List<DnsRecord> Items { get; set; } = new();
}

public class CreateDomainRequest
{
    [JsonPropertyName("domain")]
    public string Domain { get; set; } = "";
    public string Email { get; set; } = "";
    public string? SenderName { get; set; }
    public string? Region { get; set; }
}

public class UpdateDomainRequest
{
    public string? InboundPrefix { get; set; }
    public string? Email { get; set; }
    public string? SenderName { get; set; }
    public bool? ClickTracking { get; set; }
    public bool? OpenTracking { get; set; }

    [JsonPropertyName("tlsPolicy")]
    public TlsPolicy? TlsPolicy { get; set; }

    [JsonPropertyName("inboundEnabled")]
    public bool? InboundEnabled { get; set; }
}

public class RegionsResponse
{
    public List<string> Items { get; set; } = new();
}

public class ListDomainsParams
{
    public int? Page { get; set; }
    public int? Limit { get; set; }
    public string? After { get; set; }
    public string? Before { get; set; }
    public bool? Verified { get; set; }
}
