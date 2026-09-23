using System;
using System.Collections.Generic;
using System.Text;

namespace PBCRM2.Infrastructure.Services;

public class TenantDatabaseInfo
{
    public string ServerName { get; set; } =
        string.Empty;

    public string DatabaseName { get; set; } =
        string.Empty;

    public string CredentialKey { get; set; } =
        string.Empty;
}