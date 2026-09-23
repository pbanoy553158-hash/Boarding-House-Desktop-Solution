namespace PBCRM2.Domain.Entities;

public class BedAssignmentRequest
{
    public int Id { get; set; }

    public int BedId { get; set; }

    public int TenantId { get; set; }

    public string Status { get; set; } = "Pending";

    public string? RequestedByUserId { get; set; }

    public string? ApprovedByUserId { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ApprovedAt { get; set; }

    public string? RejectionReason { get; set; }

    public Bed? Bed { get; set; }

    public Tenant? Tenant { get; set; }
}