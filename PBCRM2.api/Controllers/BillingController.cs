using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBCRM2.Infrastructure.Identity;
using PBCRM2.Infrastructure.Services;
using System.Security.Claims;

namespace PBCRM2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager,Staff")]
public class BillingController : ControllerBase
{
    private readonly ITenantDbContextFactory _tenantDbFactory;
    private readonly UserManager<ApplicationUser> _userManager;

    public BillingController(
        ITenantDbContextFactory tenantDbFactory,
        UserManager<ApplicationUser> userManager)
    {
        _tenantDbFactory = tenantDbFactory;
        _userManager = userManager;
    }

    // =========================================================
    // GET CURRENT COMPANY ID
    // =========================================================
    // CompanyId = System Tenant / Boarding House business.
    //
    // This is NOT the TenantId of the person renting a room.
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
    // GET ALL BILLING RECORDS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> GetBillings()
    {
        var user = await CurrentUserAsync();

        if (user == null)
        {
            return Unauthorized();
        }

        // -----------------------------------------------------
        // COMPANY SECURITY
        // -----------------------------------------------------

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

        var query = db.Billings
            .AsNoTracking()
            .Include(b => b.Tenant)
            .ThenInclude(t => t!.Bed)
            .ThenInclude(b => b!.Room)
            .Include(b => b.Payments)
            .AsQueryable();

        // -----------------------------------------------------
        // BRANCH SECURITY
        // -----------------------------------------------------
        //
        // ADMIN:
        // Can view billing records from ALL branches.
        //
        // MANAGER / STAFF:
        // Can only view billing records from their
        // assigned branch.
        // -----------------------------------------------------

        if (!User.IsInRole("Admin"))
        {
            if (!user.BranchId.HasValue)
            {
                return BadRequest(
                    "Your account is not assigned to a branch.");
            }

            query = query.Where(
                b =>
                    b.Tenant != null &&
                    b.Tenant.BranchId ==
                    user.BranchId.Value);
        }

        // -----------------------------------------------------
        // GET BILLING RECORDS
        // -----------------------------------------------------

        var billings = await query
            .OrderByDescending(
                b => b.DueDate)
            .ToListAsync();

        // -----------------------------------------------------
        // CREATE RESPONSE
        // -----------------------------------------------------

        var result = billings.Select(b =>
        {
            decimal totalPaid =
                b.Payments.Sum(
                    p => p.Amount);

            decimal totalDue =
                b.MonthlyRentalAmount;

            decimal outstandingBalance =
                Math.Max(
                    0,
                    totalDue - totalPaid);

            string status =
                GetBillingStatus(
                    b.DueDate,
                    totalDue,
                    totalPaid);

            return new
            {
                b.Id,

                b.TenantId,

                TenantName =
                    b.Tenant?.FullName ??
                    "Unknown Tenant",

                RoomNumber =
                    b.Tenant?.Bed?.Room?.RoomNumber,

                BedNumber =
                    b.Tenant?.Bed?.BedNumber,

                b.MonthlyRentalAmount,

                b.BillingPeriodStart,

                b.BillingPeriodEnd,

                b.DueDate,

                TotalDue =
                    totalDue,

                TotalPaid =
                    totalPaid,

                OutstandingBalance =
                    outstandingBalance,

                Status =
                    status,

                b.Notes
            };
        });

        return Ok(result);
    }

