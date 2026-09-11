namespace PBCRM2.Domain.Entities;

public class Renewal
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string RenewalStatus { get; set; } = "Pending";

    public DateTime? RenewalDate { get; set; }

    public string? Notes { get; set; }

    public Tenant? Tenant { get; set; }
}