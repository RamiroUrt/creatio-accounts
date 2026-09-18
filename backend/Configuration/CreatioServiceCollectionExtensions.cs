using CreatioAccounts.Api.Integration;
using CreatioAccounts.Api.Services;

namespace CreatioAccounts.Api.Configuration;

public static class CreatioServiceCollectionExtensions
{
    public static IServiceCollection AddCreatio(this IServiceCollection services, IConfiguration configuration)
    {
        var creatioSection = configuration.GetSection(CreatioOptions.SectionName);
        services.Configure<CreatioOptions>(creatioSection);

        var options = new CreatioOptions();
        creatioSection.Bind(options);
        Validate(options);
        services.AddSingleton(options);

        services.AddHttpClient("creatio");
        services.AddSingleton<ICreatioTokenService, CreatioTokenService>();
        services.AddSingleton<ICreatioODataClient, CreatioODataClient>();
        services.AddSingleton<ICreatioODataService, CreatioODataService>();

        return services;
    }

    private static void Validate(CreatioOptions options)
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(options.InstanceUrl)) missing.Add("CREATIO_INSTANCE_URL");
        if (string.IsNullOrWhiteSpace(options.TokenUrl)) missing.Add("CREATIO_TOKEN_URL");
        if (string.IsNullOrWhiteSpace(options.ClientId)) missing.Add("CREATIO_CLIENT_ID");
        if (string.IsNullOrWhiteSpace(options.ClientSecret)) missing.Add("CREATIO_CLIENT_SECRET");

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                "Faltan valores de configuración de Creatio. Definalos en un archivo .env (ver .env.example) o como variables de entorno: " +
                string.Join(", ", missing));
        }
    }
}