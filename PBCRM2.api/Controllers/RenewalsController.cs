using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBCRM2.Domain.Entities;
using PBCRM2.Infrastructure.Identity;
using PBCRM2.Infrastructure.Services;
using System.Security.Claims;

namespace PBCRM2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class RenewalsController : ControllerBase
{
    // ============================================================
    // SERVICES
    // ============================================================

    private readonly ITenantDbContextFactory _tenantDbFactory;
    private readonly UserManager<ApplicationUser> _userManager;

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public RenewalsController(
        ITenantDbContextFactory tenantDbFactory,
        UserManager<ApplicationUser> userManager)
    {
        _tenantDbFactory = tenantDbFactory;
        _userManager = userManager;
    }

    // ============================================================
    // CURRENT COMPANY ID
    // ============================================================

    private bool TryGetCompanyId(out int companyId)
    {
        companyId = 0;

        var companyIdValue =
            User.FindFirstValue("CompanyId");

        return int.TryParse(
                   companyIdValue,
                   out companyId)
               && companyId > 0;
    }

    // ============================================================
    // CURRENT USER
    // ============================================================

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

    // ============================================================
    // GET: api/Renewals
    //
    // ADMIN:
    //   Can see renewals from all branches.
    //
    // MANAGER:
    //   Can only see renewals for their assigned branch.
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var user =
            await CurrentUserAsync();

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

        if (User.IsInRole("Manager") &&
            !user.BranchId.HasValue)
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

        var query =
            db.Renewals
                .AsNoTracking()
                .Include(r => r.Tenant)
                .AsQueryable();

        // --------------------------------------------------------
        // MANAGER = ASSIGNED BRANCH ONLY
        // --------------------------------------------------------

        if (User.IsInRole("Manager"))
        {
            query = query.Where(r =>
                r.Tenant != null &&
                r.Tenant.BranchId ==
                user.BranchId!.Value);
        }

        var renewals =
            await query
                .OrderByDescending(
                    r => r.EndDate)
                .Select(r => new
                {
                    r.Id,
                    r.TenantId,

                    TenantName =
                        r.Tenant != null
                            ? r.Tenant.FullName
                            : "Unknown Tenant",

                    TenantEmail =
                        r.Tenant != null
                            ? r.Tenant.Email
                            : "",

                    r.StartDate,
                    r.EndDate,
                    r.RenewalStatus,
                    r.RenewalDate,
                    r.Notes
                })
                .ToListAsync();

