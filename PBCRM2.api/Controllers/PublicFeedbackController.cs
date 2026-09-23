using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBCRM2.Domain.Entities;
using PBCRM2.Infrastructure.Services;
using static System.Net.Mime.MediaTypeNames;

namespace PBCRM2.API.Controllers;

[ApiController]
[Route("api/public-feedback")]
public class PublicFeedbackController : ControllerBase
{
    private readonly ITenantDbContextFactory _tenantDbContextFactory;

    public PublicFeedbackController(
        ITenantDbContextFactory tenantDbContextFactory)
    {
        _tenantDbContextFactory = tenantDbContextFactory;
    }

    // ============================================================
    // GET FEEDBACK REQUEST
    // ============================================================
    // Public endpoint used by the feedback page to verify
    // the company and feedback token.
    // ============================================================

    [HttpGet("{companyId:int}/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFeedbackRequest(
        int companyId,
        string token)
    {
        try
        {
            // ====================================================
            // VALIDATE COMPANY ID
            // ====================================================

            if (companyId <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid company ID."
                });
            }

            // ====================================================
            // VALIDATE TOKEN
            // ====================================================

            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new
                {
                    message =
                        "Feedback token is required."
                });
            }

            // ====================================================
            // CREATE TENANT DATABASE CONTEXT
            // ====================================================

            await using var db =
                await _tenantDbContextFactory
                    .CreateAsync(companyId);

            // ====================================================
            // FIND FEEDBACK REQUEST
            // ====================================================

            var feedbackRequest =
                await db.FeedbackRequests
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        r => r.Token == token);

            if (feedbackRequest == null)
            {
                return NotFound(new
                {
                    message =
                        "Feedback request was not found."
                });
            }

            // ====================================================
            // VERIFY COMPANY
            // ====================================================

            if (feedbackRequest.CompanyId != companyId)
            {
                return NotFound(new
                {
                    message =
                        "Feedback request was not found."
                });
            }

            // ====================================================
            // CHECK IF ALREADY SUBMITTED
            // ====================================================

            if (feedbackRequest.IsSubmitted)
            {
                return BadRequest(new
                {
                    message =
                        "This feedback request has already been submitted."
                });
            }

            // ====================================================
            // CHECK EXPIRATION
            // ====================================================

            var now = DateTime.UtcNow;

            if (feedbackRequest.ExpiresAt <= now)
            {
                return BadRequest(new
                {
                    message =
                        "This feedback request has expired."
                });
            }

            // ====================================================
            // VERIFY TENANT
            // ====================================================

            var tenantExists =
                await db.Tenants
                    .AsNoTracking()
                    .AnyAsync(
                        t => t.Id == feedbackRequest.TenantId);

            if (!tenantExists)
            {
                return NotFound(new
                {
                    message =
                        "The tenant associated with this feedback request was not found."
                });
            }

            // ====================================================
            // RETURN SAFE INFORMATION
            // ====================================================
            // Do not expose the tenant's email address.
            // The public page only needs enough information
            // to display the feedback form.
            // ====================================================

            return Ok(new
            {
                valid = true,

                companyId = companyId,

                tenantId =
                    feedbackRequest.TenantId,

                expiresAt =
                    feedbackRequest.ExpiresAt,

                message =
                    "Feedback request is valid."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Unable to process feedback request.",

                    error =
                        ex.Message
                });
        }
    }

    // ============================================================
    // SUBMIT FEEDBACK
    // ============================================================

    [HttpPost("{companyId:int}/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> SubmitFeedback(
        int companyId,
        string token,
        [FromBody] SubmitFeedbackRequest request)
    {
        try
        {
            // ====================================================
            // VALIDATE COMPANY ID
            // ====================================================

            if (companyId <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid company ID."
                });
            }

            // ====================================================
            // VALIDATE TOKEN
            // ====================================================

            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new
                {
                    message =
                        "Feedback token is required."
                });
            }

            // ====================================================
            // VALIDATE REQUEST
            // ====================================================

            if (request == null)
            {
                return BadRequest(new
                {
                    message =
                        "Feedback data is required."
                });
            }

            // ====================================================
            // VALIDATE RATING
            // ====================================================

            if (request.Rating < 1 ||
                request.Rating > 5)
            {
                return BadRequest(new
                {
                    message =
                        "Rating must be between 1 and 5."
                });
            }

            // ====================================================
            // CREATE TENANT DATABASE CONTEXT
            // ====================================================

            await using var db =
                await _tenantDbContextFactory
                    .CreateAsync(companyId);

            // ====================================================
            // FIND FEEDBACK REQUEST
            // ====================================================

            var feedbackRequest =
                await db.FeedbackRequests
                    .FirstOrDefaultAsync(
                        r => r.Token == token);

            if (feedbackRequest == null)
            {
                return NotFound(new
                {
                    message =
                        "Feedback request was not found."
                });
            }

            // ====================================================
            // VERIFY COMPANY
            // ====================================================

            if (feedbackRequest.CompanyId != companyId)
            {
                return NotFound(new
                {
                    message =
                        "Feedback request was not found."
                });
            }

            // ====================================================
            // CHECK IF ALREADY SUBMITTED
            // ====================================================

            if (feedbackRequest.IsSubmitted)
            {
                return BadRequest(new
                {
                    message =
                        "This feedback request has already been submitted."
                });
            }

            // ====================================================
            // CHECK EXPIRATION
            // ====================================================

            var now = DateTime.UtcNow;

            if (feedbackRequest.ExpiresAt <= now)
            {
                return BadRequest(new
                {
                    message =
                        "This feedback request has expired."
                });
            }

            // ====================================================
            // VERIFY TENANT
            // ====================================================

            var tenantExists =
                await db.Tenants
                    .AsNoTracking()
                    .AnyAsync(
                        t => t.Id == feedbackRequest.TenantId);

            if (!tenantExists)
            {
                return NotFound(new
                {
                    message =
                        "The tenant associated with this feedback request was not found."
                });
            }

            // ====================================================
            // CREATE FEEDBACK
            // ====================================================

            var feedback =
                new Feedback
                {
                    TenantId =
                        feedbackRequest.TenantId,

                    Rating =
                        request.Rating,

                    Comment =
                        string.IsNullOrWhiteSpace(
                            request.Comment)
                            ? null
                            : request.Comment.Trim(),

                    SubmittedAt =
                        now
                };

            db.Feedbacks.Add(feedback);

            // ====================================================
            // MARK REQUEST AS SUBMITTED
            // ====================================================

            feedbackRequest.IsSubmitted = true;

            feedbackRequest.SubmittedAt = now;

            // ====================================================
            // SAVE BOTH RECORDS
            // ====================================================

            await db.SaveChangesAsync();

            // ====================================================
            // SUCCESS RESPONSE
            // ====================================================

            return Ok(new
            {
                message =
                    "Feedback submitted successfully.",

                feedbackId =
                    feedback.Id
            });
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "Unable to submit feedback.",

                    error =
                        ex.Message
                });
        }
    }
}

// ================================================================
// SUBMIT FEEDBACK REQUEST MODEL
// ================================================================

public class SubmitFeedbackRequest
{
    public int Rating { get; set; }

    public string? Comment { get; set; }
}