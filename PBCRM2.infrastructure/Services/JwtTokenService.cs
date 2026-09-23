using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PBCRM2.Infrastructure.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PBCRM2.Infrastructure.Services;

public class JwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;

    public JwtTokenService(
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager)
    {
        _configuration = configuration;
        _userManager = userManager;
    }

    // =========================================================
    // CREATE JWT TOKEN
    // =========================================================

    public async Task<string> CreateTokenAsync(
        ApplicationUser user)
    {
        // -----------------------------------------------------
        // GET ROLES
        // -----------------------------------------------------

        var roles =
            await _userManager.GetRolesAsync(user);

        // -----------------------------------------------------
        // GET JWT SETTINGS
        // -----------------------------------------------------

        var jwtKey =
            _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT key is not configured.");

        var jwtIssuer =
            _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer is not configured.");

        var jwtAudience =
            _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience is not configured.");

        // -----------------------------------------------------
        // CREATE CLAIMS
        // -----------------------------------------------------

        var claims =
            new List<Claim>
            {
                // User identity
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    user.Id),

                new Claim(
                    JwtRegisteredClaimNames.UniqueName,
                    user.UserName ?? string.Empty),

                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id),

                new Claim(
                    ClaimTypes.Name,
                    user.UserName ?? string.Empty),

                // Full name
                new Claim(
                    "FullName",
                    user.FullName ?? string.Empty)
            };

        // -----------------------------------------------------
        // COMPANY CLAIM
        // -----------------------------------------------------
        // This identifies the System Tenant / Company.
        //
        // It is NOT the boarding-house renter TenantId.
        // -----------------------------------------------------

        if (user.CompanyId.HasValue)
        {
            claims.Add(
                new Claim(
                    "CompanyId",
                    user.CompanyId.Value.ToString()));
        }

        // -----------------------------------------------------
        // BRANCH CLAIM
        // -----------------------------------------------------
        // Manager/Staff can have a branch.
        // -----------------------------------------------------

        if (user.BranchId.HasValue)
        {
            claims.Add(
                new Claim(
                    "BranchId",
                    user.BranchId.Value.ToString()));
        }

        // -----------------------------------------------------
        // ROLE CLAIMS
        // -----------------------------------------------------

        foreach (var role in roles)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }

        // -----------------------------------------------------
        // CREATE SIGNING KEY
        // -----------------------------------------------------

        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        // -----------------------------------------------------
        // CREATE TOKEN
        // -----------------------------------------------------

        var token =
            new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

        // -----------------------------------------------------
        // RETURN TOKEN STRING
        // -----------------------------------------------------

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}