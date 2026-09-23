using Microsoft.EntityFrameworkCore;
using PBCRM2.Domain.Entities;

namespace PBCRM2.Infrastructure.Data;

public class TenantCRMDbContext : DbContext
{
    public TenantCRMDbContext(
        DbContextOptions<TenantCRMDbContext> options)
        : base(options)
    {
    }

    // =========================================================
    // BOARDING HOUSE TENANT DATABASE TABLES
    // =========================================================

    public DbSet<Tenant> Tenants =>
        Set<Tenant>();

    public DbSet<Branch> Branches =>
        Set<Branch>();

    public DbSet<Room> Rooms =>
        Set<Room>();

    public DbSet<Bed> Beds =>
        Set<Bed>();

    public DbSet<BedAssignmentRequest> BedAssignmentRequests =>
        Set<BedAssignmentRequest>();

    public DbSet<Billing> Billings =>
        Set<Billing>();

    public DbSet<Payment> Payments =>
        Set<Payment>();

    public DbSet<MaintenanceRequest> MaintenanceRequests =>
        Set<MaintenanceRequest>();

    public DbSet<Feedback> Feedbacks =>
        Set<Feedback>();

    public DbSet<FeedbackRequest> FeedbackRequests =>
        Set<FeedbackRequest>();

    public DbSet<Renewal> Renewals =>
        Set<Renewal>();

    // =========================================================
    // MODEL CONFIGURATION
    // =========================================================

    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // =====================================================
        // TENANT
        // =====================================================

        builder.Entity<Tenant>()
            .Property(t => t.CurrentAddress)
            .HasColumnName("Address");

        builder.Entity<Tenant>()
            .HasOne(t => t.Branch)
            .WithMany()
            .HasForeignKey(t => t.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // ROOM
        // =====================================================

        builder.Entity<Room>()
            .HasOne(r => r.Branch)
            .WithMany()
            .HasForeignKey(r => r.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // BED
        // =====================================================

        builder.Entity<Bed>()
            .HasOne(b => b.Room)
            .WithMany(r => r.Beds)
            .HasForeignKey(b => b.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Bed>()
            .HasOne(b => b.Tenant)
            .WithOne(t => t.Bed)
            .HasForeignKey<Bed>(b => b.TenantId)
            .OnDelete(DeleteBehavior.SetNull);

        // =====================================================
        // BED ASSIGNMENT REQUEST
        // =====================================================

        builder.Entity<BedAssignmentRequest>()
            .HasOne(r => r.Bed)
            .WithMany()
            .HasForeignKey(r => r.BedId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BedAssignmentRequest>()
            .HasOne(r => r.Tenant)
            .WithMany()
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // BILLING
        // =====================================================

        builder.Entity<Billing>()
            .HasOne(b => b.Tenant)
            .WithMany(t => t.Billings)
            .HasForeignKey(b => b.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Billing>()
            .Property(b => b.MonthlyRentalAmount)
            .HasPrecision(18, 2);

        // =====================================================
        // PAYMENT
        // =====================================================

        builder.Entity<Payment>()
            .HasOne(p => p.Billing)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BillingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Payment>()
            .HasOne(p => p.Tenant)
            .WithMany(t => t.Payments)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        // =====================================================
        // MAINTENANCE
        // =====================================================

        builder.Entity<MaintenanceRequest>()
            .HasOne(m => m.Tenant)
            .WithMany(t => t.MaintenanceRequests)
            .HasForeignKey(m => m.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // FEEDBACK
        // Existing feedback submitted by tenants
        // =====================================================

        builder.Entity<Feedback>()
            .HasOne(f => f.Tenant)
            .WithMany(t => t.Feedbacks)
            .HasForeignKey(f => f.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Feedback>()
            .ToTable(
                "Feedbacks",
                table =>
                {
                    table.HasCheckConstraint(
                        "CK_Feedback_Rating",
                        "[Rating] >= 1 AND [Rating] <= 5");
                });

        // =====================================================
        // FEEDBACK REQUEST
        // Used for Email + QR Code invitations
        // =====================================================

        builder.Entity<FeedbackRequest>()
            .HasOne(r => r.Tenant)
            .WithMany()
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // FEEDBACK REQUEST TOKEN
        // =====================================================

        builder.Entity<FeedbackRequest>()
            .HasIndex(r => r.Token)
            .IsUnique();

        // =====================================================
        // RENEWAL
        // =====================================================

        builder.Entity<Renewal>()
            .HasOne(r => r.Tenant)
            .WithMany(t => t.Renewals)
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}