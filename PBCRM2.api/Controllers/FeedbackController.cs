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
[Authorize(Roles = "Manager,Staff")]
public class FeedbackController : ControllerBase
{
    private readonly ITenantDbContextFactory _tenantDbFactory;
    private readonly UserManager<ApplicationUser> _userManager;

    public FeedbackController(
        ITenantDbContextFactory tenantDbFactory,
        UserManager<ApplicationUser> userManager)
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
    // GET: api/Feedback
    // MANAGER / STAFF
    // =========================================================

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetFeedback()
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

        var feedback =
            await db.Feedbacks
                .AsNoTracking()
                .Include(f => f.Tenant)
                .Where(f =>
                    f.Tenant != null &&
                    f.Tenant.BranchId ==
                    user.BranchId.Value)
                .OrderByDescending(
                    f => f.SubmittedAt)
                .Select(f => new
                {
                    f.Id,
                    f.TenantId,

                    TenantName =
                        f.Tenant != null
                            ? f.Tenant.FullName
                            : "Unknown Tenant",

                    TenantEmail =
                        f.Tenant != null
                            ? f.Tenant.Email
                            : "",

                    f.Rating,
                    f.Comment,
                    f.SubmittedAt
                })
                .ToListAsync();

        return Ok(feedback);
    }

    // =========================================================
    // GET: api/Feedback/5
    // MANAGER / STAFF
    // =========================================================

    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> GetFeedback(
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

        var feedback =
            await db.Feedbacks
                .AsNoTracking()
                .Include(f => f.Tenant)
                .Where(f =>
                    f.Id == id &&
                    f.Tenant != null &&
                    f.Tenant.BranchId ==
                    user.BranchId.Value)
                .Select(f => new
                {
                    f.Id,
                    f.TenantId,

                    TenantName =
                        f.Tenant != null
                            ? f.Tenant.FullName
                            : "Unknown Tenant",

                    TenantEmail =
                        f.Tenant != null
                            ? f.Tenant.Email
                            : "",

                    f.Rating,
                    f.Comment,
                    f.SubmittedAt
                })
                .FirstOrDefaultAsync();

        if (feedback == null)
        {
            return NotFound(new
            {
                message =
                    "Feedback not found."
            });
        }

        return Ok(feedback);
    }

    // =========================================================
    // POST: api/Feedback
    // ANONYMOUS TENANT FEEDBACK
    // =========================================================
    //
    // This endpoint is intentionally anonymous because the
    // boarding-house tenant does not need a system account
    // to submit feedback.
    //
    // CompanyId identifies which boarding-house database
    // should receive the feedback.
    // =========================================================

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> CreateFeedback(
        [FromBody] FeedbackCreateRequest model)
    {
        // -----------------------------------------------------
        // VALIDATE COMPANY
        // -----------------------------------------------------

        if (model.CompanyId <= 0)
        {
            return BadRequest(new
            {
                message =
                    "Company ID is required."
            });
        }

        // -----------------------------------------------------
        // VALIDATE TENANT
        // -----------------------------------------------------

        if (model.TenantId <= 0)
        {
            return BadRequest(new
            {
                message =
                    "Tenant ID is required."
            });
        }

        // -----------------------------------------------------
        // VALIDATE RATING
        // -----------------------------------------------------

        if (model.Rating < 1 ||
            model.Rating > 5)
        {
            return BadRequest(new
            {
                message =
                    "Rating must be between 1 and 5."
            });
        }

        // -----------------------------------------------------
        // CONNECT TO COMPANY DATABASE
        // -----------------------------------------------------

        await using var db =
            await _tenantDbFactory
                .CreateAsync(
                    model.CompanyId);

        // -----------------------------------------------------
        // CHECK TENANT
        // -----------------------------------------------------

        var tenantExists =
            await db.Tenants
                .AsNoTracking()
                .AnyAsync(t =>
                    t.Id == model.TenantId);

        if (!tenantExists)
        {
            return BadRequest(new
            {
                message =
                    "The selected tenant does not exist."
            });
        }

        // -----------------------------------------------------
        // CREATE FEEDBACK
        // -----------------------------------------------------

        var feedback =
            new Feedback
            {
                TenantId =
                    model.TenantId,

                Rating =
                    model.Rating,

                Comment =
                    model.Comment?.Trim()
                    ?? string.Empty,

                SubmittedAt =
                    DateTime.UtcNow
            };

        db.Feedbacks.Add(feedback);

        await db.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Feedback submitted successfully.",

            id =
                feedback.Id
        });
    }

    // =========================================================
    // DELETE: api/Feedback/5
    // MANAGER ONLY
    // =========================================================

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> DeleteFeedback(
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

        // -----------------------------------------------------
        // FIND FEEDBACK IN MANAGER'S BRANCH
        // -----------------------------------------------------

        var feedback =
            await db.Feedbacks
                .Include(f => f.Tenant)
                .FirstOrDefaultAsync(f =>
                    f.Id == id &&
                    f.Tenant != null &&
                    f.Tenant.BranchId ==
                    user.BranchId.Value);

        if (feedback == null)
        {
            return NotFound(new
            {
                message =
                    "Feedback not found."
            });
        }

        // -----------------------------------------------------
        // DELETE
        // -----------------------------------------------------

        db.Feedbacks.Remove(feedback);

        await db.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Feedback deleted successfully."
        });
    }
}

// =============================================================
// ANONYMOUS FEEDBACK REQUEST
// =============================================================

public class FeedbackCreateRequest
{
    public int CompanyId { get; set; }

    public int TenantId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }
}