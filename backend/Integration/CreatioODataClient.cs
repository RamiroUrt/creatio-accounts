using System.Net;
using System.Net.Http.Headers;
using CreatioAccounts.Api.Configuration;

namespace CreatioAccounts.Api.Integration;

public interface ICreatioODataClient
{
    Task<string> GetStringAsync(string url, CancellationToken ct = default);
    Task<string> PostStringAsync(string url, string jsonBody, CancellationToken ct = default);
}

public class CreatioODataClient : ICreatioODataClient
{
    private readonly HttpClient _http;
    private readonly CreatioOptions _options;
    private readonly ICreatioTokenService _tokens;

    public CreatioODataClient(IHttpClientFactory httpFactory, CreatioOptions options, ICreatioTokenService tokens)
    {
        _http = httpFactory.CreateClient("creatio");
        _options = options;
        _tokens = tokens;
    }

    public async Task<string> GetStringAsync(string url, CancellationToken ct = default)
    {
        using var request = CreateJsonRequest(HttpMethod.Get, url);
        return await SendAsync(request, ct);
    }

    public async Task<string> PostStringAsync(string url, string jsonBody, CancellationToken ct = default)
    {
        using var request = CreateJsonRequest(HttpMethod.Post, url);
        request.Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
        return await SendAsync(request, ct);
    }

    private async Task<string> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _tokens.GetAccessTokenAsync(ct));
        using var response = await _http.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await _tokens.InvalidateAsync(ct);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _tokens.GetAccessTokenAsync(ct));
            response.Dispose();
            using var retry = await _http.SendAsync(request, ct);
            return await ReadOrThrowAsync(retry, ct);
        }

        return await ReadOrThrowAsync(response, ct);
    }

    private HttpRequestMessage CreateJsonRequest(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, BuildUrl(url));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }

    private string BuildUrl(string url)
        => url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? url
            : $"{_options.InstanceUrl.TrimEnd('/')}{url}";

    private static async Task<string> ReadOrThrowAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var payload = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new CreatioApiException(
                (int)response.StatusCode,
                $"Creatio devolvió un error ({(int)response.StatusCode}). Detalle: {Truncate(payload)}");
        }
        return payload;
    }

    private static string Truncate(string value, int max = 500)
        => value.Length <= max ? value : value[..max] + "...";
}