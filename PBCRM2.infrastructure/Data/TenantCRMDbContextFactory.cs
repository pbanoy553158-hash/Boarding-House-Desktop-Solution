using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace PBCRM2.Infrastructure.Data;

public class TenantCRMDbContextFactory
    : IDesignTimeDbContextFactory<TenantCRMDbContext>
{
    public TenantCRMDbContext CreateDbContext(string[] args)
    {
        // =====================================================
        // LOAD PBCRM2.Api/appsettings.json
        // =====================================================

        var basePath = Directory.GetCurrentDirectory();

        var apiPath = Path.Combine(
            basePath,
            "..",
            "PBCRM2.Api");

        if (!File.Exists(
                Path.Combine(apiPath, "appsettings.json")))
        {
            apiPath = Path.Combine(
                basePath,
                "PBCRM2.Api");
        }

        var configuration =
            new ConfigurationBuilder()
                .SetBasePath(apiPath)
                .AddJsonFile(
                    "appsettings.json",
                    optional: false,
                    reloadOnChange: false)
                .Build();

        // =====================================================
        // TENANT DATABASE SETTINGS
        // =====================================================

        const string credentialKey = "PercyCRM";

        var userId =
            configuration[
                $"TenantCredentials:{credentialKey}:UserId"];

        var password =
            configuration[
                $"TenantCredentials:{credentialKey}:Password"];

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException(
                "Tenant database UserId was not found in " +
                "TenantCredentials:PercyCRM.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Tenant database Password was not found in " +
                "TenantCredentials:PercyCRM.");
        }

        // =====================================================
        // BUILD TENANT CONNECTION
        // =====================================================

        var connectionBuilder =
            new SqlConnectionStringBuilder
            {
                DataSource =
                    "db68082.public.databaseasp.net,1433",

                InitialCatalog =
                    "db68082",

                UserID =
                    userId,

                Password =
                    password,

                Encrypt = true,

                TrustServerCertificate = true,

                MultipleActiveResultSets = true,

                ConnectTimeout = 60
            };

        // =====================================================
        // CREATE TENANT CONTEXT
        // =====================================================

        var optionsBuilder =
            new DbContextOptionsBuilder<TenantCRMDbContext>();

        optionsBuilder.UseSqlServer(
            connectionBuilder.ConnectionString,
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            });

        return new TenantCRMDbContext(
            optionsBuilder.Options);
    }
}