        return Ok(renewals);
    }

    // ============================================================
    // GET: api/Renewals/5
    //
    // ADMIN:
    //   Can view any renewal in the company.
    //
    // MANAGER:
    //   Can only view renewals from their assigned branch.
    // ============================================================

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user =
            await CurrentUserAsync();

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

        if (User.IsInRole("Manager") &&
            !user.BranchId.HasValue)
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

        var query =
            db.Renewals
                .AsNoTracking()
                .Include(r => r.Tenant)
                .Where(r => r.Id == id);

        // --------------------------------------------------------
        // MANAGER = ASSIGNED BRANCH ONLY
        // --------------------------------------------------------

        if (User.IsInRole("Manager"))
        {
            query = query.Where(r =>
                r.Tenant != null &&
                r.Tenant.BranchId ==
                user.BranchId!.Value);
        }

        var renewal =
            await query
                .Select(r => new
                {
                    r.Id,
                    r.TenantId,

                    TenantName =
                        r.Tenant != null
                            ? r.Tenant.FullName
                            : "Unknown Tenant",

                    TenantEmail =
                        r.Tenant != null
                            ? r.Tenant.Email
                            : "",

                    r.StartDate,
                    r.EndDate,
                    r.RenewalStatus,
                    r.RenewalDate,
                    r.Notes
                })
                .FirstOrDefaultAsync();

        if (renewal == null)
        {
            return NotFound(new
            {
                message =
                    "Renewal record not found."
            });
        }

        return Ok(renewal);
    }

    // ============================================================
    // GET: api/Renewals/Tenant/5
    //
    // ADMIN:
    //   Can view renewals for any tenant in the company.
    //
    // MANAGER:
    //   Can only view renewals for tenants in their branch.
    // ============================================================

    [HttpGet("Tenant/{tenantId:int}")]
    public async Task<IActionResult> GetByTenant(
        int tenantId)
    {
        var user =
            await CurrentUserAsync();

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

        if (User.IsInRole("Manager") &&
            !user.BranchId.HasValue)
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

        // --------------------------------------------------------
        // CHECK TENANT
        // --------------------------------------------------------

        var tenantQuery =
            db.Tenants
                .AsNoTracking()
                .Where(t => t.Id == tenantId);

        if (User.IsInRole("Manager"))
        {
            tenantQuery = tenantQuery.Where(t =>
                t.BranchId ==
                user.BranchId!.Value);
        }

        var tenantExists =
            await tenantQuery.AnyAsync();

        if (!tenantExists)
        {
            return NotFound(new
            {
                message =
                    "Tenant not found."
            });
        }

        // --------------------------------------------------------
        // GET RENEWALS
        // --------------------------------------------------------

        var renewals =
            await db.Renewals
                .AsNoTracking()
                .Where(r =>
                    r.TenantId == tenantId)
                .OrderByDescending(
                    r => r.EndDate)
                .Select(r => new
                {
                    r.Id,
                    r.TenantId,
                    r.StartDate,
                    r.EndDate,
                    r.RenewalStatus,
                    r.RenewalDate,
                    r.Notes
                })
                .ToListAsync();

        return Ok(renewals);
    }

    // ============================================================
    // CREATE RENEWAL
    //
    // ADMIN / MANAGER
    // ============================================================

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] RenewalRequest request)
    {
        // --------------------------------------------------------
        // VALIDATE DATES
        // --------------------------------------------------------

        if (request.StartDate == default)
        {
            return BadRequest(
                "Start date is required.");
        }

        if (request.EndDate == default)
        {
            return BadRequest(
                "End date is required.");
        }

        if (request.EndDate.Date <
            request.StartDate.Date)
        {
            return BadRequest(
                "End date cannot be earlier than the start date.");
        }

        // --------------------------------------------------------
        // VALIDATE STATUS
        // --------------------------------------------------------

        var allowedStatuses =
            new[]
            {
                "Pending",
                "Approved",
                "Declined",
                "Completed"
            };

        if (!allowedStatuses.Contains(
                request.RenewalStatus,
                StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message =
                    "Invalid renewal status."
            });
        }

        // --------------------------------------------------------
        // CURRENT USER
        // --------------------------------------------------------

        var user =
            await CurrentUserAsync();

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

        if (User.IsInRole("Manager") &&
            !user.BranchId.HasValue)
        {
            return BadRequest(new
            {
                message =
                    "Your account is not assigned to a branch."
            });
        }

        // --------------------------------------------------------
        // GET TENANT DATABASE
        // --------------------------------------------------------

        await using var db =
            await _tenantDbFactory
                .CreateAsync(companyId);

        // --------------------------------------------------------
        // FIND TENANT
        // --------------------------------------------------------

        var tenantQuery =
            db.Tenants
                .Where(t =>
                    t.Id == request.TenantId);

        if (User.IsInRole("Manager"))
        {
            tenantQuery =
                tenantQuery.Where(t =>
                    t.BranchId ==
                    user.BranchId!.Value);
        }

        var tenant =
            await tenantQuery
                .FirstOrDefaultAsync();

        if (tenant == null)
        {
            return NotFound(new
            {
                message =
                    "Tenant not found."
            });
        }

        // --------------------------------------------------------
        // TENANT MUST BE ACTIVE
        // --------------------------------------------------------

        if (tenant.Status != "Active" ||
            tenant.ActualMoveOutDate.HasValue)
        {
            return BadRequest(new
            {
                message =
                    "Renewal cannot be created for an inactive tenant."
            });
        }

        // --------------------------------------------------------
        // CREATE RENEWAL
        // --------------------------------------------------------

        var renewal =
            new Renewal
            {
                TenantId =
                    request.TenantId,

                StartDate =
                    request.StartDate.Date,

                EndDate =
                    request.EndDate.Date,

                RenewalStatus =
                    request.RenewalStatus.Trim(),

                RenewalDate =
                    request.RenewalDate?
                        .Date,

                Notes =
                    request.Notes?
                        .Trim()
            };

        db.Renewals.Add(renewal);

        await db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = renewal.Id
            },
            new
            {
                message =
                    "Renewal created successfully.",

                renewal.Id,
                renewal.TenantId,
                renewal.StartDate,
                renewal.EndDate,
                renewal.RenewalStatus,
                renewal.RenewalDate,
                renewal.Notes
            });
    }

    // ============================================================
    // UPDATE RENEWAL
    //
    // ADMIN / MANAGER
    // ============================================================

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] RenewalRequest request)
    {
        // --------------------------------------------------------
        // VALIDATE DATES
        // --------------------------------------------------------

        if (request.StartDate == default)
        {
            return BadRequest(
                "Start date is required.");
        }

        if (request.EndDate == default)
        {
            return BadRequest(
                "End date is required.");
        }

        if (request.EndDate.Date <
            request.StartDate.Date)
        {
            return BadRequest(
                "End date cannot be earlier than the start date.");
        }

        // --------------------------------------------------------
        // VALIDATE STATUS
        // --------------------------------------------------------

        var allowedStatuses =
            new[]
            {
                "Pending",
                "Approved",
                "Declined",
                "Completed"
            };

        if (!allowedStatuses.Contains(
                request.RenewalStatus,
                StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message =
                    "Invalid renewal status."
            });
        }

        // --------------------------------------------------------
        // CURRENT USER
        // --------------------------------------------------------

        var user =
            await CurrentUserAsync();

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

        if (User.IsInRole("Manager") &&
            !user.BranchId.HasValue)
        {
            return BadRequest(new
            {
                message =
                    "Your account is not assigned to a branch."
            });
        }

        // --------------------------------------------------------
        // GET TENANT DATABASE
        // --------------------------------------------------------

        await using var db =
            await _tenantDbFactory
                .CreateAsync(companyId);

        // --------------------------------------------------------
        // FIND RENEWAL
        // --------------------------------------------------------

        var query =
            db.Renewals
                .Include(r => r.Tenant)
                .Where(r => r.Id == id);

        if (User.IsInRole("Manager"))
        {
            query = query.Where(r =>
                r.Tenant != null &&
                r.Tenant.BranchId ==
                user.BranchId!.Value);
        }

        var renewal =
            await query.FirstOrDefaultAsync();

        if (renewal == null)
        {
            return NotFound(new
            {
                message =
                    "Renewal record not found."
            });
        }

        // --------------------------------------------------------
        // UPDATE
        // --------------------------------------------------------

        renewal.StartDate =
            request.StartDate.Date;

        renewal.EndDate =
            request.EndDate.Date;

        renewal.RenewalStatus =
            request.RenewalStatus.Trim();

        renewal.RenewalDate =
            request.RenewalDate?.Date;

        renewal.Notes =
            request.Notes?.Trim();

        await db.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Renewal updated successfully."
        });
    }

    // ============================================================
    // DELETE RENEWAL
    //
    // ADMIN ONLY
    //
    // Manager should not delete historical renewal records.
    // ============================================================

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(
        int id)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return BadRequest(new
            {
                message =
                    "Your account is not assigned to a company."
            });
        }

        await using var db =
            await _tenantDbFactory
                .CreateAsync(companyId);

        var renewal =
            await db.Renewals
                .FirstOrDefaultAsync(
                    r => r.Id == id);

        if (renewal == null)
        {
            return NotFound(new
            {
                message =
                    "Renewal record not found."
            });
        }

        db.Renewals.Remove(renewal);

        await db.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Renewal deleted successfully."
        });
    }
}

// ================================================================
// RENEWAL REQUEST
// ================================================================

public class RenewalRequest
{
    public int TenantId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string RenewalStatus { get; set; } =
        "Pending";

    public DateTime? RenewalDate { get; set; }

    public string? Notes { get; set; }
}