using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBCRM2.Infrastructure.Services;

namespace PBCRM2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantTestController : ControllerBase
{
    private readonly ITenantDbContextFactory _tenantDbFactory;

    public TenantTestController(
        ITenantDbContextFactory tenantDbFactory)
    {
        _tenantDbFactory = tenantDbFactory;
    }

    [HttpGet("{companyId:int}")]
    public async Task<IActionResult> TestTenantDatabase(
        int companyId)
    {
        try
        {
            await using var db =
                await _tenantDbFactory.CreateAsync(companyId);

            var canConnect =
                await db.Database.CanConnectAsync();

            if (!canConnect)
            {
                return StatusCode(
                    500,
                    new
                    {
                        connected = false,
                        message =
                            "Could not connect to the tenant database."
                    });
            }

            var tenantCount =
                await db.Tenants.CountAsync();

            return Ok(
                new
                {
                    connected = true,
                    companyId = companyId,
                    tenantDatabase = "Connected",
                    tenantCount = tenantCount
                });
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                new
                {
                    connected = false,
                    companyId = companyId,
                    error = ex.Message
                });
        }
    }
}