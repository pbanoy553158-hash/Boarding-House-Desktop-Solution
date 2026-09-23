using System;
using System.Collections.Generic;
using System.Text;

namespace PBCRM2.Domain.Entities;

public class Billing
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public decimal MonthlyRentalAmount { get; set; }

    public DateTime BillingPeriodStart { get; set; }

    public DateTime BillingPeriodEnd { get; set; }

    public DateTime DueDate { get; set; }

    public string Notes { get; set; } = string.Empty;

    public Tenant? Tenant { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
