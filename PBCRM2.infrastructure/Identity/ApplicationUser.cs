using Microsoft.AspNetCore.Identity;

namespace PBCRM2.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    // =========================================================
    // USER INFORMATION
    // =========================================================

    public string FullName { get; set; } = string.Empty;

    // =========================================================
    // SYSTEM TENANT / COMPANY
    // =========================================================
    // Identifies which boarding-house business this user belongs to.
    //
    // Example:
    // CompanyId = 1
    // → Percy's Boarding House
    //
    // This is NOT the boarding-house renter's TenantId.
    // =========================================================

    public int? CompanyId { get; set; }

    // =========================================================
    // BRANCH
    // =========================================================
    // Used for Manager and Staff branch assignment.
    //
    // Admin/SuperAdmin may have this as null.
    // =========================================================

    public int? BranchId { get; set; }
}