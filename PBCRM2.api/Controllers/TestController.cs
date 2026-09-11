using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBCRM2.Domain.Constants;

namespace PBCRM2.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    // Any authenticated user
    [Authorize]
    [HttpGet("protected")]
    public IActionResult Protected()
    {
        return Ok(new
        {
            message = "JWT authentication is working.",
            username = User.Identity?.Name,
            authenticated = User.Identity?.IsAuthenticated
        });
    }

    // Admin only
    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet("admin")]
    public IActionResult AdminOnly()
    {
        return Ok(new
        {
            message = "Admin authorization is working.",
            username = User.Identity?.Name,
            role = UserRoles.Admin
        });
    }

    // Manager only
    [Authorize(Roles = UserRoles.Manager)]
    [HttpGet("manager")]
    public IActionResult ManagerOnly()
    {
        return Ok(new
        {
            message = "Manager authorization is working.",
            username = User.Identity?.Name,
            role = UserRoles.Manager
        });
    }

    // Staff only
    [Authorize(Roles = UserRoles.Staff)]
    [HttpGet("staff")]
    public IActionResult StaffOnly()
    {
        return Ok(new
        {
            message = "Staff authorization is working.",
            username = User.Identity?.Name,
            role = UserRoles.Staff
        });
    }

    // Super Admin only
    [Authorize(Roles = UserRoles.SuperAdmin)]
    [HttpGet("super-admin")]
    public IActionResult SuperAdminOnly()
    {
        return Ok(new
        {
            message = "Super Admin authorization is working.",
            username = User.Identity?.Name,
            role = UserRoles.SuperAdmin
        });
    }
}