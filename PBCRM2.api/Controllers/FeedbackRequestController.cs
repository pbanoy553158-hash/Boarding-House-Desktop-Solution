using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBCRM2.API.Services;
using PBCRM2.Domain.Entities;
using PBCRM2.Infrastructure.Services;

namespace PBCRM2.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager,Staff")]
public class FeedbackRequestController : ControllerBase
{
    private readonly ITenantDbContextFactory _tenantDbContextFactory;
    private readonly FeedbackEmailService _emailService;
    private readonly QrCodeService _qrCodeService;
    private readonly IConfiguration _configuration;

    public FeedbackRequestController(
        ITenantDbContextFactory tenantDbContextFactory,
        FeedbackEmailService emailService,
        QrCodeService qrCodeService,
        IConfiguration configuration)
    {
        _tenantDbContextFactory = tenantDbContextFactory;
        _emailService = emailService;
        _qrCodeService = qrCodeService;
        _configuration = configuration;
    }

    [HttpPost("send/{tenantId:int}")]
    public async Task<IActionResult> SendFeedbackRequest(
        int tenantId)
    {
        try
        {
            // ============================================================
            // GET COMPANY ID
            // ============================================================

            var companyIdValue =
                User.FindFirst("CompanyId")?.Value
                ?? User.FindFirst("companyId")?.Value
                ?? User.FindFirst(ClaimTypes.GroupSid)?.Value;

            if (!int.TryParse(
                companyIdValue,
                out var companyId))
            {
                return Unauthorized(new
                {
                    message =
                        "Company information was not found in your login session."
                });
            }

            // ============================================================
            // GET USER ROLE
            // ============================================================

            var role =
                User.FindFirst(ClaimTypes.Role)?.Value
                ?? User.FindFirst("role")?.Value;

            if (string.IsNullOrWhiteSpace(role))
            {
                return Unauthorized(new
                {
                    message =
                        "User role was not found in your login session."
                });
            }

            // ============================================================
            // GET USER BRANCH
            // ============================================================

            var branchIdValue =
                User.FindFirst("BranchId")?.Value
                ?? User.FindFirst("branchId")?.Value;

            int? userBranchId = null;

            if (int.TryParse(
                branchIdValue,
                out var parsedBranchId))
            {
                userBranchId = parsedBranchId;
            }

            // Manager and Staff must have a branch
            if (
                (
                    role.Equals(
                        "Manager",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    role.Equals(
                        "Staff",
                        StringComparison.OrdinalIgnoreCase)
                )
                &&
                !userBranchId.HasValue)
            {
                return Unauthorized(new
                {
                    message =
                        "Your branch information was not found in your login session."
                });
            }

            // ============================================================
            // OPEN TENANT DATABASE
            // ============================================================

            await using var db =
                await _tenantDbContextFactory.CreateAsync(
                    companyId);

            // ============================================================
            // FIND TENANT
            // ============================================================

            var tenant =
                await db.Tenants
                    .FirstOrDefaultAsync(
                        t => t.Id == tenantId);

            if (tenant == null)
            {
                return NotFound(new
                {
                    message = "Tenant was not found."
                });
            }

            // ============================================================
            // MANAGER / STAFF BRANCH SECURITY
            // ============================================================

            if (
                role.Equals(
                    "Manager",
                    StringComparison.OrdinalIgnoreCase)
                ||
                role.Equals(
                    "Staff",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (!userBranchId.HasValue)
                {
                    return Unauthorized(new
                    {
                        message =
                            "Your branch information was not found in your login session."
                    });
                }

                if (tenant.BranchId != userBranchId.Value)
                {
                    return Forbid();
                }
            }

            // ============================================================
            // CHECK EMAIL
            // ============================================================

            if (string.IsNullOrWhiteSpace(
                tenant.Email))
            {
                return BadRequest(new
                {
                    message =
                        "This tenant does not have an email address."
                });
            }

            // ============================================================
            // REMOVE ALL PREVIOUS UNSUBMITTED REQUESTS
            //
            // IMPORTANT:
            // There is intentionally NO "active request" error here.
            //
            // Every time Send Feedback is clicked:
            //
            //   Old unsubmitted request(s)
            //              ↓
            //           DELETE
            //              ↓
            //       Create new request
            //
            // Submitted requests are preserved.
            // ============================================================

            var previousRequests =
                await db.FeedbackRequests
                    .Where(
                        r =>
                            r.TenantId == tenantId
                            &&
                            !r.IsSubmitted)
                    .ToListAsync();

            if (previousRequests.Count > 0)
            {
                db.FeedbackRequests.RemoveRange(
                    previousRequests);

                await db.SaveChangesAsync();
            }

            // ============================================================
            // CREATE NEW TOKEN
            // ============================================================

            var now =
                DateTime.UtcNow;

            var tokenBytes =
                RandomNumberGenerator.GetBytes(32);

            var token =
                Convert.ToHexString(tokenBytes);

            // ============================================================
            // CREATE NEW FEEDBACK REQUEST
            // ============================================================

            var feedbackRequest =
                new FeedbackRequest
                {
                    CompanyId =
                        companyId,

                    TenantId =
                        tenant.Id,

                    BranchId =
                        tenant.BranchId,

                    Token =
                        token,

                    CreatedAt =
                        now,

                    ExpiresAt =
                        now.AddDays(30),

                    IsSubmitted =
                        false,

                    SubmittedAt =
                        null
                };

            db.FeedbackRequests.Add(
                feedbackRequest);

            await db.SaveChangesAsync();

            // ============================================================
            // GET FEEDBACK WEBSITE
            // ============================================================

            var feedbackBaseUrl =
                _configuration[
                    "FeedbackEmail:FeedbackBaseUrl"];

            if (string.IsNullOrWhiteSpace(
                feedbackBaseUrl))
            {
                db.FeedbackRequests.Remove(
                    feedbackRequest);

                await db.SaveChangesAsync();

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message =
                            "FeedbackBaseUrl is not configured in appsettings.json."
                    });
            }

            feedbackBaseUrl =
                feedbackBaseUrl.TrimEnd('/');

            // ============================================================
            // CREATE FEEDBACK URL
            // ============================================================

            var feedbackUrl =
                $"{feedbackBaseUrl}/feedback/{companyId}/{token}";

            // ============================================================
            // GENERATE QR CODE
            // ============================================================

            byte[] qrCode;

            try
            {
                qrCode =
                    _qrCodeService.GenerateQrCode(
                        feedbackUrl);
            }
            catch (Exception ex)
            {
                db.FeedbackRequests.Remove(
                    feedbackRequest);

                await db.SaveChangesAsync();

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message =
                            "Unable to generate the feedback QR code.",
                        error =
                            ex.Message
                    });
            }

            // ============================================================
            // SEND EMAIL
            // ============================================================

            try
            {
                await _emailService
                    .SendFeedbackRequestAsync(
                        tenant.FullName,
                        tenant.Email,
                        feedbackUrl,
                        qrCode);
            }
            catch (Exception emailException)
            {
                // ========================================================
                // EMAIL FAILED
                //
                // Delete the request so another attempt can be made.
                // ========================================================

                db.FeedbackRequests.Remove(
                    feedbackRequest);

                await db.SaveChangesAsync();

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message =
                            "The feedback request was not sent because the email could not be delivered.",
                        error =
                            emailException.Message
                    });
            }

            // ============================================================
            // SUCCESS
            // ============================================================

            return Ok(new
            {
                message =
                    "Feedback request sent successfully.",

                tenantId =
                    tenant.Id,

                tenantName =
                    tenant.FullName,

                email =
                    tenant.Email,

                companyId =
                    companyId,

                feedbackUrl =
                    feedbackUrl,

                expiresAt =
                    feedbackRequest.ExpiresAt
            });
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Failed to send feedback request.",

                    error =
                        ex.Message
                });
        }
    }
}