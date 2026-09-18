namespace CreatioAccounts.Api.Contracts.Accounts;

public class AccountItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public Guid? TypeId { get; set; }
    public string? TypeName { get; set; }
}