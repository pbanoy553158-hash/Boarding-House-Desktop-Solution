using System;
using System.Collections.Generic;
using System.Text;

namespace PBCRM2.Infrastructure.Services;

public interface ITenantDatabaseResolver
{
    Task<TenantDatabaseInfo> GetDatabaseInfoAsync(
        int companyId);
}