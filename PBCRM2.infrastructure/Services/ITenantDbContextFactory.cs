using System;
using System.Collections.Generic;
using System.Text;

using PBCRM2.Infrastructure.Data;

namespace PBCRM2.Infrastructure.Services;

public interface ITenantDbContextFactory
{
    Task<TenantCRMDbContext> CreateAsync(int companyId);
}