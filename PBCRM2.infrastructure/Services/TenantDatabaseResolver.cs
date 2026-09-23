using Microsoft.EntityFrameworkCore;
using PBCRM2.Infrastructure.Data;

namespace PBCRM2.Infrastructure.Services;

public class TenantDatabaseResolver : ITenantDatabaseResolver
{
    private readonly ApplicationDbContext _masterDb;

    public TenantDatabaseResolver(
        ApplicationDbContext masterDb)
    {
        _masterDb = masterDb;
    }

    public async Task<TenantDatabaseInfo> GetDatabaseInfoAsync(
        int companyId)
    {
        var database = await _masterDb.CompanyDatabases
            .AsNoTracking()
            .FirstOrDefaultAsync(cd =>
                cd.CompanyId == companyId &&
                cd.IsActive);

        if (database == null)
        {
            throw new InvalidOperationException(
                $"No active tenant database was found for CompanyId {companyId}.");
        }

        return new TenantDatabaseInfo
        {
            ServerName = database.ServerName,
            DatabaseName = database.DatabaseName,
            CredentialKey = database.CredentialKey
        };
    }
}