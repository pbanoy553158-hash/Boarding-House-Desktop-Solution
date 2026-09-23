namespace PBCRM2.Domain.Entities;

public class FeedbackRequest
{
    public int Id { get; set; }

    // =========================================================
    // COMPANY
    // =========================================================

    // Company that owns this feedback request.
    // Used by the public feedback endpoint to locate
    // the correct tenant database.
    public int CompanyId { get; set; }

    // =========================================================
    // TENANT
    // =========================================================

    // Tenant who will receive the feedback request
    public int TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    // =========================================================
    // BRANCH
    // =========================================================

    // Branch associated with the tenant
    public int? BranchId { get; set; }

    // =========================================================
    // TOKEN
    // =========================================================

    // Unique secure token used in the feedback link
    public string Token { get; set; } = string.Empty;

    // =========================================================
    // REQUEST DATES
    // =========================================================

    // When the request was created
    public DateTime CreatedAt { get; set; } =
        DateTime.UtcNow;

    // When the feedback link expires
    public DateTime ExpiresAt { get; set; }

    // =========================================================
    // SUBMISSION STATUS
    // =========================================================

    // True after the tenant submits feedback
    public bool IsSubmitted { get; set; }

    // When the tenant submitted feedback
    public DateTime? SubmittedAt { get; set; }
}