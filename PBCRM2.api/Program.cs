using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PBCRM2.API.Services;
using PBCRM2.API.Settings;
using PBCRM2.Infrastructure.Data;
using PBCRM2.Infrastructure.Data.Seed;
using PBCRM2.Infrastructure.Identity;
using PBCRM2.Infrastructure.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// MASTER DATABASE CONNECTION
// ============================================================

var connectionString =
    builder.Configuration.GetConnectionString("MasterCRM2")
    ?? throw new InvalidOperationException(
        "Connection string 'MasterCRM2' was not found.");

// ============================================================
// MASTER DATABASE - APPLICATION DB CONTEXT
// ============================================================
//
// Master database:
// db67683
//
// Used for:
// - ASP.NET Core Identity
// - Users
// - Roles
// - Company information
// - Company database configuration
//
// ============================================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        });

    options.EnableDetailedErrors();
});

// ============================================================
// TENANT CRM DATABASE
// ============================================================
//
// Tenant CRM database:
// db68082
//
// Used for the actual Boarding House CRM:
//
// - Branches
// - Tenants
// - Rooms
// - Beds
// - Bed Assignment Requests
// - Billings
// - Payments
// - Maintenance Requests
// - Feedback Requests
// - Feedback
// - Renewals
//
// ============================================================

var tenantConnectionString =
    builder.Configuration.GetConnectionString("TenantCRM");

if (!string.IsNullOrWhiteSpace(tenantConnectionString))
{
    builder.Services.AddDbContext<TenantCRMDbContext>(options =>
    {
        options.UseSqlServer(
            tenantConnectionString,
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            });

        options.EnableDetailedErrors();
    });
}
else
{
    throw new InvalidOperationException(
        "Connection string 'TenantCRM' was not found.");
}

// ============================================================
// TENANT DATABASE RESOLVER AND FACTORY
// ============================================================
//
// These services allow the API to resolve the correct CRM
// database for a company.
//
// ============================================================

builder.Services.AddScoped<
    ITenantDatabaseResolver,
    TenantDatabaseResolver>();

builder.Services.AddScoped<
    ITenantDbContextFactory,
    TenantDbContextFactory>();

// ============================================================
// ASP.NET CORE IDENTITY
// ============================================================

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        // ====================================================
        // PASSWORD REQUIREMENTS
        // ====================================================

        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ============================================================
// JWT AUTHENTICATION
// ============================================================

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtKey =
            builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT key is not configured.");

        var jwtIssuer =
            builder.Configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer is not configured.");

        var jwtAudience =
            builder.Configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience is not configured.");

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
            };
    });

// ============================================================
// JWT TOKEN SERVICE
// ============================================================

builder.Services.AddScoped<JwtTokenService>();

// ============================================================
// FEEDBACK EMAIL SETTINGS
// ============================================================

builder.Services.Configure<FeedbackEmailSettings>(
    builder.Configuration.GetSection("FeedbackEmail"));

// ============================================================
// FEEDBACK EMAIL SERVICE
// ============================================================

builder.Services.AddScoped<FeedbackEmailService>();

// ============================================================
// QR CODE SERVICE
// ============================================================

builder.Services.AddScoped<QrCodeService>();

// ============================================================
// CORS
// ============================================================
//
// Allows the FeedbackWeb application running on port 7012
// to communicate with the PBCRM2 API.
//
// ============================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("FeedbackWeb", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7012")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ============================================================
// CONTROLLERS
// ============================================================

builder.Services.AddControllers();

// ============================================================
// SWAGGER
// ============================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.ParameterLocation.Header,
            Description = "Enter your JWT token."
        });

    options.AddSecurityRequirement(document =>
        new Microsoft.OpenApi.OpenApiSecurityRequirement
        {
            [
                new Microsoft.OpenApi.OpenApiSecuritySchemeReference(
                    "Bearer",
                    document)
            ] = []
        });
});

// ============================================================
// OPENAPI
// ============================================================

builder.Services.AddOpenApi();

// ============================================================
// PORT SELECTION
// ============================================================

EnsureOrSelectPort(builder);

var app = builder.Build();

// ============================================================
// HTTP REQUEST PIPELINE
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapOpenApi();
}

// ============================================================
// CORS
// ============================================================

app.UseCors("FeedbackWeb");

// ============================================================
// HTTPS
// ============================================================

app.UseHttpsRedirection();

// ============================================================
// AUTHENTICATION / AUTHORIZATION
// ============================================================
//
// Authentication must come before Authorization.
//
// ============================================================

app.UseAuthentication();
app.UseAuthorization();

// ============================================================
// CONTROLLERS
// ============================================================

app.MapControllers();

