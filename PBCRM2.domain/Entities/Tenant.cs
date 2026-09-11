namespace PBCRM2.Domain.Entities;

public class Tenant
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Bed? Bed { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } =
        new List<MaintenanceRequest>();

    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public ICollection<Renewal> Renewals { get; set; } = new List<Renewal>();
}