    // =========================================================
    // CREATE BILLING
    // =========================================================

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateBilling(
        BillingRequest request)
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
            return BadRequest(
                "Your account is not assigned to a branch.");
        }

        await using var db =
            await _tenantDbFactory
                .CreateAsync(companyId);

        // -----------------------------------------------------
        // VALIDATE TENANT
        // -----------------------------------------------------

        var tenant = await db.Tenants
            .Include(t => t.Bed)
            .FirstOrDefaultAsync(
                t =>
                    t.Id ==
                    request.TenantId);

        if (tenant == null)
        {
            return NotFound(
                "Tenant not found.");
        }

        // -----------------------------------------------------
        // BRANCH SECURITY
        // -----------------------------------------------------

        if (tenant.BranchId != user.BranchId.Value)
        {
            return Forbid();
        }

        // -----------------------------------------------------
        // TENANT STATUS
        // -----------------------------------------------------

        if (!string.Equals(
                tenant.Status,
                "Active",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(
                "Billing can only be created for an active tenant.");
        }

        // -----------------------------------------------------
        // VALIDATE AMOUNT
        // -----------------------------------------------------

        if (request.MonthlyRentalAmount <= 0)
        {
            return BadRequest(
                "Monthly rental amount must be greater than zero.");
        }

        // -----------------------------------------------------
        // VALIDATE BILLING PERIOD
        // -----------------------------------------------------

        if (request.BillingPeriodEnd.Date <
            request.BillingPeriodStart.Date)
        {
            return BadRequest(
                "Billing period end cannot be earlier than the start date.");
        }

        // -----------------------------------------------------
        // CREATE BILLING
        // -----------------------------------------------------

        var billing =
            new PBCRM2.Domain.Entities.Billing
            {
                TenantId =
                    tenant.Id,

                MonthlyRentalAmount =
                    request.MonthlyRentalAmount,

                BillingPeriodStart =
                    request.BillingPeriodStart.Date,

                BillingPeriodEnd =
                    request.BillingPeriodEnd.Date,

                DueDate =
                    request.DueDate.Date,

                Notes =
                    request.Notes?.Trim()
                    ?? string.Empty
            };

        db.Billings.Add(billing);

        await db.SaveChangesAsync();

        // -----------------------------------------------------
        // RETURN CREATED RECORD
        // -----------------------------------------------------

        return Ok(new
        {
            message =
                "Billing record created successfully.",

            billingId =
                billing.Id
        });
    }

    // =========================================================
    // GET PAYMENTS FOR BILLING
    // =========================================================

    [HttpGet("{id:int}/payments")]
    public async Task<IActionResult> GetPayments(
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
            return BadRequest(
                "Your account is not assigned to a branch.");
        }

        await using var db =
            await _tenantDbFactory
                .CreateAsync(companyId);

        var billing = await db.Billings
            .AsNoTracking()
            .Include(b => b.Tenant)
            .FirstOrDefaultAsync(
                b => b.Id == id);

        if (billing == null)
        {
            return NotFound(
                "Billing record not found.");
        }

        // -----------------------------------------------------
        // BRANCH SECURITY
        // -----------------------------------------------------

        if (billing.Tenant == null ||
            billing.Tenant.BranchId !=
            user.BranchId.Value)
        {
            return Forbid();
        }

        // -----------------------------------------------------
        // GET PAYMENTS
        // -----------------------------------------------------

        var payments =
            await db.Payments
                .AsNoTracking()
                .Where(
                    p =>
                        p.BillingId ==
                        id)
                .OrderByDescending(
                    p =>
                        p.PaymentDate)
                .Select(
                    p => new
                    {
                        p.Id,

                        p.BillingId,

                        p.TenantId,

                        TenantName =
                            p.Tenant != null
                                ? p.Tenant.FullName
                                : "Unknown Tenant",

                        p.Amount,

                        p.PaymentDate,

                        p.PaymentMethod,

                        p.Status,

                        p.ReferenceNumber,

                        p.Notes
                    })
                .ToListAsync();

        return Ok(payments);
    }

    // =========================================================
    // ADD PAYMENT
    // =========================================================

    [HttpPost("{id:int}/payments")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    public async Task<IActionResult> AddPayment(
        int id,
        PaymentRequest request)
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
            return BadRequest(
                "Your account is not assigned to a branch.");
        }

        await using var db =
            await _tenantDbFactory
                .CreateAsync(companyId);

        var billing = await db.Billings
            .Include(b => b.Tenant)
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(
                b => b.Id == id);

        if (billing == null)
        {
            return NotFound(
                "Billing record not found.");
        }

        // -----------------------------------------------------
        // BRANCH SECURITY
        // -----------------------------------------------------

        if (billing.Tenant == null ||
            billing.Tenant.BranchId !=
            user.BranchId.Value)
        {
            return Forbid();
        }

        // -----------------------------------------------------
        // VALIDATE PAYMENT
        // -----------------------------------------------------

        if (request.Amount <= 0)
        {
            return BadRequest(
                "Payment amount must be greater than zero.");
        }

        decimal currentPaid =
            billing.Payments.Sum(
                p => p.Amount);

        decimal remaining =
            billing.MonthlyRentalAmount -
            currentPaid;

        if (remaining <= 0)
        {
            return BadRequest(
                "This billing record has already been fully paid.");
        }

        if (request.Amount > remaining)
        {
            return BadRequest(
                $"Payment exceeds the outstanding balance of {remaining:N2}.");
        }

        // -----------------------------------------------------
        // CREATE PAYMENT
        // -----------------------------------------------------

        var payment =
            new PBCRM2.Domain.Entities.Payment
            {
                BillingId =
                    billing.Id,

                TenantId =
                    billing.TenantId,

                Amount =
                    request.Amount,

                PaymentDate =
                    request.PaymentDate.Date,

                PaymentMethod =
                    string.IsNullOrWhiteSpace(
                        request.PaymentMethod)
                        ? "Cash"
                        : request.PaymentMethod.Trim(),

                Status =
                    "Paid",

                ReferenceNumber =
                    string.IsNullOrWhiteSpace(
                        request.ReferenceNumber)
                        ? null
                        : request.ReferenceNumber.Trim(),

                Notes =
                    string.IsNullOrWhiteSpace(
                        request.Notes)
                        ? null
                        : request.Notes.Trim()
            };

        db.Payments.Add(payment);

        await db.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Payment recorded successfully.",

            paymentId =
                payment.Id
        });
    }

    // =========================================================
    // DELETE PAYMENT
    // =========================================================

    [HttpDelete("payments/{paymentId:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeletePayment(
        int paymentId)
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
            return BadRequest(
                "Your account is not assigned to a branch.");
        }

        await using var db =
            await _tenantDbFactory
                .CreateAsync(companyId);

        var payment = await db.Payments
            .Include(p => p.Billing)
            .ThenInclude(b => b!.Tenant)
            .FirstOrDefaultAsync(
                p =>
                    p.Id ==
                    paymentId);

        if (payment == null)
        {
            return NotFound(
                "Payment not found.");
        }

        // -----------------------------------------------------
        // BRANCH SECURITY
        // -----------------------------------------------------

        if (payment.Billing == null ||
            payment.Billing.Tenant == null ||
            payment.Billing.Tenant.BranchId !=
            user.BranchId.Value)
        {
            return Forbid();
        }

        db.Payments.Remove(payment);

        await db.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Payment deleted successfully."
        });
    }

    // =========================================================
    // BILLING STATUS
    // =========================================================

    private static string GetBillingStatus(
        DateTime dueDate,
        decimal totalDue,
        decimal totalPaid)
    {
        // -----------------------------------------------------
        // FULLY PAID
        // -----------------------------------------------------

        if (totalPaid >= totalDue)
        {
            return "Paid";
        }

        // -----------------------------------------------------
        // SOME PAYMENT HAS BEEN MADE
        // -----------------------------------------------------

        if (totalPaid > 0)
        {
            if (dueDate.Date < DateTime.Today)
            {
                return "Overdue";
            }

            return "Partially Paid";
        }

        // -----------------------------------------------------
        // NOTHING PAID AND OVERDUE
        // -----------------------------------------------------

        if (dueDate.Date < DateTime.Today)
        {
            return "Overdue";
        }

        // -----------------------------------------------------
        // NOTHING PAID AND NOT YET DUE
        // -----------------------------------------------------

        return "Pending";
    }
}

// =============================================================
// BILLING REQUEST
// =============================================================

public class BillingRequest
{
    public int TenantId { get; set; }

    public decimal MonthlyRentalAmount { get; set; }

    public DateTime BillingPeriodStart { get; set; }

    public DateTime BillingPeriodEnd { get; set; }

    public DateTime DueDate { get; set; }

    public string? Notes { get; set; }
}

// =============================================================
// PAYMENT REQUEST
// =============================================================

public class PaymentRequest
{
    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; } =
        DateTime.Today;

    public string PaymentMethod { get; set; } =
        "Cash";

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }
}