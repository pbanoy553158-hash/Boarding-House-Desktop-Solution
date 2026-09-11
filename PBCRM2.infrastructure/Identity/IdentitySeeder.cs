using Microsoft.AspNetCore.Identity;
using PBCRM2.Domain.Constants;

namespace PBCRM2.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(
        RoleManager<IdentityRole> roleManager)
    {
        string[] roles =
        {
            UserRoles.SuperAdmin,
            UserRoles.Admin,
            UserRoles.Manager,
            UserRoles.Staff
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(
                    new IdentityRole(role));
            }
        }
    }
}