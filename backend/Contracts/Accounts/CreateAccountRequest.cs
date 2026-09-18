namespace CreatioAccounts.Api.Contracts.Accounts;

public class CreateAccountRequest
{
    public string Name { get; set; } = "";
    public Guid? TypeId { get; set; }
}