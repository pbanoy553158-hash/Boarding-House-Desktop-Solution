using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBCRM2.Domain.Entities;
using PBCRM2.Infrastructure.Identity;
using PBCRM2.Infrastructure.Services;
using System.Security.Claims;

namespace PBCRM2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Manager,Staff")]
public class MaintenanceController : ControllerBase
{
    private readonly ITenantDbContextFactory _tenantDbFactory;
    private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;

    public MaintenanceController(
        ITenantDbContextFactory tenantDbFactory,
        Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
    {
        _tenantDbFactory = tenantDbFactory;
        _userManager = userManager;
    }

    // =========================================================
    // GET CURRENT COMPANY ID
    // =========================================================

    private bool TryGetCompanyId(out int companyId)
    {
        companyId = 0;

        var companyIdValue =
            User.FindFirst("CompanyId")?.Value;

        return int.TryParse(
                   companyIdValue,
                   out companyId)
               && companyId > 0;
    }

    // =========================================================
    // GET CURRENT USER
    // =========================================================

    private async Task<ApplicationUser?> CurrentUserAsync()
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await _userManager
            .FindByIdAsync(userId);
    }

    // =========================================================
    // GET ALL
    // MANAGER / STAFF
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> GetMaintenanceRequests()
    {
        var user = await CurrentUserAsync();

        if (user == null)
        {
            return Unauthorized();
        }

        if (!TryGetCompanyId(out var companyId))
        {
            return BadRequest(new
            {
                message =
                    "Your account is not assigned to a company."
            });
        }

        if (!user.BranchId.HasValue)
        {
            return BadRequest(new
            {
                message =
                    "Your account is not assigned to a branch."
            });
        }

        await using var db =
            await _tenantDbFactory
                .CreateAsync(companyId);

        var requests = await db.MaintenanceRequests
            .AsNoTracking()
            .Include(m => m.Tenant)
            .Where(m =>
                m.Tenant != null &&
                m.Tenant.BranchId ==
                user.BranchId.Value)
            .OrderByDescending(m => m.DateReported)
            .Select(m => new
            {
                m.Id,
                m.TenantId,

                TenantName =
                    m.Tenant != null
                        ? m.Tenant.FullName
                        : "Unknown Tenant",

                m.Title,
                m.Description,
                m.Priority,
                m.Status,
                m.DateReported,
                m.DateResolved,
                m.ResolutionNotes
            })
            .ToListAsync();

        return Ok(requests);
    }

    // =========================================================
    // GET BY ID
    // MANAGER / STAFF
    // =========================================================

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMaintenanceRequest(
        int id)
    {
        var user = await CurrentUserAsync();

        if (user == null)
        {
            return Unauthorized();
        }

        if (!TryGetCompanyId(out var companyId))
        {
            return BadRequest(new
            {
                message =
                    "Your account is not assigned to a company."
            });
        }

        if (!user.BranchId.HasValue)
        {
            return BadRequest(new
            {
                message =
                    "Your account is not assigned to a branch."
            });
        }

        await using var db =
            await _tenantDbFactory
                .CreateAsync(companyId);

        var request = await db.MaintenanceRequests
            .AsNoTracking()
            .Include(m => m.Tenant)
            .Where(m =>
                m.Id == id &&
                m.Tenant != null &&
                m.Tenant.BranchId ==
                user.BranchId.Value)
            .Select(m => new
            {
                m.Id,
                m.TenantId,

                TenantName =
                    m.Tenant != null
                        ? m.Tenant.FullName
                        : "Unknown Tenant",

                m.Title,
                m.Description,
                m.Priority,
                m.Status,
                m.DateReported,
                m.DateResolved,
                m.ResolutionNotes
            })
            .FirstOrDefaultAsync();

        if (request == null)
        {
            return NotFound(new
            {
                message =
                    "Maintenance request not found."
            });
        }

        return Ok(request);
    }

    // =========================================================
    // GET BY TENANT
    // MANAGER / STAFF
    // =========================================================

    [HttpGet("tenant/{tenantId:int}")]
    public async Task<IActionResult> GetTenantMaintenanceRequests(
        int tenantId)
    {
        var user = await CurrentUserAsync();

        if (user == null)
        {
            return Unauthorized();
        }

        if (!TryGetCompanyId(out var companyId))
        {
            return BadRequest(new
            {
                message =
                    "Your account is not assigned to a company."
            });
        }

        if (!user.BranchId.HasValue)
        {
            return BadRequest(new
            {
                message =
                    "Your account is not assigned to a branch."
            });
        }

        await using var db =
            await _tenantDbFactory
                .CreateAsync(companyId);

        // -----------------------------------------------------
        // CHECK TENANT AND BRANCH
        // -----------------------------------------------------

        var tenantExists =
            await db.Tenants
                .AsNoTracking()
                .AnyAsync(t =>
                    t.Id == tenantId &&
                    t.BranchId ==
                    user.BranchId.Value);

        if (!tenantExists)
        {
            return NotFound(new
            {
                message =
                    "Tenant not found."
            });
        }

        // -----------------------------------------------------
        // GET MAINTENANCE REQUESTS
        // -----------------------------------------------------

        var requests =
            await db.MaintenanceRequests
                .AsNoTracking()
                .Include(m => m.Tenant)
                .Where(m =>
                    m.TenantId == tenantId &&
                    m.Tenant != null &&
                    m.Tenant.BranchId ==
                    user.BranchId.Value)
                .OrderByDescending(
                    m => m.DateReported)
                .Select(m => new
                {
                    m.Id,
                    m.TenantId,

                    TenantName =
                        m.Tenant != null
                            ? m.Tenant.FullName
                            : "Unknown Tenant",

                    m.Title,
                    m.Description,
                    m.Priority,
                    m.Status,
                    m.DateReported,
                    m.DateResolved,
                    m.ResolutionNotes
                })
                .ToListAsync();

        return Ok(requests);
    }

