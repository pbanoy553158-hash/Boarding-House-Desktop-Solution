namespace PBCRM2.Domain.Entities;

public class MaintenanceRequest
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Priority { get; set; } = "Normal";

    public string Status { get; set; } = "Pending";

    public DateTime DateReported { get; set; } = DateTime.UtcNow;

    public DateTime? DateResolved { get; set; }

    public string? ResolutionNotes { get; set; }

    public Tenant? Tenant { get; set; }
}