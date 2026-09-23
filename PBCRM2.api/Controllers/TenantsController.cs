using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBCRM2.Domain.Entities;
using PBCRM2.Infrastructure.Services;

namespace PBCRM2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager,Staff")]
public class TenantsController : ControllerBase
{
    // ============================================================
    // SERVICES
    // ============================================================

    private readonly ITenantDbContextFactory _tenantDbFactory;

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public TenantsController(
        ITenantDbContextFactory tenantDbFactory)
    {
        _tenantDbFactory = tenantDbFactory;
    }

    // ============================================================
    // CURRENT COMPANY ID
    // ============================================================
    // CompanyId = the System Tenant / Boarding House business.
    //
    // TenantId = the actual person renting a room/bed.
    // ============================================================

    private int? CurrentCompanyId
    {
        get
        {
            var companyId =
                User.FindFirstValue("CompanyId");

            if (int.TryParse(companyId, out var id) &&
                id > 0)
            {
                return id;
            }

            return null;
        }
    }

    // ============================================================
    // GET TENANT DATABASE
    // ============================================================

    private async Task<
        PBCRM2.Infrastructure.Data.TenantCRMDbContext?>
        GetTenantDbAsync()
    {
        var companyId = CurrentCompanyId;

        if (!companyId.HasValue)
        {
            return null;
        }

        return await _tenantDbFactory
            .CreateAsync(companyId.Value);
    }

    // ============================================================
    // GET ALL TENANTS
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var db = await GetTenantDbAsync();

        if (db == null)
        {
            return Unauthorized(
                "Your account is not assigned to a company.");
        }

