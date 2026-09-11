namespace PBCRM2.Domain.Entities;

public class Feedback
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public Tenant? Tenant { get; set; }
}