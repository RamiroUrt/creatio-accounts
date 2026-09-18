using CreatioAccounts.Api.Contracts.Accounts;
using CreatioAccounts.Api.Integration;
using CreatioAccounts.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CreatioAccounts.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly ICreatioODataService _creatio;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(ICreatioODataService creatio, ILogger<AccountsController> logger)
    {
        _creatio = creatio;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<AccountListResponse>> Get(
        [FromQuery] string? search,
        [FromQuery] string? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        try
        {
            var result = await _creatio.GetAccountsAsync(search, type, page, pageSize, ct);
            return Ok(result);
        }
        catch (CreatioApiException ex)
        {
            _logger.LogWarning(ex, "Error al consultar cuentas de Creatio.");
            return StatusCode(ex.StatusCode == 0 ? 502 : ex.StatusCode, ErrorResponse(ex.StatusCode, ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<AccountItem>> Create(
        [FromBody] CreateAccountRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(ErrorResponse(400, "El nombre de la cuenta es obligatorio."));
        }

        try
        {
            var created = await _creatio.CreateAccountAsync(request.Name.Trim(), request.TypeId, ct);
            return StatusCode(201, created);
        }
        catch (CreatioApiException ex)
        {
            _logger.LogWarning(ex, "Error al crear cuenta en Creatio.");
            return StatusCode(ex.StatusCode, ErrorResponse(ex.StatusCode, ex.Message));
        }
    }

    private static object ErrorResponse(int statusCode, string message)
        => new { error = new { code = statusCode, message } };
}