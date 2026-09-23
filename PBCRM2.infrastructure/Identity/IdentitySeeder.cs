using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using PBCRM2.Domain.Constants;

namespace PBCRM2.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        await SeedRolesAsync(roleManager);
        await SeedSuperAdminAsync(userManager, configuration);
    }

    // =========================================================
    // SEED ROLES
    // =========================================================

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

        foreach (string role in roles)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            IdentityResult result =
                await roleManager.CreateAsync(
                    new IdentityRole(role));

            if (!result.Succeeded)
            {
                string errors = string.Join(
                    "; ",
                    result.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Unable to create role '{role}'. {errors}");
            }

            Console.WriteLine(
                $"Role '{role}' created successfully.");
        }
    }

    // =========================================================
    // SEED SUPER ADMIN
    // =========================================================

    private static async Task SeedSuperAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        IConfigurationSection section =
            configuration.GetSection("SuperAdmin");

        string username =
            section["Username"]
            ?? throw new InvalidOperationException(
                "SuperAdmin:Username is not configured.");

        string fullName =
            section["FullName"]
            ?? throw new InvalidOperationException(
                "SuperAdmin:FullName is not configured.");

        string email =
            section["Email"]
            ?? throw new InvalidOperationException(
                "SuperAdmin:Email is not configured.");

        string password =
            section["Password"]
            ?? throw new InvalidOperationException(
                "SuperAdmin:Password is not configured. " +
                "Add it to User Secrets.");

        // =====================================================
        // FIND EXISTING SUPER ADMIN
        // =====================================================

        ApplicationUser? existingUser =
            await userManager.FindByNameAsync(username);

        if (existingUser != null)
        {
            Console.WriteLine(
                $"Super Admin account '{username}' already exists.");

            bool changed = false;

            // -------------------------------------------------
            // UPDATE FULL NAME
            // -------------------------------------------------

            if (existingUser.FullName != fullName)
            {
                existingUser.FullName = fullName;
                changed = true;
            }

            // -------------------------------------------------
            // UPDATE EMAIL
            // -------------------------------------------------

            if (existingUser.Email != email)
            {
                existingUser.Email = email;
                existingUser.EmailConfirmed = true;
                changed = true;
            }

            // -------------------------------------------------
            // SUPER ADMIN HAS NO COMPANY
            // -------------------------------------------------

            if (existingUser.CompanyId.HasValue)
            {
                existingUser.CompanyId = null;
                changed = true;
            }

            // -------------------------------------------------
            // SUPER ADMIN HAS NO BRANCH
            // -------------------------------------------------

            if (existingUser.BranchId.HasValue)
            {
                existingUser.BranchId = null;
                changed = true;
            }

            // -------------------------------------------------
            // SAVE USER CHANGES
            // -------------------------------------------------

            if (changed)
            {
                IdentityResult updateResult =
                    await userManager.UpdateAsync(existingUser);

                if (!updateResult.Succeeded)
                {
                    string errors = string.Join(
                        "; ",
                        updateResult.Errors.Select(
                            e => e.Description));

                    throw new InvalidOperationException(
                        $"Unable to update Super Admin account. {errors}");
                }

                Console.WriteLine(
                    $"Super Admin account '{username}' updated.");
            }

            // =================================================
            // CHECK CURRENT ROLES
            // =================================================

            IList<string> existingRoles =
                await userManager.GetRolesAsync(existingUser);

            Console.WriteLine(
                $"Current roles for '{username}': " +
                (existingRoles.Count > 0
                    ? string.Join(", ", existingRoles)
                    : "NONE"));

            // =================================================
            // REMOVE INCORRECT ROLES
            // =================================================

            List<string> otherRoles =
                existingRoles
                    .Where(role =>
                        role != UserRoles.SuperAdmin)
                    .ToList();

            if (otherRoles.Count > 0)
            {
                Console.WriteLine(
                    $"Removing incorrect roles from '{username}': " +
                    string.Join(", ", otherRoles));

                IdentityResult removeRolesResult =
                    await userManager.RemoveFromRolesAsync(
                        existingUser,
                        otherRoles);

                if (!removeRolesResult.Succeeded)
                {
                    string errors = string.Join(
                        "; ",
                        removeRolesResult.Errors.Select(
                            e => e.Description));

                    throw new InvalidOperationException(
                        $"Unable to remove incorrect roles " +
                        $"from Super Admin. {errors}");
                }
            }

            // =================================================
            // ASSIGN SUPER ADMIN ROLE
            // =================================================

            if (!existingRoles.Contains(
                    UserRoles.SuperAdmin))
            {
                Console.WriteLine(
                    $"Assigning role '{UserRoles.SuperAdmin}' " +
                    $"to '{existingUser.UserName}'.");

                IdentityResult addRoleResult =
                    await userManager.AddToRoleAsync(
                        existingUser,
                        UserRoles.SuperAdmin);

                if (!addRoleResult.Succeeded)
                {
                    string errors = string.Join(
                        "; ",
                        addRoleResult.Errors.Select(
                            e => e.Description));

                    throw new InvalidOperationException(
                        $"Unable to assign Super Admin role. " +
                        $"{errors}");
                }

                Console.WriteLine(
                    $"Role '{UserRoles.SuperAdmin}' assigned " +
                    $"successfully to '{existingUser.UserName}'.");
            }
            else
            {
                Console.WriteLine(
                    $"User '{existingUser.UserName}' already has " +
                    $"the '{UserRoles.SuperAdmin}' role.");
            }

            return;
        }

        // =====================================================
        // CREATE NEW SUPER ADMIN
        // =====================================================

        ApplicationUser superAdmin =
            new ApplicationUser
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,

                // Super Admin is system-level.
                CompanyId = null,
                BranchId = null
            };

        IdentityResult createResult =
            await userManager.CreateAsync(
                superAdmin,
                password);

        if (!createResult.Succeeded)
        {
            string errors = string.Join(
                "; ",
                createResult.Errors.Select(
                    e => e.Description));

            throw new InvalidOperationException(
                $"Unable to create Super Admin account. {errors}");
        }

        Console.WriteLine(
            $"Super Admin account '{username}' created successfully.");

        // =====================================================
        // ASSIGN SUPER ADMIN ROLE
        // =====================================================

        IdentityResult roleResult =
            await userManager.AddToRoleAsync(
                superAdmin,
                UserRoles.SuperAdmin);

        if (!roleResult.Succeeded)
        {
            string errors = string.Join(
                "; ",
                roleResult.Errors.Select(
                    e => e.Description));

            throw new InvalidOperationException(
                $"Unable to assign Super Admin role. {errors}");
        }

        Console.WriteLine(
            $"Role '{UserRoles.SuperAdmin}' assigned successfully " +
            $"to '{username}'.");
    }
}