using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PBCRM2.Domain.Constants;
using PBCRM2.Infrastructure.Identity;
using PBCRM2.Infrastructure.Services;

namespace PBCRM2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        JwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest(new
            {
                message = "Username, password, and full name are required."
            });
        }

        var existingUser =
            await _userManager.FindByNameAsync(request.Username);

        if (existingUser != null)
        {
            return BadRequest(new
            {
                message = "Username is already registered."
            });
        }

        var user = new ApplicationUser
        {
            UserName = request.Username,
            Email = request.Email,
            FullName = request.FullName,
            BranchId = request.BranchId
        };

        var result = await _userManager.CreateAsync(
            user,
            request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Registration failed.",
                errors = result.Errors.Select(e => e.Description)
            });
        }

        var role = string.IsNullOrWhiteSpace(request.Role)
            ? UserRoles.Staff
            : request.Role;

        if (!await _roleManager.RoleExistsAsync(role))
        {
            await _userManager.DeleteAsync(user);

            return BadRequest(new
            {
                message = "The specified role does not exist."
            });
        }

        await _userManager.AddToRoleAsync(user, role);

        return Ok(new
        {
            message = "User registered successfully.",
            username = user.UserName,
            role
        });
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                message = "Username and password are required."
            });
        }

        var user =
            await _userManager.FindByNameAsync(request.Username);

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Invalid username or password."
            });
        }

        var passwordValid =
            await _userManager.CheckPasswordAsync(
                user,
                request.Password);

        if (!passwordValid)
        {
            return Unauthorized(new
            {
                message = "Invalid username or password."
            });
        }

        var roles = await _userManager.GetRolesAsync(user);

        // Generate JWT token
        var token = await _jwtTokenService.CreateTokenAsync(user);

        return Ok(new
        {
            message = "Login successful.",
            token,
            userId = user.Id,
            username = user.UserName,
            fullName = user.FullName,
            email = user.Email,
            branchId = user.BranchId,
            roles
        });
    }
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int? BranchId { get; set; }

    public string Role { get; set; } = UserRoles.Staff;
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}