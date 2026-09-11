namespace PBCRM2.Domain.Entities;

public class Bed
{
    public int Id { get; set; }

    public int RoomId { get; set; }

    public string BedNumber { get; set; } = string.Empty;

    public string Status { get; set; } = "Available";

    public int? TenantId { get; set; }

    public Room? Room { get; set; }

    public Tenant? Tenant { get; set; }
}