// ============================================================
// DATABASE SEEDING
// ============================================================
//
// DATABASE STRUCTURE:
//
// ============================================================
//
// MASTER DATABASE
// db67683
//     ApplicationDbContext
//          |
//          +-- ASP.NET Identity
//          +-- Users
//          +-- Roles
//          +-- Companies
//          +-- CompanyDatabases
//
// ============================================================
//
// TENANT CRM DATABASE
// db68082
//     TenantCRMDbContext
//          |
//          +-- Branches
//          +-- Tenants
//          +-- Rooms
//          +-- Beds
//          +-- BedAssignmentRequests
//          +-- Billings
//          +-- Payments
//          +-- MaintenanceRequests
//          +-- FeedbackRequests
//          +-- Feedbacks
//          +-- Renewals
//
// ============================================================
//
// IMPORTANT:
//
// TenantSeeder and RoomBedSeeder are NOT executed against
// ApplicationDbContext anymore.
//
// TenantCRMSeeder is executed against TenantCRMDbContext.
//
// ============================================================

using (var scope = app.Services.CreateScope())
{
    var serviceProvider =
        scope.ServiceProvider;

    // ========================================================
    // CONFIGURATION
    // ========================================================

    var configuration =
        serviceProvider
            .GetRequiredService<IConfiguration>();

    // ========================================================
    // IDENTITY MANAGERS
    // ========================================================

    var roleManager =
        serviceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

    var userManager =
        serviceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

    // ========================================================
    // IDENTITY SEEDING
    // ========================================================
    //
    // Identity belongs to the MasterCRM2 database.
    //
    // Database:
    // db67683
    //
    // Creates / verifies:
    //
    // - SuperAdmin
    // - Admin
    // - Manager
    // - Staff
    //
    // ========================================================

    Console.WriteLine();
    Console.WriteLine("=================================================");
    Console.WriteLine("RUNNING IDENTITY SEEDER...");
    Console.WriteLine("=================================================");

    await IdentitySeeder.SeedAsync(
        userManager,
        roleManager,
        configuration);

    Console.WriteLine("Identity seeding completed.");

    // ========================================================
    // MASTER DATABASE
    // ========================================================

    var masterDbContext =
        serviceProvider
            .GetRequiredService<ApplicationDbContext>();

    var masterConnection =
        masterDbContext.Database
            .GetDbConnection();

    // ========================================================
    // MASTER DATABASE DIAGNOSTICS
    // ========================================================

    Console.WriteLine();
    Console.WriteLine("=================================================");
    Console.WriteLine("PBCRM2 MASTER DATABASE CHECK");
    Console.WriteLine("=================================================");

    Console.WriteLine(
        $"Database: {masterConnection.Database}");

    Console.WriteLine(
        $"Server: {masterConnection.DataSource}");

    Console.WriteLine("=================================================");

    // ========================================================
    // TENANT CRM DATABASE
    // ========================================================

    var tenantCrmContext =
        serviceProvider
            .GetRequiredService<TenantCRMDbContext>();

    var tenantCrmConnection =
        tenantCrmContext.Database
            .GetDbConnection();

    // ========================================================
    // TENANT CRM DATABASE DIAGNOSTICS
    // ========================================================

    Console.WriteLine();
    Console.WriteLine("=================================================");
    Console.WriteLine("PBCRM2 TENANT CRM DATABASE CHECK");
    Console.WriteLine("=================================================");

    Console.WriteLine(
        $"Database: {tenantCrmConnection.Database}");

    Console.WriteLine(
        $"Server: {tenantCrmConnection.DataSource}");

    Console.WriteLine("=================================================");

    // ========================================================
    // CURRENT CRM COUNTS BEFORE SEEDING
    // ========================================================

    var beforeBranchCount =
        await tenantCrmContext.Branches
            .CountAsync();

    var beforeTenantCount =
        await tenantCrmContext.Tenants
            .CountAsync();

    var beforeRoomCount =
        await tenantCrmContext.Rooms
            .CountAsync();

    var beforeBedCount =
        await tenantCrmContext.Beds
            .CountAsync();

    var beforeOccupiedBedCount =
        await tenantCrmContext.Beds
            .CountAsync(b =>
                b.TenantId != null);

    var beforeAvailableBedCount =
        await tenantCrmContext.Beds
            .CountAsync(b =>
                b.TenantId == null);

    Console.WriteLine();
    Console.WriteLine("CURRENT TENANT CRM DATA");
    Console.WriteLine(
        $"Branches: {beforeBranchCount}");
    Console.WriteLine(
        $"Tenants: {beforeTenantCount}");
    Console.WriteLine(
        $"Rooms: {beforeRoomCount}");
    Console.WriteLine(
        $"Beds: {beforeBedCount}");
    Console.WriteLine(
        $"Occupied beds: {beforeOccupiedBedCount}");
    Console.WriteLine(
        $"Available beds: {beforeAvailableBedCount}");

    // ========================================================
    // RUN TENANT CRM SEEDER
    // ========================================================

    Console.WriteLine();
    Console.WriteLine("=================================================");
    Console.WriteLine("RUNNING TENANT CRM SEEDER...");
    Console.WriteLine("=================================================");

    await TenantCRMSeeder.SeedAsync(
        tenantCrmContext);

    // ========================================================
    // FINAL CRM COUNTS
    // ========================================================

    var finalBranchCount =
        await tenantCrmContext.Branches
            .CountAsync();

    var finalTenantCount =
        await tenantCrmContext.Tenants
            .CountAsync();

    var finalRoomCount =
        await tenantCrmContext.Rooms
            .CountAsync();

    var finalBedCount =
        await tenantCrmContext.Beds
            .CountAsync();

    var finalOccupiedBedCount =
        await tenantCrmContext.Beds
            .CountAsync(b =>
                b.TenantId != null);

    var finalAvailableBedCount =
        await tenantCrmContext.Beds
            .CountAsync(b =>
                b.TenantId == null);

    var finalActiveTenantCount =
        await tenantCrmContext.Tenants
            .CountAsync(t =>
                t.Status == "Active");

    var finalMovedOutTenantCount =
        await tenantCrmContext.Tenants
            .CountAsync(t =>
                t.Status == "Moved Out");

    var finalTenantsWithoutBeds =
        await tenantCrmContext.Tenants
            .CountAsync(t =>
                t.Status == "Active" &&
                t.Bed == null);

    // ========================================================
    // FINAL TENANT CRM SUMMARY
    // ========================================================

    Console.WriteLine();
    Console.WriteLine("=================================================");
    Console.WriteLine("FINAL TENANT CRM DATABASE SUMMARY");
    Console.WriteLine("=================================================");

    Console.WriteLine(
        $"Database: {tenantCrmConnection.Database}");

    Console.WriteLine(
        $"Server: {tenantCrmConnection.DataSource}");

    Console.WriteLine(
        $"Branches: {finalBranchCount}");

    Console.WriteLine(
        $"Tenants: {finalTenantCount}");

    Console.WriteLine(
        $"Active tenants: {finalActiveTenantCount}");

    Console.WriteLine(
        $"Moved Out tenants: {finalMovedOutTenantCount}");

    Console.WriteLine(
        $"Rooms: {finalRoomCount}");

    Console.WriteLine(
        $"Beds: {finalBedCount}");

    Console.WriteLine(
        $"Occupied beds: {finalOccupiedBedCount}");

    Console.WriteLine(
        $"Available beds: {finalAvailableBedCount}");

    Console.WriteLine(
        $"Active tenants without beds: {finalTenantsWithoutBeds}");

    Console.WriteLine("=================================================");

    // ========================================================
    // BASIC VALIDATION
    // ========================================================

    Console.WriteLine();

    if (finalTenantCount >= 200)
    {
        Console.WriteLine(
            "SUCCESS: Tenant CRM contains at least 200 tenants.");
    }
    else
    {
        Console.WriteLine(
            $"WARNING: Tenant CRM currently contains only " +
            $"{finalTenantCount} tenants.");
    }

    if (finalRoomCount >= 20)
    {
        Console.WriteLine(
            "SUCCESS: Tenant CRM contains at least 20 rooms.");
    }
    else
    {
        Console.WriteLine(
            $"WARNING: Tenant CRM currently contains only " +
            $"{finalRoomCount} rooms.");
    }

    if (finalBedCount > 0)
    {
        Console.WriteLine(
            "SUCCESS: Tenant CRM contains beds.");
    }
    else
    {
        Console.WriteLine(
            "WARNING: Tenant CRM contains no beds.");
    }

    Console.WriteLine();
    Console.WriteLine("=================================================");
    Console.WriteLine("DATABASE SEEDING COMPLETED");
    Console.WriteLine("=================================================");
    Console.WriteLine();
}

