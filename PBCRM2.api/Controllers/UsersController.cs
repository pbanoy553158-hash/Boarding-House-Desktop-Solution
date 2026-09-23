using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBCRM2.Domain.Constants;
using PBCRM2.Infrastructure.Data;
using PBCRM2.Infrastructure.Identity;
using PBCRM2.Infrastructure.Services;

namespace PBCRM2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRoles.SuperAdmin)]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _masterDb;
    private readonly ITenantDbContextFactory _tenantDbFactory;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext masterDb,
        ITenantDbContextFactory tenantDbFactory)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _masterDb = masterDb;
        _tenantDbFactory = tenantDbFactory;
    }

    // ============================================================
    // GET: api/Users
    // Get all system users
    // Super Admin only
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .ToListAsync();

        var result = new List<UserResponse>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            result.Add(new UserResponse
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                CompanyId = user.CompanyId,
                BranchId = user.BranchId,
                Roles = roles.ToList(),
                IsLockedOut =
                    await _userManager.IsLockedOutAsync(user)
            });
        }

        return Ok(result);
    }

    // ============================================================
    // GET: api/Users/{id}
    // Get one user
    // ============================================================

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new
            {
                message = "User ID is required."
            });
        }

        var user =
            await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound(new
            {
                message = "User was not found."
            });
        }

        var roles =
            await _userManager.GetRolesAsync(user);

        return Ok(new UserResponse
        {
            Id = user.Id,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            CompanyId = user.CompanyId,
            BranchId = user.BranchId,
            Roles = roles.ToList(),
            IsLockedOut =
                await _userManager.IsLockedOutAsync(user)
        });
    }

    // ============================================================
    // GET: api/Users/companies
    // Get active companies from Master CRM database
    // ============================================================

    [HttpGet("companies")]
    public async Task<IActionResult> GetCompanies()
    {
        var companies = await _masterDb.Companies
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.CompanyName)
            .Select(c => new CompanyResponse
            {
                Id = c.CompanyId,
                CompanyName = c.CompanyName
            })
            .ToListAsync();

        return Ok(companies);
    }

    // ============================================================
    // GET: api/Users/companies/{companyId}/branches
    // Get active branches belonging to a company
    // ============================================================

    [HttpGet("companies/{companyId}/branches")]
    public async Task<IActionResult> GetBranches(
        int companyId)
    {
        if (companyId <= 0)
        {
            return BadRequest(new
            {
                message = "Invalid Company ID."
            });
        }

        try
        {
            await using var tenantDb =
                await _tenantDbFactory.CreateAsync(
                    companyId);

            var branches = await tenantDb.Branches
                .AsNoTracking()
                .Where(b => b.IsActive)
                .OrderBy(b => b.BranchName)
                .Select(b => new BranchResponse
                {
                    Id = b.Id,
                    BranchName = b.BranchName
                })
                .ToListAsync();

            return Ok(branches);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message =
                    "Unable to load branches for this company.",
                error = ex.Message
            });
        }
    }

    // ============================================================
    // POST: api/Users
    // Create a new system user
    // Super Admin only
    // ============================================================

    [HttpPost]
    public async Task<IActionResult> CreateUser(
        CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new
            {
                message = "Username is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                message = "Password is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest(new
            {
                message = "Full name is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Role))
        {
            return BadRequest(new
            {
                message = "Role is required."
            });
        }

        // --------------------------------------------------------
        // VALIDATE ROLE
        // --------------------------------------------------------

        var validRoles = new[]
        {
            UserRoles.SuperAdmin,
            UserRoles.Admin,
            UserRoles.Manager,
            UserRoles.Staff
        };

        if (!validRoles.Contains(
                request.Role,
                StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Invalid role.",
                allowedRoles = validRoles
            });
        }

        var role = validRoles.First(r =>
            r.Equals(
                request.Role,
                StringComparison.OrdinalIgnoreCase));

        // --------------------------------------------------------
        // MAKE SURE ROLE EXISTS
        // --------------------------------------------------------

        if (!await _roleManager.RoleExistsAsync(role))
        {
            return BadRequest(new
            {
                message =
                    $"The role '{role}' does not exist."
            });
        }

        // --------------------------------------------------------
        // PREVENT DUPLICATE USERNAME
        // --------------------------------------------------------

        var existingUser =
            await _userManager.FindByNameAsync(
                request.Username.Trim());

        if (existingUser != null)
        {
            return BadRequest(new
            {
                message =
                    "Username is already registered."
            });
        }

        // --------------------------------------------------------
        // SUPER ADMIN
        // System-level only
        // --------------------------------------------------------

        if (role == UserRoles.SuperAdmin)
        {
            request.CompanyId = null;
            request.BranchId = null;
        }
        else
        {
            // ----------------------------------------------------
            // COMPANY REQUIRED
            // ----------------------------------------------------

            if (!request.CompanyId.HasValue)
            {
                return BadRequest(new
                {
                    message =
                        $"{role} must be assigned to a company."
                });
            }

            var companyExists =
                await _masterDb.Companies
                    .AnyAsync(c =>
                        c.CompanyId ==
                            request.CompanyId.Value &&
                        c.IsActive);

            if (!companyExists)
            {
                return BadRequest(new
                {
                    message =
                        "The selected company does not exist or is inactive."
                });
            }

            // ----------------------------------------------------
            // MANAGER / STAFF REQUIRE BRANCH
            // ----------------------------------------------------

            if (role == UserRoles.Manager ||
                role == UserRoles.Staff)
            {
                if (!request.BranchId.HasValue)
                {
                    return BadRequest(new
                    {
                        message =
                            $"{role} must be assigned to a branch."
                    });
                }

                var branchValid =
                    await ValidateBranchAsync(
                        request.CompanyId.Value,
                        request.BranchId.Value);

                if (!branchValid)
                {
                    return BadRequest(new
                    {
                        message =
                            "The selected branch does not exist or is inactive."
                    });
                }
            }

            // ----------------------------------------------------
            // ADMIN IS COMPANY-WIDE
            // ----------------------------------------------------

            if (role == UserRoles.Admin)
            {
                request.BranchId = null;
            }
        }

        // --------------------------------------------------------
        // CREATE USER
        // --------------------------------------------------------

        var user = new ApplicationUser
        {
            UserName =
                request.Username.Trim(),

            Email =
                string.IsNullOrWhiteSpace(request.Email)
                    ? null
                    : request.Email.Trim(),

            FullName =
                request.FullName.Trim(),

            CompanyId =
                request.CompanyId,

            BranchId =
                request.BranchId,

            EmailConfirmed = false
        };

        var createResult =
            await _userManager.CreateAsync(
                user,
                request.Password);

        if (!createResult.Succeeded)
        {
            return BadRequest(new
            {
                message = "User creation failed.",
                errors =
                    createResult.Errors
                        .Select(e => e.Description)
            });
        }

        // --------------------------------------------------------
        // ASSIGN ROLE
        // --------------------------------------------------------

        var roleResult =
            await _userManager.AddToRoleAsync(
                user,
                role);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            return BadRequest(new
            {
                message =
                    "User was created but the role could not be assigned.",
                errors =
                    roleResult.Errors
                        .Select(e => e.Description)
            });
        }

        return Ok(new
        {
            message =
                "User created successfully.",

            user = new UserResponse
            {
                Id = user.Id,
                Username =
                    user.UserName ?? string.Empty,
                Email =
                    user.Email ?? string.Empty,
                FullName =
                    user.FullName,
                CompanyId =
                    user.CompanyId,
                BranchId =
                    user.BranchId,
                Roles =
                    new List<string> { role },
                IsLockedOut = false
            }
        });
    }

    // ============================================================
    // PUT: api/Users/{id}
    // Update user
    // ============================================================

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(
        string id,
        UpdateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest(new
            {
                message = "User ID is required."
            });
        }

        var user =
            await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound(new
            {
                message = "User was not found."
            });
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest(new
            {
                message = "Full name is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Role))
        {
            return BadRequest(new
            {
                message = "Role is required."
            });
        }

        // --------------------------------------------------------
        // VALIDATE ROLE
        // --------------------------------------------------------

        var validRoles = new[]
        {
            UserRoles.SuperAdmin,
            UserRoles.Admin,
            UserRoles.Manager,
            UserRoles.Staff
        };

        if (!validRoles.Contains(
                request.Role,
                StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Invalid role.",
                allowedRoles = validRoles
            });
        }

        var newRole = validRoles.First(r =>
            r.Equals(
                request.Role,
                StringComparison.OrdinalIgnoreCase));

        if (!await _roleManager.RoleExistsAsync(newRole))
        {
            return BadRequest(new
            {
                message =
                    $"The role '{newRole}' does not exist."
            });
        }

        // --------------------------------------------------------
        // PROTECT CURRENT SUPER ADMIN
        // --------------------------------------------------------

        var currentUserId =
            _userManager.GetUserId(User);

        if (user.Id == currentUserId &&
            newRole != UserRoles.SuperAdmin)
        {
            return BadRequest(new
            {
                message =
                    "The currently logged-in Super Admin cannot change their own role."
            });
        }

        // --------------------------------------------------------
        // SUPER ADMIN
        // --------------------------------------------------------

        if (newRole == UserRoles.SuperAdmin)
        {
            user.CompanyId = null;
            user.BranchId = null;
        }
        else
        {
            // ----------------------------------------------------
            // COMPANY REQUIRED
            // ----------------------------------------------------

            if (!request.CompanyId.HasValue)
            {
                return BadRequest(new
                {
                    message =
                        $"{newRole} must be assigned to a company."
                });
            }

            var companyExists =
                await _masterDb.Companies
                    .AnyAsync(c =>
                        c.CompanyId ==
                            request.CompanyId.Value &&
                        c.IsActive);

            if (!companyExists)
            {
                return BadRequest(new
                {
                    message =
                        "The selected company does not exist or is inactive."
                });
            }

            user.CompanyId =
                request.CompanyId;

            // ----------------------------------------------------
            // MANAGER / STAFF
            // ----------------------------------------------------

            if (newRole == UserRoles.Manager ||
                newRole == UserRoles.Staff)
            {
                if (!request.BranchId.HasValue)
                {
                    return BadRequest(new
                    {
                        message =
                            $"{newRole} must be assigned to a branch."
                    });
                }

                var branchValid =
                    await ValidateBranchAsync(
                        request.CompanyId.Value,
                        request.BranchId.Value);

                if (!branchValid)
                {
                    return BadRequest(new
                    {
                        message =
                            "The selected branch does not exist or is inactive."
                    });
                }

                user.BranchId =
                    request.BranchId;
            }
            else
            {
                // Admin works company-wide.
                user.BranchId = null;
            }
        }

        // --------------------------------------------------------
        // UPDATE BASIC INFORMATION
        // --------------------------------------------------------

        user.FullName =
            request.FullName.Trim();

        user.Email =
            string.IsNullOrWhiteSpace(request.Email)
                ? null
                : request.Email.Trim();

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            user.NormalizedEmail =
                request.Email
                    .Trim()
                    .ToUpperInvariant();
        }
        else
        {
            user.NormalizedEmail = null;
        }

        // --------------------------------------------------------
        // UPDATE USER
        // --------------------------------------------------------

        var updateResult =
            await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return BadRequest(new
            {
                message = "User update failed.",
                errors =
                    updateResult.Errors
                        .Select(e => e.Description)
            });
        }

        // --------------------------------------------------------
        // UPDATE ROLE
        // --------------------------------------------------------

        var currentRoles =
            await _userManager.GetRolesAsync(user);

        if (currentRoles.Count > 0)
        {
            var removeResult =
                await _userManager.RemoveFromRolesAsync(
                    user,
                    currentRoles);

            if (!removeResult.Succeeded)
            {
                return BadRequest(new
                {
                    message =
                        "The user's previous role could not be removed.",
                    errors =
                        removeResult.Errors
                            .Select(e => e.Description)
                });
            }
        }

        var addRoleResult =
            await _userManager.AddToRoleAsync(
                user,
                newRole);

        if (!addRoleResult.Succeeded)
        {
            return BadRequest(new
            {
                message =
                    "The new role could not be assigned.",
                errors =
                    addRoleResult.Errors
                        .Select(e => e.Description)
            });
        }

        return Ok(new
        {
            message =
                "User updated successfully."
        });
    }

    // ============================================================
    // POST: api/Users/{id}/reset-password
    // Reset password
    // ============================================================

    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(
        string id,
        ResetPasswordRequest request)
    {
        var user =
            await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound(new
            {
                message = "User was not found."
            });
        }

        if (string.IsNullOrWhiteSpace(
                request.NewPassword))
        {
            return BadRequest(new
            {
                message =
                    "New password is required."
            });
        }

        var token =
            await _userManager
                .GeneratePasswordResetTokenAsync(user);

        var result =
            await _userManager.ResetPasswordAsync(
                user,
                token,
                request.NewPassword);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message =
                    "Password reset failed.",
                errors =
                    result.Errors
                        .Select(e => e.Description)
            });
        }

        return Ok(new
        {
            message =
                "Password reset successfully."
        });
    }

    // ============================================================
    // POST: api/Users/{id}/deactivate
    // ============================================================

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(
        string id)
    {
        var user =
            await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound(new
            {
                message =
                    "User was not found."
            });
        }

        var currentUserId =
            _userManager.GetUserId(User);

        if (user.Id == currentUserId)
        {
            return BadRequest(new
            {
                message =
                    "The currently logged-in Super Admin cannot deactivate their own account."
            });
        }

        user.LockoutEnabled = true;
        user.LockoutEnd =
            DateTimeOffset.MaxValue;

        var result =
            await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message =
                    "User deactivation failed.",
                errors =
                    result.Errors
                        .Select(e => e.Description)
            });
        }

        return Ok(new
        {
            message =
                "User deactivated successfully."
        });
    }

    // ============================================================
    // POST: api/Users/{id}/activate
    // ============================================================

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateUser(
        string id)
    {
        var user =
            await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound(new
            {
                message =
                    "User was not found."
            });
        }

        user.LockoutEnd = null;

        var result =
            await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message =
                    "User activation failed.",
                errors =
                    result.Errors
                        .Select(e => e.Description)
            });
        }

        return Ok(new
        {
            message =
                "User activated successfully."
        });
    }

    // ============================================================
    // DELETE: api/Users/{id}
    // Deactivate instead of permanent deletion
    // ============================================================

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(
        string id)
    {
        var user =
            await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound(new
            {
                message =
                    "User was not found."
            });
        }

        var currentUserId =
            _userManager.GetUserId(User);

        if (user.Id == currentUserId)
        {
            return BadRequest(new
            {
                message =
                    "The currently logged-in Super Admin cannot delete their own account."
            });
        }

        // Do not physically delete the Identity record.
        // Deactivate the account instead.

        user.LockoutEnabled = true;
        user.LockoutEnd =
            DateTimeOffset.MaxValue;

        var result =
            await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message =
                    "User deactivation failed.",
                errors =
                    result.Errors
                        .Select(e => e.Description)
            });
        }

        return Ok(new
        {
            message =
                "User deactivated successfully."
        });
    }

    // ============================================================
    // VALIDATE BRANCH
    // ============================================================

    private async Task<bool> ValidateBranchAsync(
        int companyId,
        int branchId)
    {
        try
        {
            await using var tenantDb =
                await _tenantDbFactory.CreateAsync(
                    companyId);

            return await tenantDb.Branches
                .AsNoTracking()
                .AnyAsync(b =>
                    b.Id == branchId &&
                    b.IsActive);
        }
        catch
        {
            return false;
        }
    }
}


// =================================================================
// USER RESPONSE
// =================================================================

public class UserResponse
{
    public string Id { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public int? CompanyId { get; set; }

    public int? BranchId { get; set; }

    public List<string> Roles { get; set; } = new();

    public bool IsLockedOut { get; set; }
}


// =================================================================
// COMPANY RESPONSE
// =================================================================

public class CompanyResponse
{
    public int Id { get; set; }

    public string CompanyName { get; set; } = string.Empty;
}


// =================================================================
// BRANCH RESPONSE
// =================================================================

public class BranchResponse
{
    public int Id { get; set; }

    public string BranchName { get; set; } = string.Empty;
}


// =================================================================
// CREATE USER REQUEST
// =================================================================

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int? CompanyId { get; set; }

    public int? BranchId { get; set; }

    public string Role { get; set; } = UserRoles.Staff;
}


// =================================================================
// UPDATE USER REQUEST
// =================================================================

public class UpdateUserRequest
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int? CompanyId { get; set; }

    public int? BranchId { get; set; }

    public string Role { get; set; } = UserRoles.Staff;
}


// =================================================================
// RESET PASSWORD REQUEST
// =================================================================

public class ResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}