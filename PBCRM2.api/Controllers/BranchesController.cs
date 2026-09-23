using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBCRM2.Domain.Entities;
using PBCRM2.Infrastructure.Identity;
using PBCRM2.Infrastructure.Services;
using System.Security.Claims;

namespace PBCRM2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class BranchesController : ControllerBase
{
    // ============================================================
    // SERVICES
    // ============================================================

    private readonly ITenantDbContextFactory _tenantDbFactory;
    private readonly UserManager<ApplicationUser> _userManager;

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public BranchesController(
        ITenantDbContextFactory tenantDbFactory,
        UserManager<ApplicationUser> userManager)
    {
        _tenantDbFactory = tenantDbFactory;
        _userManager = userManager;
    }

    // ============================================================
    // CURRENT COMPANY ID
    // ============================================================

    private int? CurrentCompanyId
    {
        get
        {
            var companyId =
                User.FindFirstValue("CompanyId");

            if (int.TryParse(companyId, out var id) &&
                id > 0)
            {
                return id;
            }

            return null;
        }
    }

    // ============================================================
    // CURRENT USER
    // ============================================================

    private async Task<ApplicationUser?> CurrentUserAsync()
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await _userManager.FindByIdAsync(userId);
    }

    // ============================================================
    // GET TENANT DATABASE
    // ============================================================

    private async Task<
        PBCRM2.Infrastructure.Data.TenantCRMDbContext?>
        GetTenantDbAsync()
    {
        var companyId = CurrentCompanyId;

        if (!companyId.HasValue)
        {
            return null;
        }

        return await _tenantDbFactory
            .CreateAsync(companyId.Value);
    }

    // ============================================================
    // GET COMPANY MANAGERS
    // ============================================================

    private async Task<List<ApplicationUser>>
        GetCompanyManagersAsync()
    {
        var companyId = CurrentCompanyId;

        if (!companyId.HasValue)
        {
            return new List<ApplicationUser>();
        }

        var companyUsers =
            await _userManager.Users
                .Where(x =>
                    x.CompanyId == companyId.Value)
                .ToListAsync();

        var managers =
            new List<ApplicationUser>();

        foreach (var user in companyUsers)
        {
            if (await _userManager.IsInRoleAsync(
                user,
                "Manager"))
            {
                managers.Add(user);
            }
        }

        return managers;
    }

    // ============================================================
    // FIND MANAGER FOR BRANCH
    // ============================================================

    private async Task<ApplicationUser?>
        GetBranchManagerAsync(int branchId)
    {
        var managers =
            await GetCompanyManagersAsync();

        return managers.FirstOrDefault(
            x => x.BranchId == branchId);
    }

    // ============================================================
    // VALIDATE EXISTING MANAGER
    // ============================================================

    private async Task<(
        bool Success,
        ApplicationUser? Manager,
        string ErrorMessage)>
        ValidateManagerAsync(
            string? managerId)
    {
        if (string.IsNullOrWhiteSpace(managerId))
        {
            return (
                true,
                null,
                string.Empty);
        }

        var manager =
            await _userManager.FindByIdAsync(
                managerId);

        if (manager == null)
        {
            return (
                false,
                null,
                "The selected manager could not be found.");
        }

        var companyId = CurrentCompanyId;

        if (!companyId.HasValue)
        {
            return (
                false,
                null,
                "Your account is not assigned to a company.");
        }

        if (manager.CompanyId != companyId.Value)
        {
            return (
                false,
                null,
                "The selected manager does not belong to your company.");
        }

        var isManager =
            await _userManager.IsInRoleAsync(
                manager,
                "Manager");

        if (!isManager)
        {
            return (
                false,
                null,
                "The selected user does not have the Manager role.");
        }

        return (
            true,
            manager,
            string.Empty);
    }

    // ============================================================
    // ASSIGN EXISTING MANAGER
    // ============================================================

    private async Task<(
        bool Success,
        string ErrorMessage)>
        AssignExistingManagerToBranchAsync(
            int branchId,
            string? managerId)
    {
        var validation =
            await ValidateManagerAsync(managerId);

        if (!validation.Success)
        {
            return (
                false,
                validation.ErrorMessage);
        }

        var selectedManager =
            validation.Manager;

        var managers =
            await GetCompanyManagersAsync();

        // --------------------------------------------------------
        // CHECK SELECTED MANAGER BEFORE CHANGING ANYTHING
        // --------------------------------------------------------

        if (selectedManager != null &&
            selectedManager.BranchId.HasValue &&
            selectedManager.BranchId.Value != branchId)
        {
            return (
                false,
                "This manager is already assigned to another branch. " +
                "Create a new Manager account for the new branch instead.");
        }

        // --------------------------------------------------------
        // SAVE CURRENT MANAGER
        // --------------------------------------------------------

        var currentBranchManagers =
            managers
                .Where(x =>
                    x.BranchId == branchId)
                .ToList();

        // --------------------------------------------------------
        // IF SAME MANAGER, NOTHING TO CHANGE
        // --------------------------------------------------------

        if (selectedManager != null &&
            currentBranchManagers.Any(
                x => x.Id == selectedManager.Id))
        {
            return (
                true,
                string.Empty);
        }

        // --------------------------------------------------------
        // REMOVE CURRENT MANAGER
        // --------------------------------------------------------

        var unassignedManagers =
            new List<ApplicationUser>();

        foreach (var currentManager in currentBranchManagers)
        {
            currentManager.BranchId = null;

            var updateResult =
                await _userManager.UpdateAsync(
                    currentManager);

            if (!updateResult.Succeeded)
            {
                var errors =
                    string.Join(
                        " | ",
                        updateResult.Errors.Select(
                            x =>
                                $"{x.Code}: {x.Description}"));

                return (
                    false,
                    $"The current branch manager could not be unassigned. {errors}");
            }

            unassignedManagers.Add(
                currentManager);
        }

        // --------------------------------------------------------
        // ASSIGN SELECTED MANAGER
        // --------------------------------------------------------

        if (selectedManager != null)
        {
            selectedManager.BranchId =
                branchId;

            var updateResult =
                await _userManager.UpdateAsync(
                    selectedManager);

            if (!updateResult.Succeeded)
            {
                // -----------------------------------------------
                // TRY TO RESTORE PREVIOUS MANAGER
                // -----------------------------------------------

                foreach (var previousManager
                    in unassignedManagers)
                {
                    previousManager.BranchId =
                        branchId;

                    await _userManager.UpdateAsync(
                        previousManager);
                }

                var errors =
                    string.Join(
                        " | ",
                        updateResult.Errors.Select(
                            x =>
                                $"{x.Code}: {x.Description}"));

                return (
                    false,
                    $"The manager could not be assigned to this branch. {errors}");
            }
        }

        return (
            true,
            string.Empty);
    }

    // ============================================================
    // GET ALL BRANCHES
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeInactive = false)
    {
        var user =
            await CurrentUserAsync();

        if (user == null)
        {
            return Unauthorized();
        }

        var companyId = CurrentCompanyId;

        if (!companyId.HasValue)
        {
            return Unauthorized(
                "Your account is not assigned to a company.");
        }

        var db =
            await GetTenantDbAsync();

        if (db == null)
        {
            return Unauthorized(
                "Your account is not assigned to a company.");
        }

        await using (db)
        {
            var query =
                db.Branches
                    .AsNoTracking()
                    .AsQueryable();

            // ----------------------------------------------------
            // MANAGER
            // ----------------------------------------------------

            if (User.IsInRole("Manager"))
            {
                if (!user.BranchId.HasValue)
                {
                    return BadRequest(
                        "Your account is not assigned to a branch.");
                }

                query =
                    query.Where(
                        b =>
                            b.Id ==
                            user.BranchId.Value &&
                            b.IsActive);
            }
            else
            {
                // ------------------------------------------------
                // ADMIN
                // ------------------------------------------------

                if (!includeInactive)
                {
                    query =
                        query.Where(
                            b => b.IsActive);
                }
            }

            var branches =
                await query
                    .OrderBy(
                        b => b.BranchName)
                    .Select(
                        b => new
                        {
                            b.Id,
                            b.BranchName,
                            b.Address,
                            b.ContactNumber,
                            b.IsActive
                        })
                    .ToListAsync();

            var managers =
                await GetCompanyManagersAsync();

            var result =
                branches
                    .Select(
                        branch =>
                        {
                            var manager =
                                managers.FirstOrDefault(
                                    m =>
                                        m.BranchId ==
                                        branch.Id);

                            return new
                            {
                                branch.Id,
                                branch.BranchName,
                                branch.Address,
                                branch.ContactNumber,
                                branch.IsActive,

                                ManagerId =
                                    manager?.Id,

                                ManagerName =
                                    GetManagerDisplayName(
                                        manager)
                            };
                        })
                    .ToList();

            return Ok(result);
        }
    }

    // ============================================================
    // GET MANAGERS
    // ============================================================

    [HttpGet("managers")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetManagers()
    {
        var db =
            await GetTenantDbAsync();

        if (db == null)
        {
            return Unauthorized(
                "Your account is not assigned to a company.");
        }

        await using (db)
        {
            var branches =
                await db.Branches
                    .AsNoTracking()
                    .ToListAsync();

            var managers =
                await GetCompanyManagersAsync();

            var result =
                managers
                    .OrderBy(
                        x =>
                            string.IsNullOrWhiteSpace(
                                x.FullName)
                                ? x.UserName
                                : x.FullName)
                    .Select(
                        manager =>
                        {
                            var branchName =
                                string.Empty;

                            if (manager.BranchId.HasValue)
                            {
                                branchName =
                                    branches
                                        .FirstOrDefault(
                                            b =>
                                                b.Id ==
                                                manager.BranchId.Value)
                                        ?.BranchName
                                    ?? string.Empty;
                            }

                            return new
                            {
                                Id = manager.Id,

                                FullName =
                                    GetManagerDisplayName(
                                        manager),

                                Email =
                                    manager.Email ??
                                    string.Empty,

                                BranchId =
                                    manager.BranchId,

                                BranchName =
                                    branchName
                            };
                        })
                    .ToList();

            return Ok(result);
        }
    }

    // ============================================================
    // GET BRANCH BY ID
    // ============================================================

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id)
    {
        var user =
            await CurrentUserAsync();

        if (user == null)
        {
            return Unauthorized();
        }

        var db =
            await GetTenantDbAsync();

        if (db == null)
        {
            return Unauthorized(
                "Your account is not assigned to a company.");
        }

        await using (db)
        {
            var query =
                db.Branches
                    .AsNoTracking()
                    .Where(
                        b => b.Id == id);

            if (User.IsInRole("Manager"))
            {
                if (!user.BranchId.HasValue)
                {
                    return BadRequest(
                        "Your account is not assigned to a branch.");
                }

                query =
                    query.Where(
                        b =>
                            b.Id ==
                            user.BranchId.Value &&
                            b.IsActive);
            }

            var branch =
                await query.FirstOrDefaultAsync();

            if (branch == null)
            {
                return NotFound(
                    "Branch not found.");
            }

            var manager =
                await GetBranchManagerAsync(
                    branch.Id);

            return Ok(
                new
                {
                    branch.Id,
                    branch.BranchName,
                    branch.Address,
                    branch.ContactNumber,
                    branch.IsActive,

                    ManagerId =
                        manager?.Id,

                    ManagerName =
                        GetManagerDisplayName(
                            manager)
                });
        }
    }

    // ============================================================
    // CREATE BRANCH + OPTIONAL NEW MANAGER
    //
    // POST: api/Branches
    //
    // ADMIN ONLY
    // ============================================================

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        [FromBody] BranchRequest request)
    {
        if (request == null)
        {
            return BadRequest(
                "Branch information is required.");
        }

        // ========================================================
        // BASIC BRANCH VALIDATION
        // ========================================================

        var branchName =
            request.BranchName?.Trim();

        var address =
            request.Address?.Trim();

        var contactNumber =
            request.ContactNumber?.Trim();

        if (string.IsNullOrWhiteSpace(branchName))
        {
            return BadRequest(
                "Branch name is required.");
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            return BadRequest(
                "Branch address is required.");
        }

        if (string.IsNullOrWhiteSpace(contactNumber))
        {
            return BadRequest(
                "Branch contact number is required.");
        }

        // ========================================================
        // COMPANY
        // ========================================================

        var companyId =
            CurrentCompanyId;

        if (!companyId.HasValue)
        {
            return Unauthorized(
                "Your account is not assigned to a company.");
        }

        // ========================================================
        // DATABASE
        // ========================================================

        var db =
            await GetTenantDbAsync();

        if (db == null)
        {
            return Unauthorized(
                "Your account is not assigned to a company.");
        }

        await using (db)
        {
            // ====================================================
            // DUPLICATE BRANCH NAME
            // ====================================================

            var branchNameExists =
                await db.Branches.AnyAsync(
                    b =>
                        b.IsActive &&
                        b.BranchName.ToLower() ==
                        branchName.ToLower());

            if (branchNameExists)
            {
                return BadRequest(
                    "A branch with this name already exists.");
            }

            // ====================================================
            // PREPARE NEW MANAGER
            // ====================================================

            ApplicationUser? newManager =
                null;

            string? managerPassword =
                null;

            if (request.CreateNewManager)
            {
                // ------------------------------------------------
                // FULL NAME
                // ------------------------------------------------

                var managerFullName =
                    request.ManagerFullName?.Trim();

                if (string.IsNullOrWhiteSpace(
                    managerFullName))
                {
                    return BadRequest(
                        "Manager full name is required.");
                }

                // ------------------------------------------------
                // USERNAME
                // ------------------------------------------------

                var username =
                    request.ManagerUsername?.Trim();

                if (string.IsNullOrWhiteSpace(username))
                {
                    return BadRequest(
                        "Manager username is required.");
                }

                if (username.Length < 3)
                {
                    return BadRequest(
                        "Manager username must contain at least 3 characters.");
                }

                // ------------------------------------------------
                // EMAIL
                // ------------------------------------------------

                var email =
                    request.ManagerEmail?.Trim();

                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest(
                        "Manager email is required.");
                }

                try
                {
                    var mailAddress =
                        new System.Net.Mail.MailAddress(
                            email);

                    if (!mailAddress.Address.Equals(
                        email,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return BadRequest(
                            "Please provide a valid Manager email address.");
                    }
                }
                catch
                {
                    return BadRequest(
                        "Please provide a valid Manager email address.");
                }

                // ------------------------------------------------
                // PASSWORD
                // ------------------------------------------------

                managerPassword =
                    request.ManagerPassword;

                if (string.IsNullOrWhiteSpace(
                    managerPassword))
                {
                    return BadRequest(
                        "Manager password is required.");
                }

                // Matches Program.cs Identity configuration.
                var passwordError =
                    ValidatePasswordPolicy(
                        managerPassword);

                if (passwordError != null)
                {
                    return BadRequest(
                        passwordError);
                }

                // ------------------------------------------------
                // CONFIRM PASSWORD
                // ------------------------------------------------

                if (string.IsNullOrWhiteSpace(
                    request.ManagerConfirmPassword))
                {
                    return BadRequest(
                        "Manager password confirmation is required.");
                }

                if (managerPassword !=
                    request.ManagerConfirmPassword)
                {
                    return BadRequest(
                        "Manager passwords do not match.");
                }

                // ------------------------------------------------
                // USERNAME DUPLICATE
                // ------------------------------------------------

                var existingUsername =
                    await _userManager.FindByNameAsync(
                        username);

                if (existingUsername != null)
                {
                    return BadRequest(
                        "That Manager username is already in use.");
                }

                // ------------------------------------------------
                // EMAIL DUPLICATE
                // ------------------------------------------------

                var existingEmail =
                    await _userManager.FindByEmailAsync(
                        email);

                if (existingEmail != null)
                {
                    return BadRequest(
                        "That Manager email is already in use.");
                }

                // =================================================
                // CREATE IDENTITY USER FIRST
                // =================================================

                newManager =
                    new ApplicationUser
                    {
                        UserName =
                            username,

                        Email =
                            email,

                        FullName =
                            managerFullName,

                        CompanyId =
                            companyId.Value,

                        // Branch is assigned AFTER branch creation.
                        BranchId =
                            null,

                        EmailConfirmed =
                            true
                    };

                var createResult =
                    await _userManager.CreateAsync(
                        newManager,
                        managerPassword);

                if (!createResult.Succeeded)
                {
                    var errors =
                        FormatIdentityErrors(
                            createResult);

                    return BadRequest(
                        $"Manager account could not be created. {errors}");
                }

                // =================================================
                // ADD MANAGER ROLE
                // =================================================

                var roleResult =
                    await _userManager.AddToRoleAsync(
                        newManager,
                        "Manager");

                if (!roleResult.Succeeded)
                {
                    var errors =
                        FormatIdentityErrors(
                            roleResult);

                    // ---------------------------------------------
                    // CLEAN UP USER
                    // ---------------------------------------------

                    await _userManager.DeleteAsync(
                        newManager);

                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new
                        {
                            message =
                                "The Manager account was created, but the Manager role could not be assigned.",

                            details =
                                errors
                        });
                }
            }
            else
            {
                // =================================================
                // EXISTING MANAGER MODE
                // =================================================

                if (!string.IsNullOrWhiteSpace(
                    request.ManagerId))
                {
                    var validation =
                        await ValidateManagerAsync(
                            request.ManagerId);

                    if (!validation.Success)
                    {
                        return BadRequest(
                            validation.ErrorMessage);
                    }

                    if (validation.Manager != null &&
                        validation.Manager.BranchId.HasValue)
                    {
                        return BadRequest(
                            "The selected manager is already assigned to another branch. " +
                            "Create a new Manager account for this branch instead.");
                    }
                }
            }

            // ====================================================
            // CREATE BRANCH
            // ====================================================

            var branch =
                new Branch
                {
                    BranchName =
                        branchName,

                    Address =
                        address,

                    ContactNumber =
                        contactNumber,

                    IsActive =
                        true
                };

            try
            {
                db.Branches.Add(
                    branch);

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // ------------------------------------------------
                // BRANCH CREATION FAILED
                // DELETE NEW IDENTITY USER
                // ------------------------------------------------

                if (newManager != null)
                {
                    await _userManager.DeleteAsync(
                        newManager);
                }

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message =
                            "The branch could not be created.",

                        details =
                            ex.GetBaseException().Message
                    });
            }

            // ====================================================
            // ASSIGN NEW MANAGER TO CREATED BRANCH
            // ====================================================

            if (newManager != null)
            {
                newManager.BranchId =
                    branch.Id;

                var updateResult =
                    await _userManager.UpdateAsync(
                        newManager);

                if (!updateResult.Succeeded)
                {
                    var errors =
                        FormatIdentityErrors(
                            updateResult);

                    // ---------------------------------------------
                    // DELETE MANAGER
                    // ---------------------------------------------

                    await _userManager.DeleteAsync(
                        newManager);

                    // ---------------------------------------------
                    // DELETE BRANCH
                    // ---------------------------------------------

                    try
                    {
                        db.Branches.Remove(
                            branch);

                        await db.SaveChangesAsync();
                    }
                    catch
                    {
                        // The original error is more important.
                    }

                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        new
                        {
                            message =
                                "The Manager account was created, but the branch could not be assigned to the Manager.",

                            details =
                                errors
                        });
                }
            }

            // ====================================================
            // ASSIGN EXISTING MANAGER
            // ====================================================

            else if (!string.IsNullOrWhiteSpace(
                request.ManagerId))
            {
                var assignmentResult =
                    await AssignExistingManagerToBranchAsync(
                        branch.Id,
                        request.ManagerId);

                if (!assignmentResult.Success)
                {
                    // ---------------------------------------------
                    // ROLLBACK BRANCH
                    // ---------------------------------------------

                    try
                    {
                        db.Branches.Remove(
                            branch);

                        await db.SaveChangesAsync();
                    }
                    catch
                    {
                        // Preserve original assignment error.
                    }

                    return BadRequest(
                        "The branch could not be created because manager assignment failed. " +
                        assignmentResult.ErrorMessage);
                }
            }

            // ====================================================
            // GET FINAL MANAGER
            // ====================================================

            var assignedManager =
                await GetBranchManagerAsync(
                    branch.Id);

            // ====================================================
            // SUCCESS
            // ====================================================

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    id = branch.Id
                },
                new
                {
                    message =
                        "Branch and Manager account created successfully.",

                    branch.Id,

                    branch.BranchName,

                    branch.Address,

                    branch.ContactNumber,

                    branch.IsActive,

                    ManagerId =
                        assignedManager?.Id,

                    ManagerName =
                        GetManagerDisplayName(
                            assignedManager)
                });
        }
    }

    // ============================================================
    // UPDATE BRANCH
    // ============================================================

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] BranchRequest request)
    {
        if (request == null)
        {
            return BadRequest(
                "Branch information is required.");
        }

        var branchName =
            request.BranchName?.Trim();

        var address =
            request.Address?.Trim();

        var contactNumber =
            request.ContactNumber?.Trim();

        if (string.IsNullOrWhiteSpace(branchName))
        {
            return BadRequest(
                "Branch name is required.");
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            return BadRequest(
                "Branch address is required.");
        }

        if (string.IsNullOrWhiteSpace(contactNumber))
        {
            return BadRequest(
                "Branch contact number is required.");
        }

        var user =
            await CurrentUserAsync();

        if (user == null)
        {
            return Unauthorized();
        }

        // --------------------------------------------------------
        // MANAGER CANNOT CHANGE MANAGER
        // --------------------------------------------------------

        if (User.IsInRole("Manager"))
        {
            if (request.CreateNewManager ||
                !string.IsNullOrWhiteSpace(
                    request.ManagerId))
            {
                return Forbid();
            }
        }

        var db =
            await GetTenantDbAsync();

        if (db == null)
        {
            return Unauthorized(
                "Your account is not assigned to a company.");
        }

        await using (db)
        {
            var query =
                db.Branches
                    .Where(
                        b => b.Id == id);

            if (User.IsInRole("Manager"))
            {
                if (!user.BranchId.HasValue)
                {
                    return BadRequest(
                        "Your account is not assigned to a branch.");
                }

                query =
                    query.Where(
                        b =>
                            b.Id ==
                            user.BranchId.Value);
            }

            var branch =
                await query.FirstOrDefaultAsync();

            if (branch == null)
            {
                return NotFound(
                    "Branch not found.");
            }

            // ----------------------------------------------------
            // DUPLICATE NAME
            // ----------------------------------------------------

            var duplicateName =
                await db.Branches.AnyAsync(
                    b =>
                        b.Id != id &&
                        b.IsActive &&
                        b.BranchName.ToLower() ==
                        branchName.ToLower());

            if (duplicateName)
            {
                return BadRequest(
                    "A branch with this name already exists.");
            }

            // ----------------------------------------------------
            // SAVE OLD VALUES
            // ----------------------------------------------------

            var oldBranchName =
                branch.BranchName;

            var oldAddress =
                branch.Address;

            var oldContactNumber =
                branch.ContactNumber;

            // ----------------------------------------------------
            // UPDATE BRANCH
            // ----------------------------------------------------

            branch.BranchName =
                branchName;

            branch.Address =
                address;

            branch.ContactNumber =
                contactNumber;

            await db.SaveChangesAsync();

            // ----------------------------------------------------
            // ADMIN MANAGER UPDATE
            // ----------------------------------------------------

            if (User.IsInRole("Admin"))
            {
                if (request.CreateNewManager)
                {
                    branch.BranchName =
                        oldBranchName;

                    branch.Address =
                        oldAddress;

                    branch.ContactNumber =
                        oldContactNumber;

                    await db.SaveChangesAsync();

                    return BadRequest(
                        "Creating a new Manager account is only available when adding a new branch.");
                }

                var assignmentResult =
                    await AssignExistingManagerToBranchAsync(
                        branch.Id,
                        request.ManagerId);

                if (!assignmentResult.Success)
                {
                    branch.BranchName =
                        oldBranchName;

                    branch.Address =
                        oldAddress;

                    branch.ContactNumber =
                        oldContactNumber;

                    await db.SaveChangesAsync();

                    return BadRequest(
                        $"Branch information was not saved because the manager assignment failed. {assignmentResult.ErrorMessage}");
                }
            }

            var manager =
                await GetBranchManagerAsync(
                    branch.Id);

            return Ok(
                new
                {
                    message =
                        "Branch updated successfully.",

                    branch.Id,

                    branch.BranchName,

                    branch.Address,

                    branch.ContactNumber,

                    branch.IsActive,

                    ManagerId =
                        manager?.Id,

                    ManagerName =
                        GetManagerDisplayName(
                            manager)
                });
        }
    }

    // ============================================================
    // DEACTIVATE BRANCH
    // ============================================================

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(
        int id)
    {
        var db =
            await GetTenantDbAsync();

        if (db == null)
        {
            return Unauthorized(
                "Your account is not assigned to a company.");
        }

        await using (db)
        {
            var branch =
                await db.Branches.FirstOrDefaultAsync(
                    b => b.Id == id);

            if (branch == null)
            {
                return NotFound(
                    "Branch not found.");
            }

            if (!branch.IsActive)
            {
                return BadRequest(
                    "This branch is already inactive.");
            }

            branch.IsActive =
                false;

            await db.SaveChangesAsync();

            var manager =
                await GetBranchManagerAsync(
                    branch.Id);

            return Ok(
                new
                {
                    message =
                        "Branch deactivated successfully.",

                    branch.Id,

                    branch.BranchName,

                    branch.IsActive,

                    ManagerId =
                        manager?.Id,

                    ManagerName =
                        GetManagerDisplayName(
                            manager)
                });
        }
    }

    // ============================================================
    // ACTIVATE BRANCH
    // ============================================================

    [HttpPut("{id:int}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(
        int id)
    {
        var db =
            await GetTenantDbAsync();

        if (db == null)
        {
            return Unauthorized(
                "Your account is not assigned to a company.");
        }

        await using (db)
        {
            var branch =
                await db.Branches.FirstOrDefaultAsync(
                    b => b.Id == id);

            if (branch == null)
            {
                return NotFound(
                    "Branch not found.");
            }

            if (branch.IsActive)
            {
                return BadRequest(
                    "This branch is already active.");
            }

            branch.IsActive =
                true;

            await db.SaveChangesAsync();

            var manager =
                await GetBranchManagerAsync(
                    branch.Id);

            return Ok(
                new
                {
                    message =
                        "Branch activated successfully.",

                    branch.Id,

                    branch.BranchName,

                    branch.IsActive,

                    ManagerId =
                        manager?.Id,

                    ManagerName =
                        GetManagerDisplayName(
                            manager)
                });
        }
    }

    // ============================================================
    // PASSWORD VALIDATION
    // ============================================================

    private static string? ValidatePasswordPolicy(
        string password)
    {
        // Must match Program.cs Identity settings:
        //
        // RequireDigit = true
        // RequireLowercase = true
        // RequireUppercase = true
        // RequireNonAlphanumeric = true
        // RequiredLength = 8

        if (password.Length < 8)
        {
            return
                "Manager password must be at least 8 characters.";
        }

        if (!password.Any(
            char.IsUpper))
        {
            return
                "Manager password must contain at least one uppercase letter.";
        }

        if (!password.Any(
            char.IsLower))
        {
            return
                "Manager password must contain at least one lowercase letter.";
        }

        if (!password.Any(
            char.IsDigit))
        {
            return
                "Manager password must contain at least one number.";
        }

        if (!password.Any(
            x => !char.IsLetterOrDigit(x)))
        {
            return
                "Manager password must contain at least one special character.";
        }

        return null;
    }

    // ============================================================
    // FORMAT IDENTITY ERRORS
    // ============================================================

    private static string FormatIdentityErrors(
        IdentityResult result)
    {
        return string.Join(
            " | ",
            result.Errors.Select(
                x =>
                    $"{x.Code}: {x.Description}"));
    }

    // ============================================================
    // MANAGER DISPLAY NAME
    // ============================================================

    private static string GetManagerDisplayName(
        ApplicationUser? manager)
    {
        if (manager == null)
        {
            return "Unassigned";
        }

        if (!string.IsNullOrWhiteSpace(
            manager.FullName))
        {
            return manager.FullName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(
            manager.UserName))
        {
            return manager.UserName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(
            manager.Email))
        {
            return manager.Email.Trim();
        }

        return "Unnamed Manager";
    }

    // ============================================================
    // BRANCH REQUEST
    // ============================================================

    public class BranchRequest
    {
        // ========================================================
        // BRANCH
        // ========================================================

        public string BranchName { get; set; } =
            string.Empty;

        public string Address { get; set; } =
            string.Empty;

        public string ContactNumber { get; set; } =
            string.Empty;

        // ========================================================
        // EXISTING MANAGER
        // ========================================================

        public string? ManagerId { get; set; }

        // ========================================================
        // NEW MANAGER
        // ========================================================

        public bool CreateNewManager { get; set; }

        public string? ManagerFullName { get; set; }

        public string? ManagerUsername { get; set; }

        public string? ManagerEmail { get; set; }

        public string? ManagerPassword { get; set; }

        public string? ManagerConfirmPassword { get; set; }
    }
}