        await using (db)
        {
            var tenants = await db.Tenants
                .AsNoTracking()
                .Include(t => t.Branch)
                .Include(t => t.Bed!)
                    .ThenInclude(b => b.Room)
                .OrderBy(t => t.FullName)
                .Select(t => new
                {
                    t.Id,
                    t.FullName,
                    t.DateOfBirth,
                    t.Sex,
                    t.ContactNumber,
                    t.Email,
                    t.CurrentAddress,
                    t.EmergencyContactName,
                    t.EmergencyRelationship,
                    t.EmergencyContactNumber,

                    t.BranchId,

                    BranchName =
                        t.Branch != null
                            ? t.Branch.BranchName
                            : null,

                    t.MoveInDate,
                    t.ActualMoveOutDate,
                    t.Status,

                    RoomId =
                        t.Bed != null
                            ? t.Bed.RoomId
                            : (int?)null,

                    RoomNumber =
                        t.Bed != null &&
                        t.Bed.Room != null
                            ? t.Bed.Room.RoomNumber
                            : null,

                    BedId =
                        t.Bed != null
                            ? t.Bed.Id
                            : (int?)null,

                    BedNumber =
                        t.Bed != null
                            ? t.Bed.BedNumber
                            : null
                })
                .ToListAsync();

            return Ok(tenants);
        }
    }

    // ============================================================
    // GET TENANT BY ID
    // ============================================================

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var db = await GetTenantDbAsync();

        if (db == null)
        {
            return Unauthorized(
                "Your account is not assigned to a company.");
        }

        await using (db)
        {
            var tenant = await db.Tenants
                .AsNoTracking()
                .Include(t => t.Branch)
                .Include(t => t.Bed!)
                    .ThenInclude(b => b.Room)
                .FirstOrDefaultAsync(
                    t => t.Id == id);

            if (tenant == null)
            {
                return NotFound(
                    "Tenant not found.");
            }

            return Ok(new
            {
                tenant.Id,
                tenant.FullName,
                tenant.DateOfBirth,
                tenant.Sex,
                tenant.ContactNumber,
                tenant.Email,
                tenant.CurrentAddress,
                tenant.EmergencyContactName,
                tenant.EmergencyRelationship,
                tenant.EmergencyContactNumber,

                tenant.BranchId,

                BranchName =
                    tenant.Branch?.BranchName,

                tenant.MoveInDate,
                tenant.ActualMoveOutDate,
                tenant.Status,

                RoomId =
                    tenant.Bed?.RoomId,

                RoomNumber =
                    tenant.Bed?.Room?.RoomNumber,

                BedId =
                    tenant.Bed?.Id,

                BedNumber =
                    tenant.Bed?.BedNumber
            });
        }
    }

    // ============================================================
    // CREATE TENANT
    // ============================================================

    [HttpPost]
    public async Task<IActionResult> Create(
        TenantRequest request)
    {
        if (!HasRequiredFields(request))
        {
            return BadRequest(
                "All required tenant and emergency-contact fields must be completed.");
        }

        if (!request.BranchId.HasValue)
        {
            return BadRequest(
                "BranchId is required.");
        }

        var db = await GetTenantDbAsync();

        if (db == null)
        {
            return Unauthorized(
                "Your account is not assigned to a company.");
        }

        await using (db)
        {
            int branchId =
                request.BranchId.Value;

            var branchExists =
                await db.Branches.AnyAsync(
                    b =>
                        b.Id == branchId &&
                        b.IsActive);

            if (!branchExists)
            {
                return BadRequest(
                    "The selected branch does not exist or is inactive.");
            }

            var tenant = new Tenant
            {
                FullName =
                    request.FullName.Trim(),

                DateOfBirth =
                    request.DateOfBirth.Date,

                Sex =
                    request.Sex.Trim(),

                ContactNumber =
                    request.ContactNumber.Trim(),

                Email =
                    request.Email.Trim(),

                CurrentAddress =
                    request.CurrentAddress.Trim(),

                EmergencyContactName =
                    request.EmergencyContactName.Trim(),

                EmergencyRelationship =
                    request.EmergencyRelationship.Trim(),

                EmergencyContactNumber =
                    request.EmergencyContactNumber.Trim(),

                BranchId =
                    branchId,

                MoveInDate =
                    request.MoveInDate.Date,

                Status =
                    "Active",

                CreatedAt =
                    DateTime.UtcNow
            };

            db.Tenants.Add(tenant);

            await db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = tenant.Id },
                new
                {
                    message =
                        "Tenant created successfully.",

                    tenantId =
                        tenant.Id
                });
        }
    }

    // ============================================================
    // UPDATE TENANT
    // ============================================================

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        TenantRequest request)
    {
        if (!HasRequiredFields(request))
        {
            return BadRequest(
                "All required tenant and emergency-contact fields must be completed.");
        }

        var db = await GetTenantDbAsync();

        if (db == null)
        {
            return Unauthorized(
                "Your account is not assigned to a company.");
        }

        await using (db)
        {
            var tenant =
                await db.Tenants
                    .FirstOrDefaultAsync(
                        t => t.Id == id);

            if (tenant == null)
            {
                return NotFound(
                    "Tenant not found.");
            }

            tenant.FullName =
                request.FullName.Trim();

            tenant.DateOfBirth =
                request.DateOfBirth.Date;

            tenant.Sex =
                request.Sex.Trim();

            tenant.ContactNumber =
                request.ContactNumber.Trim();

            tenant.Email =
                request.Email.Trim();

            tenant.CurrentAddress =
                request.CurrentAddress.Trim();

            tenant.EmergencyContactName =
                request.EmergencyContactName.Trim();

            tenant.EmergencyRelationship =
                request.EmergencyRelationship.Trim();

            tenant.EmergencyContactNumber =
                request.EmergencyContactNumber.Trim();

            tenant.MoveInDate =
                request.MoveInDate.Date;

            await db.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Tenant updated successfully."
            });
        }
    }

    // ============================================================
    // MOVE OUT TENANT
    // ============================================================

    [HttpPost("{id:int}/move-out")]
    public async Task<IActionResult> MoveOut(
        int id,
        [FromBody] MoveOutRequest request)
    {
        var db = await GetTenantDbAsync();

        if (db == null)
        {
            return Unauthorized(
                "Your account is not assigned to a company.");
        }

        await using (db)
        {
            var tenant =
                await db.Tenants
                    .Include(t => t.Bed)
                    .FirstOrDefaultAsync(
                        t => t.Id == id);

            if (tenant == null)
            {
                return NotFound(
                    "Tenant not found.");
            }

            if (tenant.Status == "Moved Out")
            {
                return BadRequest(
                    "Tenant is already moved out.");
            }

            // ----------------------------------------------------
            // SET MOVE-OUT INFORMATION
            // ----------------------------------------------------

            tenant.ActualMoveOutDate =
                (request.MoveOutDate ??
                 DateTime.Today).Date;

            tenant.Status =
                "Moved Out";

            // ----------------------------------------------------
            // RELEASE BED
            // ----------------------------------------------------

            if (tenant.Bed != null)
            {
                tenant.Bed.TenantId = null;
                tenant.Bed.Status = "Available";
            }

            await db.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Tenant moved out successfully.",

                moveOutDate =
                    tenant.ActualMoveOutDate
            });
        }
    }

    // ============================================================
    // DELETE TENANT
    // ============================================================

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var db = await GetTenantDbAsync();

        if (db == null)
        {
            return Unauthorized(
                "Your account is not assigned to a company.");
        }

        await using (db)
        {
            var tenant =
                await db.Tenants
                    .Include(t => t.Bed)
                    .FirstOrDefaultAsync(
                        t => t.Id == id);

            if (tenant == null)
            {
                return NotFound(
                    "Tenant not found.");
            }

            if (tenant.Bed != null)
            {
                return BadRequest(
                    "Move the tenant out and release the bed before deleting the record.");
            }

            db.Tenants.Remove(tenant);

            await db.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Tenant deleted successfully."
            });
        }
    }

    // ============================================================
    // REQUIRED FIELD VALIDATION
    // ============================================================

    private static bool HasRequiredFields(
        TenantRequest request)
    {
        return
            !string.IsNullOrWhiteSpace(
                request.FullName) &&

            !string.IsNullOrWhiteSpace(
                request.Sex) &&

            !string.IsNullOrWhiteSpace(
                request.ContactNumber) &&

            !string.IsNullOrWhiteSpace(
                request.Email) &&

            !string.IsNullOrWhiteSpace(
                request.CurrentAddress) &&

            !string.IsNullOrWhiteSpace(
                request.EmergencyContactName) &&

            !string.IsNullOrWhiteSpace(
                request.EmergencyRelationship) &&

            !string.IsNullOrWhiteSpace(
                request.EmergencyContactNumber);
    }
}

// ================================================================
// TENANT REQUEST
// ================================================================

public class TenantRequest
{
    public string FullName { get; set; } =
        string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string Sex { get; set; } =
        string.Empty;

    public string ContactNumber { get; set; } =
        string.Empty;

    public string Email { get; set; } =
        string.Empty;

    public string CurrentAddress { get; set; } =
        string.Empty;

    public string EmergencyContactName { get; set; } =
        string.Empty;

    public string EmergencyRelationship { get; set; } =
        string.Empty;

    public string EmergencyContactNumber { get; set; } =
        string.Empty;

    public int? BranchId { get; set; }

    public DateTime MoveInDate { get; set; } =
        DateTime.Today;
}

// ================================================================
// MOVE-OUT REQUEST
// ================================================================

public class MoveOutRequest
{
    public DateTime? MoveOutDate { get; set; }
}