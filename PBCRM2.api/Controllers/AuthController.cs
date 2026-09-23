using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using PBCRM2.Domain.Constants;
using PBCRM2.Infrastructure.Identity;
using PBCRM2.Infrastructure.Services;

namespace PBCRM2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        JwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    // ============================================================
    // LOGIN
    // POST: api/Auth/login
    // ============================================================
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
            await _userManager.FindByNameAsync(request.Username.Trim());

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Invalid username or password."
            });
        }

        // --------------------------------------------------------
        // Check whether the account is deactivated.
        // --------------------------------------------------------
        if (await _userManager.IsLockedOutAsync(user))
        {
            return Unauthorized(new
            {
                message = "This account has been deactivated."
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

        var roles =
            await _userManager.GetRolesAsync(user);

        if (roles.Count == 0)
        {
            return Unauthorized(new
            {
                message =
                    "This account has no assigned role. Please contact the Super Admin."
            });
        }

        var token =
            await _jwtTokenService.CreateTokenAsync(user);

        return Ok(new
        {
            message = "Login successful.",
            token,
            userId = user.Id,
            username = user.UserName,
            fullName = user.FullName,
            email = user.Email,
            companyId = user.CompanyId,
            branchId = user.BranchId,
            roles
        });
    }
}

// ================================================================
// LOGIN REQUEST
// ================================================================
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}