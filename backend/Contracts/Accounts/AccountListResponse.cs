namespace CreatioAccounts.Api.Contracts.Accounts;

public class AccountListResponse
{
    public List<AccountItem> Items { get; set; } = new();
    public long Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}