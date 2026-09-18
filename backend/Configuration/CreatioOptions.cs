namespace CreatioAccounts.Api.Configuration;

public class CreatioOptions
{
    public const string SectionName = "Creatio";

    public string InstanceUrl { get; set; } = "";
    public string TokenUrl { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string ODataBasePath { get; set; } = "/0/odata";
}