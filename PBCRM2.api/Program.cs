using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PBCRM2.Infrastructure.Data;
using PBCRM2.Infrastructure.Identity;
using PBCRM2.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Get SQL Server connection string
var connectionString = builder.Configuration.GetConnectionString("MasterCRM2")
    ?? throw new InvalidOperationException("Connection string 'MasterCRM2' was not found.");

// Register Entity Framework Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("JWT key is not configured.");

    var jwtIssuer = builder.Configuration["Jwt:Issuer"]
        ?? throw new InvalidOperationException("JWT issuer is not configured.");

    var jwtAudience = builder.Configuration["Jwt:Audience"]
        ?? throw new InvalidOperationException("JWT audience is not configured.");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Register JWT token service
builder.Services.AddScoped<JwtTokenService>();

// Add controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add OpenAPI
builder.Services.AddOpenApi();

// Ensure or select an available port before building the app
EnsureOrSelectPort(builder);

var app = builder.Build();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed default roles and ensure default admin user exists
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    var roleManager = serviceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    await IdentitySeeder.SeedRolesAsync(roleManager);

    var userManager = serviceProvider
        .GetRequiredService<UserManager<ApplicationUser>>();

    var adminUser = await userManager.FindByNameAsync("admin");

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@example.com",
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(
            adminUser,
            "Admin123!!");

        if (createResult.Succeeded)
        {
            if (await roleManager.RoleExistsAsync("Admin"))
            {
                await userManager.AddToRoleAsync(
                    adminUser,
                    "Admin");
            }
        }
    }
    else
    {
        // Reset admin password to Admin123!!
        string token =
            await userManager.GeneratePasswordResetTokenAsync(
                adminUser);

        await userManager.ResetPasswordAsync(
            adminUser,
            token,
            "Admin123!!");
    }
}

try
{
    app.Run();
}
catch (System.IO.IOException ex)
    when (
        ex.Message.Contains(
            "address already in use",
            StringComparison.OrdinalIgnoreCase)
        ||
        (
            ex.InnerException != null &&
            ex.InnerException.GetType().Name ==
            "AddressInUseException"
        ))
{
    Console.Error.WriteLine(
        "Failed to start web host: address already in use. " +
        "Ensure no other process is listening on the configured HTTP ports " +
        "(ASPNETCORE_URLS) or change the port. Exception: " +
        ex.Message);

    Environment.Exit(1);
}

// Attempts to ensure the configured port is available
// or selects an alternative port.
static void EnsureOrSelectPort(
    WebApplicationBuilder builder)
{
    var envUrls =
        Environment.GetEnvironmentVariable("ASPNETCORE_URLS")
        ?? builder.Configuration["urls"]
        ?? string.Empty;

    if (string.IsNullOrEmpty(envUrls))
        return;

    var urls = envUrls.Split(
        new[] { ';', ',' },
        StringSplitOptions.RemoveEmptyEntries);

    var first = urls.Length > 0
        ? urls[0]
        : null;

    if (string.IsNullOrEmpty(first))
        return;

    if (!Uri.TryCreate(
        first,
        UriKind.Absolute,
        out var u))
        return;

    var host = u.Host;
    var preferredPort = u.Port > 0
        ? u.Port
        : 0;

    if (preferredPort == 0)
        return;

    if (!IsPortInUse(
        preferredPort,
        host))
    {
        builder.WebHost.UseUrls(
            $"{u.Scheme}://{host}:{preferredPort}");

        return;
    }

    const int start = 5000;
    const int end = 5100;

    int found = -1;

    for (int p = start; p <= end; p++)
    {
        if (!IsPortInUse(
            p,
            host))
        {
            found = p;
            break;
        }
    }

    if (found > 0)
    {
        Console.WriteLine(
            $"Port {preferredPort} was in use, " +
            $"selecting available port {found} instead.");

        builder.WebHost.UseUrls(
            $"{u.Scheme}://{host}:{found}");
    }
    else
    {
        Console.Error.WriteLine(
            $"Configured port {preferredPort} on {host} " +
            $"is in use and no free port found in range " +
            $"{start}-{end}. Using configured urls: {envUrls}");
    }
}

static bool IsPortInUse(
    int port,
    string host)
{
    try
    {
        var ip =
            System.Net.IPAddress.Parse(host);

        using var tcp =
            new System.Net.Sockets.TcpListener(
                ip,
                port);

        tcp.Start();
        tcp.Stop();

        return false;
    }
    catch (System.Net.Sockets.SocketException)
    {
        return true;
    }
    catch
    {
        return false;
    }
}