    // =========================================================
    // POST
    // STAFF / MANAGER RECORDS A REQUEST
    // =========================================================

    [HttpPost]
    public async Task<IActionResult> CreateMaintenanceRequest(
        [FromBody] MaintenanceRequest request)
    {
        var user = await CurrentUserAsync();

        if (user == null)
        {
            return Unauthorized();
        }

        if (!TryGetCompanyId(out var companyId))
        {
            return BadRequest(new
            {
                message =
                    "Your account is not assigned to a company."
            });
        }

        if (!user.BranchId.HasValue)
        {
            return BadRequest(new
            {
                message =
                    "Your account is not assigned to a branch."
            });
        }

        // -----------------------------------------------------
        // VALIDATE TENANT ID
        // -----------------------------------------------------

        if (request.TenantId <= 0)
        {
            return BadRequest(new
            {
                message =
                    "A valid tenant is required."
            });
        }

        // -----------------------------------------------------
        // VALIDATE TITLE
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new
            {
                message =
                    "Maintenance title is required."
            });
        }

        // -----------------------------------------------------
        // VALIDATE DESCRIPTION
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest(new
            {
                message =
                    "Maintenance description is required."
            });
        }

        await using var db =
            await _tenantDbFactory
                .CreateAsync(companyId);

        // -----------------------------------------------------
        // CHECK TENANT
        // -----------------------------------------------------

        var tenantExists =
            await db.Tenants
                .AsNoTracking()
                .AnyAsync(t =>
                    t.Id == request.TenantId &&
                    t.BranchId ==
                    user.BranchId.Value &&
                    t.ActualMoveOutDate == null &&
                    t.Status == "Active");

        if (!tenantExists)
        {
            return BadRequest(new
            {
                message =
                    "The selected tenant does not exist, is inactive, or does not belong to your branch."
            });
        }

        // -----------------------------------------------------
        // SET CONTROLLED VALUES
        // -----------------------------------------------------

        request.Id = 0;

        request.Title =
            request.Title.Trim();

        request.Description =
            request.Description.Trim();

        if (string.IsNullOrWhiteSpace(request.Priority))
        {
            request.Priority = "Normal";
        }
        else
        {
            request.Priority =
                request.Priority.Trim();
        }

        request.Status =
            "Pending";

        request.DateReported =
            DateTime.UtcNow;

        request.DateResolved =
            null;

        request.ResolutionNotes =
            null;

        // -----------------------------------------------------
        // SAVE
        // -----------------------------------------------------

        db.MaintenanceRequests.Add(request);

        await db.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Maintenance request recorded successfully.",

            id =
                request.Id
        });
    }

    // =========================================================
    // PUT
    // MANAGER ONLY
    // =========================================================

    [Authorize(Roles = "Manager")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateMaintenanceRequest(
        int id,
        [FromBody] MaintenanceUpdateRequest model)
    {
        var user = await CurrentUserAsync();

        if (user == null)
        {
            return Unauthorized();
        }

        if (!TryGetCompanyId(out var companyId))
        {
            return BadRequest(new
            {
                message =
                    "Your account is not assigned to a company."
            });
        }

        if (!user.BranchId.HasValue)
        {
            return BadRequest(new
            {
                message =
                    "Your account is not assigned to a branch."
            });
        }

        await using var db =
            await _tenantDbFactory
                .CreateAsync(companyId);

        // -----------------------------------------------------
        // FIND REQUEST IN MANAGER'S BRANCH
        // -----------------------------------------------------

        var request =
            await db.MaintenanceRequests
                .Include(m => m.Tenant)
                .FirstOrDefaultAsync(m =>
                    m.Id == id &&
                    m.Tenant != null &&
                    m.Tenant.BranchId ==
                    user.BranchId.Value);

        if (request == null)
        {
            return NotFound(new
            {
                message =
                    "Maintenance request not found."
            });
        }

        // -----------------------------------------------------
        // VALIDATE STATUS
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(model.Status))
        {
            return BadRequest(new
            {
                message =
                    "Status is required."
            });
        }

        var allowedStatuses =
            new[]
            {
                "Pending",
                "In Progress",
                "Resolved"
            };

        var status =
            model.Status.Trim();

        if (!allowedStatuses.Contains(status))
        {
            return BadRequest(new
            {
                message =
                    "Invalid maintenance status."
            });
        }

        // -----------------------------------------------------
        // VALIDATE PRIORITY
        // -----------------------------------------------------

        var priority =
            string.IsNullOrWhiteSpace(
                model.Priority)
                    ? "Normal"
                    : model.Priority.Trim();

        // -----------------------------------------------------
        // UPDATE
        // -----------------------------------------------------

        request.Priority =
            priority;

        request.Status =
            status;

        request.ResolutionNotes =
            string.IsNullOrWhiteSpace(
                model.ResolutionNotes)
                    ? null
                    : model.ResolutionNotes.Trim();

        // -----------------------------------------------------
        // RESOLUTION DATE
        // -----------------------------------------------------

        if (status == "Resolved")
        {
            request.DateResolved ??=
                DateTime.UtcNow;
        }
        else
        {
            request.DateResolved =
                null;

            request.ResolutionNotes =
                null;
        }

        await db.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Maintenance request updated successfully."
        });
    }
}

// =============================================================
// MANAGER UPDATE MODEL
// =============================================================

public class MaintenanceUpdateRequest
{
    public string Priority { get; set; } =
        "Normal";

    public string Status { get; set; } =
        "Pending";

    public string? ResolutionNotes { get; set; }
}