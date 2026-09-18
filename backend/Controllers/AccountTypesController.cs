using CreatioAccounts.Api.Contracts.AccountTypes;
using CreatioAccounts.Api.Integration;
using CreatioAccounts.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CreatioAccounts.Api.Controllers;

[ApiController]
[Route("api/account-types")]
public class AccountTypesController : ControllerBase
{
    private readonly ICreatioODataService _creatio;
    private readonly ILogger<AccountTypesController> _logger;

    public AccountTypesController(ICreatioODataService creatio, ILogger<AccountTypesController> logger)
    {
        _creatio = creatio;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<AccountType>>> Get(CancellationToken ct = default)
    {
        try
        {
            var types = await _creatio.GetAccountTypesAsync(ct);
            return Ok(types);
        }
        catch (CreatioApiException ex)
        {
            _logger.LogWarning(ex, "Error al consultar tipos de cuenta de Creatio.");
            return StatusCode(ex.StatusCode == 0 ? 502 : ex.StatusCode,
                new { error = new { code = ex.StatusCode, message = ex.Message } });
        }
    }
}