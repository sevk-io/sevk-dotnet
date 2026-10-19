using System.Text.Json.Serialization;

namespace Sevk.Types;
/// <summary>
/// Shape varies by pagination mode: offset (?page=) sets Total, Page and
/// TotalPages; cursor (default or ?after=/?before=) sets HasMore, NextCursor and
/// PrevCursor. Items is always present.
/// </summary>
public class ListResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int? Total { get; set; }
    public int? Page { get; set; }
    public int? TotalPages { get; set; }
    [JsonPropertyName("hasMore")]
    public bool? HasMore { get; set; }
    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; set; }

    [JsonPropertyName("prevCursor")]
    public string? PrevCursor { get; set; }
}

public class PaginationParams
{
    public int? Page { get; set; }
    public int? Limit { get; set; }
    /// <summary>
    /// Cursor: id of the last item from the previous page. Mutually
    /// exclusive with <c>Page</c>. See the Pagination doc for the
    /// response shape per mode.
    /// </summary>
    public string? After { get; set; }
    /// <summary>
    /// Cursor for backward pagination: id of the first item from the
    /// current page. Pass the id of the top item to fetch the previous
    /// page. Mutually exclusive with <c>Page</c> and <c>After</c>. See
    /// the Pagination doc for the response shape per mode.
    /// </summary>
    public string? Before { get; set; }
}

public class ListParams : PaginationParams
{
    public string? Search { get; set; }
}

public class ListContactsParams : ListParams
{
    public string? ExcludeAudience { get; set; }
    public bool? Subscribed { get; set; }
}

public class ListAudienceContactsParams : ListParams
{
    public bool? Subscribed { get; set; }
    public string? ExcludeTopicId { get; set; }
}
