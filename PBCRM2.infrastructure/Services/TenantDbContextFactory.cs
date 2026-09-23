using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PBCRM2.Infrastructure.Data;

namespace PBCRM2.Infrastructure.Services;

public class TenantDbContextFactory : ITenantDbContextFactory
{
    private readonly ITenantDatabaseResolver _resolver;
    private readonly IConfiguration _configuration;

    public TenantDbContextFactory(
        ITenantDatabaseResolver resolver,
        IConfiguration configuration)
    {
        _resolver = resolver;
        _configuration = configuration;
    }

    public async Task<TenantCRMDbContext> CreateAsync(
        int companyId)
    {
        // =====================================================
        // GET TENANT DATABASE INFORMATION
        // =====================================================

        var databaseInfo =
            await _resolver.GetDatabaseInfoAsync(companyId);

        if (string.IsNullOrWhiteSpace(
                databaseInfo.ServerName))
        {
            throw new InvalidOperationException(
                "Tenant database server is not configured.");
        }

        if (string.IsNullOrWhiteSpace(
                databaseInfo.DatabaseName))
        {
            throw new InvalidOperationException(
                "Tenant database name is not configured.");
        }

        if (string.IsNullOrWhiteSpace(
                databaseInfo.CredentialKey))
        {
            throw new InvalidOperationException(
                "Tenant database credential key is not configured.");
        }

        // =====================================================
        // GET CREDENTIAL CONFIGURATION
        // =====================================================

        var credentialSection =
            _configuration.GetSection(
                $"TenantCredentials:{databaseInfo.CredentialKey}");

        var userId =
            credentialSection["UserId"];

        var password =
            credentialSection["Password"];

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException(
                $"Tenant database UserId was not found for " +
                $"credential key '{databaseInfo.CredentialKey}'.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                $"Tenant database Password was not found for " +
                $"credential key '{databaseInfo.CredentialKey}'.");
        }

        // =====================================================
        // BUILD SQL CONNECTION STRING
        // =====================================================

        var connectionStringBuilder =
            new SqlConnectionStringBuilder
            {
                DataSource = databaseInfo.ServerName,
                InitialCatalog = databaseInfo.DatabaseName,
                UserID = userId,
                Password = password,

                Encrypt = true,
                TrustServerCertificate = true,

                ConnectTimeout = 30
            };

        // =====================================================
        // CREATE TENANT DB CONTEXT
        // =====================================================

        var optionsBuilder =
            new DbContextOptionsBuilder<TenantCRMDbContext>();

        optionsBuilder.UseSqlServer(
            connectionStringBuilder.ConnectionString,
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            });

        optionsBuilder.EnableDetailedErrors();

        return new TenantCRMDbContext(
            optionsBuilder.Options);
    }
}