using System;
using System.Collections.Generic;
using System.Text;

namespace PBCRM2.Domain.Entities;

public class Tenant
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string Sex { get; set; } = string.Empty;

    public string ContactNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string CurrentAddress { get; set; } = string.Empty;

    public string EmergencyContactName { get; set; } = string.Empty;

    public string EmergencyRelationship { get; set; } = string.Empty;

    public string EmergencyContactNumber { get; set; } = string.Empty;

    public int BranchId { get; set; }

    public DateTime MoveInDate { get; set; } = DateTime.Today;

    public DateTime? ActualMoveOutDate { get; set; }

    public string Status { get; set; } = "Active";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Branch? Branch { get; set; }

    public Bed? Bed { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public ICollection<Billing> Billings { get; set; } = new List<Billing>();

    public ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();

    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public ICollection<Renewal> Renewals { get; set; } = new List<Renewal>();
}
