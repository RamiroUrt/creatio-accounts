using System.Text.Json;
using CreatioAccounts.Api.Configuration;
using CreatioAccounts.Api.Contracts.Accounts;
using CreatioAccounts.Api.Contracts.AccountTypes;
using CreatioAccounts.Api.Integration;

namespace CreatioAccounts.Api.Services;

public interface ICreatioODataService
{
    Task<AccountListResponse> GetAccountsAsync(string? search, string? typeName, int page, int pageSize, CancellationToken ct = default);
    Task<AccountItem> CreateAccountAsync(string name, Guid? typeId, CancellationToken ct = default);
    Task<List<AccountType>> GetAccountTypesAsync(CancellationToken ct = default);
}

public class CreatioODataService : ICreatioODataService
{
    private readonly CreatioOptions _options;
    private readonly ICreatioODataClient _client;

    public CreatioODataService(CreatioOptions options, ICreatioODataClient client)
    {
        _options = options;
        _client = client;
    }

    public async Task<AccountListResponse> GetAccountsAsync(string? search, string? typeName, int page, int pageSize, CancellationToken ct = default)
    {
        var query = new List<string>
        {
            "$select=Id,Name,TypeId,Type",
            "$expand=Type",
            "$count=true",
            "$orderby=Name",
            $"$top={pageSize}",
            $"$skip={(page - 1) * pageSize}",
        };

        var filters = new List<string>();
        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add($"contains(Name,'{EscapeODataFilterValue(search.Trim())}')");
        }
        if (!string.IsNullOrWhiteSpace(typeName))
        {
            filters.Add($"Type/Name eq '{EscapeODataFilterValue(typeName.Trim())}'");
        }
        if (filters.Count > 0)
        {
            query.Insert(0, $"$filter={string.Join(" and ", filters)}");
        }

        var url = $"{_options.ODataBasePath}/Account?{string.Join("&", query)}";
        var payload = await _client.GetStringAsync(url, ct);

        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;

        var total = root.TryGetProperty("@odata.count", out var countEl)
            ? countEl.GetInt64()
            : root.TryGetProperty("value", out var v) ? v.GetArrayLength() : 0L;

        var items = new List<AccountItem>();
        if (root.TryGetProperty("value", out var values))
        {
            foreach (var element in values.EnumerateArray())
            {
                items.Add(ParseAccount(element));
            }
        }

        return new AccountListResponse
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<List<AccountType>> GetAccountTypesAsync(CancellationToken ct = default)
    {
        var url = $"{_options.ODataBasePath}/AccountType?$select=Id,Name&$orderby=Name";
        var payload = await _client.GetStringAsync(url, ct);

        var types = new List<AccountType>();
        using var doc = JsonDocument.Parse(payload);
        if (doc.RootElement.TryGetProperty("value", out var values))
        {
            foreach (var element in values.EnumerateArray())
            {
                types.Add(new AccountType
                {
                    Id = ParseGuid(element, "Id") ?? Guid.Empty,
                    Name = element.TryGetProperty("Name", out var nameEl) ? nameEl.GetString() ?? "" : "",
                });
            }
        }
        return types;
    }

    public async Task<AccountItem> CreateAccountAsync(string name, Guid? typeId, CancellationToken ct = default)
    {
        var body = new Dictionary<string, object?> { ["Name"] = name };
        if (typeId.HasValue)
        {
            body["TypeId"] = typeId.Value;
        }

        var url = $"{_options.ODataBasePath}/Account";
        var payload = await _client.PostStringAsync(url, JsonSerializer.Serialize(body), ct);

        using var doc = JsonDocument.Parse(payload);
        return ParseAccount(doc.RootElement);
    }

    private static AccountItem ParseAccount(JsonElement element)
    {
        var item = new AccountItem
        {
            Id = ParseGuid(element, "Id") ?? Guid.Empty,
            Name = element.TryGetProperty("Name", out var nameEl) ? nameEl.GetString() ?? "" : "",
        };

        if (element.TryGetProperty("TypeId", out var typeIdEl) && typeIdEl.ValueKind == JsonValueKind.String)
        {
            item.TypeId = ParseGuid(element, "TypeId");
        }

        if (element.TryGetProperty("Type", out var typeEl) && typeEl.ValueKind == JsonValueKind.Object
            && typeEl.TryGetProperty("Name", out var typeNameEl))
        {
            item.TypeName = typeNameEl.GetString();
        }

        return item;
    }

    private static Guid? ParseGuid(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var el) || el.ValueKind != JsonValueKind.String)
        {
            return null;
        }
        return Guid.TryParse(el.GetString(), out var id) ? id : null;
    }

    private static string EscapeODataFilterValue(string value)
        => value.Replace("'", "''");
}