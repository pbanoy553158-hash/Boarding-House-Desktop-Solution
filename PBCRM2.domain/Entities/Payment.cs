using System;
using System.Collections.Generic;
using System.Text;

namespace PBCRM2.Domain.Entities;

public class Payment
{
    public int Id { get; set; }

    public int BillingId { get; set; }

    public int TenantId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.Today;

    public string PaymentMethod { get; set; } = "Cash";

    public string Status { get; set; } = "Paid";

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    public Billing? Billing { get; set; }

    public Tenant? Tenant { get; set; }
}