// ============================================================
// START API
// ============================================================

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

// ============================================================
// PORT HELPER
// ============================================================

static void EnsureOrSelectPort(
    WebApplicationBuilder builder)
{
    var envUrls =
        Environment.GetEnvironmentVariable(
            "ASPNETCORE_URLS")
        ?? builder.Configuration["urls"]
        ?? string.Empty;

    if (string.IsNullOrEmpty(envUrls))
        return;

    var urls =
        envUrls.Split(
            new[] { ';', ',' },
            StringSplitOptions.RemoveEmptyEntries);

    var first =
        urls.Length > 0
            ? urls[0]
            : null;

    if (string.IsNullOrEmpty(first))
        return;

    if (!Uri.TryCreate(
        first,
        UriKind.Absolute,
        out var u))
        return;

    var host =
        u.Host;

    var preferredPort =
        u.Port > 0
            ? u.Port
            : 0;

    if (preferredPort == 0)
        return;

    // ========================================================
    // USE PREFERRED PORT IF AVAILABLE
    // ========================================================

    if (!IsPortInUse(
        preferredPort,
        host))
    {
        builder.WebHost.UseUrls(
            $"{u.Scheme}://{host}:{preferredPort}");

        return;
    }

    // ========================================================
    // FALLBACK PORT RANGE
    // ========================================================

    const int start = 5000;
    const int end = 5100;

    int found = -1;

    for (
        int p = start;
        p <= end;
        p++)
    {
        if (!IsPortInUse(
            p,
            host))
        {
            found = p;
            break;
        }
    }

    // ========================================================
    // USE FOUND PORT
    // ========================================================

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

// ============================================================
// CHECK IF PORT IS IN USE
// ============================================================

static bool IsPortInUse(
    int port,
    string host)
{
    try
    {
        var ip =
            System.Net.IPAddress.Parse(
                host);

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