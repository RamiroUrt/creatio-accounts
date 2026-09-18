using System.Text.Json;
using CreatioAccounts.Api.Integration;

namespace CreatioAccounts.Api.Infrastructure.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (CreatioApiException ex)
        {
            _logger.LogWarning(ex, "Error controlado al comunicarse con Creatio.");
            await WriteJsonAsync(context, ex.StatusCode is >= 400 and < 600 ? ex.StatusCode : 502,
                new { error = new { code = ex.StatusCode, message = ex.Message } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado.");
            await WriteJsonAsync(context, 500,
                new { error = new { code = 500, message = "Ocurrió un error interno en el servidor." } });
        }
    }

    private static async Task WriteJsonAsync(HttpContext context, int statusCode, object body)
    {
        if (!context.Response.HasStarted)
        {
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(body));
        }
    }
}