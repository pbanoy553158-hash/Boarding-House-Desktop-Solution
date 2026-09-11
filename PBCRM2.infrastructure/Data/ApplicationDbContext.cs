using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PBCRM2.Domain.Entities;
using PBCRM2.Infrastructure.Identity;

namespace PBCRM2.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<Branch> Branches => Set<Branch>();

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<Bed> Beds => Set<Bed>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();

    public DbSet<Feedback> Feedbacks => Set<Feedback>();

    public DbSet<Renewal> Renewals => Set<Renewal>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Branch → Rooms
        builder.Entity<Room>()
            .HasOne(r => r.Branch)
            .WithMany()
            .HasForeignKey(r => r.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Room → Beds
        builder.Entity<Bed>()
            .HasOne(b => b.Room)
            .WithMany()
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tenant → Bed
        builder.Entity<Bed>()
            .HasOne(b => b.Tenant)
            .WithOne(t => t.Bed)
            .HasForeignKey<Bed>(b => b.TenantId)
            .OnDelete(DeleteBehavior.SetNull);

        // Tenant → Payments
        builder.Entity<Payment>()
            .HasOne(p => p.Tenant)
            .WithMany(t => t.Payments)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tenant → Maintenance Requests
        builder.Entity<MaintenanceRequest>()
            .HasOne(m => m.Tenant)
            .WithMany(t => t.MaintenanceRequests)
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tenant → Feedback
        builder.Entity<Feedback>()
            .HasOne(f => f.Tenant)
            .WithMany(t => t.Feedbacks)
            .HasForeignKey(f => f.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tenant → Renewals
        builder.Entity<Renewal>()
            .HasOne(r => r.Tenant)
            .WithMany(t => t.Renewals)
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Payment amount
        builder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        // Feedback rating
        builder.Entity<Feedback>()
            .ToTable("Feedbacks", table =>
            {
                table.HasCheckConstraint(
                    "CK_Feedback_Rating",
                    "[Rating] >= 1 AND [Rating] <= 5");
            });
    }
}