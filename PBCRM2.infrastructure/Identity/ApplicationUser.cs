using Microsoft.AspNetCore.Identity;

namespace PBCRM2.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public int? BranchId { get; set; }
}