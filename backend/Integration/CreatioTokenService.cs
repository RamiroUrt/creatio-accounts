using System.Text.Json;
using CreatioAccounts.Api.Configuration;

namespace CreatioAccounts.Api.Integration;

public interface ICreatioTokenService
{
    Task<string> GetAccessTokenAsync(CancellationToken ct = default);
    Task InvalidateAsync(CancellationToken ct = default);
}

public class CreatioTokenService : ICreatioTokenService
{
    private static readonly TimeSpan RefreshGrace = TimeSpan.FromSeconds(60);

    private readonly HttpClient _http;
    private readonly CreatioOptions _options;
    private readonly ILogger<CreatioTokenService> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private string? _accessToken;
    private DateTimeOffset _expiresAtUtc = DateTimeOffset.MinValue;

    public CreatioTokenService(IHttpClientFactory httpFactory, CreatioOptions options, ILogger<CreatioTokenService> logger)
    {
        _http = httpFactory.CreateClient("creatio");
        _options = options;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (IsCurrent())
        {
            return _accessToken!;
        }

        await _gate.WaitAsync(ct);
        try
        {
            if (IsCurrent())
            {
                return _accessToken!;
            }

            await RequestTokenAsync(ct);
            return _accessToken!;
        }
        finally
        {
            _gate.Release();
        }
    }

    public Task InvalidateAsync(CancellationToken ct = default)
    {
        _accessToken = null;
        _expiresAtUtc = DateTimeOffset.MinValue;
        return Task.CompletedTask;
    }

    private bool IsCurrent()
    {
        return !string.IsNullOrEmpty(_accessToken)
            && _expiresAtUtc > DateTimeOffset.UtcNow + RefreshGrace;
    }

    private async Task RequestTokenAsync(CancellationToken ct)
    {
        var body = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenUrl)
        {
            Content = new FormUrlEncodedContent(body),
        };

        using var response = await _http.SendAsync(request, ct);
        var payload = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new CreatioApiException(
                (int)response.StatusCode,
                $"No se pudo obtener el token de acceso en Creatio ({(int)response.StatusCode}). Revise ClientId/ClientSecret y el estado de la integración OAuth.");
        }

        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;
        _accessToken = root.GetProperty("access_token").GetString();
        var expiresIn = root.GetProperty("expires_in").GetInt32();
        _expiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(Math.Max(0, expiresIn));

        _logger.LogInformation("Token de Creatio renovado. Expiracion en {Seconds}s.", expiresIn);
    }
}