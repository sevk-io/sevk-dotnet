using System.Text.Json;
using Sevk.Types;

namespace Sevk.Resources;

internal sealed class QueryParameters
{
    private readonly List<string> _values = new();

    public QueryParameters Add(string name, string? value)
    {
        if (value is not null)
            _values.Add($"{name}={Uri.EscapeDataString(value)}");

        return this;
    }

    public QueryParameters Add(string name, int? value)
    {
        if (value.HasValue)
            _values.Add($"{name}={value.Value}");

        return this;
    }

    public QueryParameters Add(string name, bool? value)
    {
        if (value.HasValue)
            _values.Add($"{name}={(value.Value ? "true" : "false")}");

        return this;
    }

    public QueryParameters Add<TEnum>(string name, TEnum? value)
        where TEnum : struct, Enum
    {
        if (value.HasValue)
        {
            using var serialized = JsonDocument.Parse(JsonSerializer.Serialize(value.Value));
            var text = serialized.RootElement.ValueKind == JsonValueKind.String
                ? serialized.RootElement.GetString()!
                : serialized.RootElement.GetRawText();
            _values.Add($"{name}={Uri.EscapeDataString(text)}");
        }

        return this;
    }

    public QueryParameters AddPagination(
        ListParams parameters,
        bool includeSearch = true)
    {
        AddPagination(parameters.Page, parameters.Limit, parameters.After, parameters.Before);

        if (includeSearch)
            Add("search", parameters.Search);

        return this;
    }

    public QueryParameters AddPagination(PaginationParams parameters)
    {
        AddPagination(parameters.Page, parameters.Limit, parameters.After, parameters.Before);
        return this;
    }

    public QueryParameters AddPagination(
        int? page,
        int? limit,
        string? after,
        string? before)
    {
        Add("page", page);
        Add("after", after);
        Add("before", before);
        Add("limit", limit);

        return this;
    }

    public string Build(bool includeQuestionMarkWhenEmpty = false) =>
        _values.Count == 0
            ? includeQuestionMarkWhenEmpty ? "?" : ""
            : $"?{string.Join("&", _values)}";
}
