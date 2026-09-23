using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBCRM2.Domain.Entities;
using PBCRM2.Infrastructure.Data;
using System.Security.Claims;

namespace PBCRM2.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly TenantCRMDbContext _db;

    public ReportsController(
        TenantCRMDbContext db)
    {
        _db = db;
    }

    private IEnumerable<string> GetAllRoleValues()
    {
        return User.Claims
            .Where(c =>
                c.Type == ClaimTypes.Role
                || c.Type == "role"
                || c.Type == "roles"
                || c.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase)
                || c.Type.EndsWith("/claims/role", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v));
    }

    private bool HasRole(params string[] allowed)
    {
        foreach (string role in allowed)
        {
            if (User.IsInRole(role))
            {
                return true;
            }
        }

        foreach (string claim in GetAllRoleValues())
        {
            foreach (string role in allowed)
            {
                if (claim.Equals(role, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsAdminUser()
    {
        return HasRole("Admin", "Administrator", "System Administrator", "SystemAdmin");
    }

    private bool IsManagerUser()
    {
        return HasRole("Manager", "Branch Manager");
    }

    private bool IsStaffUser()
    {
        return HasRole("Staff");
    }

    private bool CanAccessReports()
    {
        return IsAdminUser() || IsManagerUser() || IsStaffUser();
    }

    private int? GetBranchIdFromClaims()
    {
        string?[] keys = { "BranchId", "branchId", "branch_id" };

        foreach (string? key in keys)
        {
            string? value = User.FindFirstValue(key);
            if (int.TryParse(value, out int id) && id > 0)
            {
                return id;
            }
        }

        return null;
    }

    [HttpGet("tenants")]
    public async Task<IActionResult> GetTenantReport()
    {
        try
        {
            if (!CanAccessReports())
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message =
                            "You are not authorized to view reports. Required role: Admin, Manager, or Staff.",
                        rolesFound = GetAllRoleValues().ToArray()
                    });
            }

            IQueryable<Tenant> query =
                _db.Tenants
                    .AsNoTracking()
                    .Include(t => t.Branch);

            if (!IsAdminUser())
            {
                int? branchId = GetBranchIdFromClaims();

                if (!branchId.HasValue)
                {
                    return BadRequest(new
                    {
                        message = "Your account is not assigned to a branch."
                    });
                }

                query = query.Where(t => t.BranchId == branchId.Value);
            }

            var tenants = await query
                .OrderBy(t => t.FullName)
                .Select(t => new
                {
                    t.Id,
                    t.FullName,
                    t.Sex,
                    t.ContactNumber,
                    t.Email,
                    BranchName = t.Branch != null ? t.Branch.BranchName : "Unknown",
                    BranchId = t.BranchId,
                    t.MoveInDate,
                    t.ActualMoveOutDate,
                    t.Status
                })
                .ToListAsync();

            int total = tenants.Count;
            int active = tenants.Count(t =>
                t.Status != null &&
                t.Status.Equals("Active", StringComparison.OrdinalIgnoreCase));
            int inactive = tenants.Count(t =>
                t.Status != null &&
                t.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase));
            int movedOut = tenants.Count(t =>
                t.ActualMoveOutDate.HasValue ||
                (t.Status != null &&
                 t.Status.Equals("Moved Out", StringComparison.OrdinalIgnoreCase)));

            return Ok(new
            {
                summary = new { total, active, inactive, movedOut },
                data = tenants
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "An error occurred while generating the tenant report.",
                error = ex.Message
            });
        }
    }
}