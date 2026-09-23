using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace PBCRM2.WinForms.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private static string? _token;

        // =========================================================
        // CURRENT LOGIN SESSION
        // =========================================================

        public static string CurrentRole { get; private set; } =
            string.Empty;

        public static int? CurrentCompanyId { get; private set; }

        public static int? CurrentBranchId { get; private set; }

        public static string CurrentFullName { get; internal set; } =
            string.Empty;

        // =========================================================
        // LAST API ERROR
        // =========================================================

        public string LastErrorMessage { get; private set; } =
            string.Empty;

        public int? LastStatusCode { get; private set; }

        public bool LastRequestWasForbidden =>
            LastStatusCode == 403;

        public bool LastRequestWasUnauthorized =>
            LastStatusCode == 401;

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(
                    "https://localhost:7241/")
            };

            if (!string.IsNullOrWhiteSpace(_token))
            {
                _httpClient
                    .DefaultRequestHeaders
                    .Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        _token);
            }
        }

        // =========================================================
        // LOGIN
        // =========================================================

        public async Task<LoginResponse?> LoginAsync(
            string username,
            string password)
        {
            try
            {
                return await SendAsync<LoginResponse>(
                    HttpMethod.Post,
                    "api/Auth/login",
                    new
                    {
                        username,
                        password
                    },
                    showError: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An unexpected error occurred.\n\n" +
                    $"{ex.Message}",
                    "Login Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return null;
            }
        }

        // =========================================================
        // TOKEN
        // =========================================================

        public void SetToken(string token)
        {
            _token = token;

            _httpClient
                .DefaultRequestHeaders
                .Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);
        }

        // =========================================================
        // LOGIN SESSION
        // =========================================================

        public void SetLoginSession(
            LoginResponse loginResponse)
        {
            if (loginResponse == null)
            {
                return;
            }

            CurrentRole =
                loginResponse.Roles?
                    .FirstOrDefault() ??
                string.Empty;

            CurrentCompanyId =
                loginResponse.CompanyId;

            CurrentBranchId =
                loginResponse.BranchId;

            CurrentFullName =
                loginResponse.FullName ??
                string.Empty;
        }

        // =========================================================
        // CLEAR TOKEN / LOGOUT
        // =========================================================

        public void ClearToken()
        {
            _token = null;

            CurrentRole =
                string.Empty;

            CurrentCompanyId =
                null;

            CurrentBranchId =
                null;

            CurrentFullName =
                string.Empty;

            _httpClient
                .DefaultRequestHeaders
                .Authorization = null;
        }

        // =========================================================
        // GET
        // =========================================================

        public async Task<T?> GetAsync<T>(
            string endpoint)
        {
            return await SendAsync<T>(
                HttpMethod.Get,
                endpoint,
                null,
                showError: true);
        }

        // =========================================================
        // GET SILENT
        // =========================================================

        public async Task<T?> GetSilentAsync<T>(
            string endpoint)
        {
            return await SendAsync<T>(
                HttpMethod.Get,
                endpoint,
                null,
                showError: false);
        }

        // =========================================================
        // GET WITH ERROR CONTROL
        // =========================================================

        public async Task<T?> GetAsync<T>(
            string endpoint,
            bool showError)
        {
            return await SendAsync<T>(
                HttpMethod.Get,
                endpoint,
                null,
                showError);
        }

        // =========================================================
        // COMPANIES
        // =========================================================

        public async Task<List<CompanyDto>?> GetCompaniesAsync()
        {
            return await GetAsync<List<CompanyDto>>(
                "api/Users/companies");
        }

        // =========================================================
        // BRANCHES BY COMPANY
        // =========================================================

        public async Task<List<BranchDto>?> GetBranchesAsync(
            int companyId)
        {
            return await GetAsync<List<BranchDto>>(
                $"api/Users/companies/{companyId}/branches");
        }

        // =========================================================
        // ACTIVE BRANCHES
        // =========================================================

        public async Task<List<AdminBranchDto>?> GetAdminBranchesAsync()
        {
            return await GetAsync<List<AdminBranchDto>>(
                "api/Branches");
        }

        // =========================================================
        // ALL BRANCHES INCLUDING INACTIVE
        // =========================================================

        public async Task<List<AdminBranchDto>?> GetAllAdminBranchesAsync()
        {
            return await GetAsync<List<AdminBranchDto>>(
                "api/Branches?includeInactive=true");
        }

        // =========================================================
        // SINGLE BRANCH
        // =========================================================

        public async Task<AdminBranchDto?> GetAdminBranchAsync(
            int branchId)
        {
            return await GetAsync<AdminBranchDto>(
                $"api/Branches/{branchId}");
        }

        // =========================================================
        // GET BRANCH MANAGERS
        // =========================================================

        public async Task<List<ManagerDto>?> GetBranchManagersAsync()
        {
            return await GetAsync<List<ManagerDto>>(
                "api/Branches/managers");
        }

        // =========================================================
        // CREATE BRANCH
        // =========================================================

        public async Task<BranchCreateResponse?> CreateBranchAsync(
            BranchSaveRequest request)
        {
            return await PostAsync<BranchCreateResponse>(
                "api/Branches",
                request);
        }

        // =========================================================
        // UPDATE BRANCH
        // =========================================================

        public async Task<BranchUpdateResponse?> UpdateBranchAsync(
            int branchId,
            BranchSaveRequest request)
        {
            return await PutAsync<BranchUpdateResponse>(
                $"api/Branches/{branchId}",
                request);
        }

        // =========================================================
        // DEACTIVATE BRANCH
        // =========================================================

        public async Task<bool> DeactivateBranchAsync(
            int branchId)
        {
            return await DeleteAsync(
                $"api/Branches/{branchId}");
        }

        // =========================================================
        // ACTIVATE BRANCH
        // =========================================================

        public async Task<BranchStatusResponse?> ActivateBranchAsync(
            int branchId)
        {
            return await PutAsync<BranchStatusResponse>(
                $"api/Branches/{branchId}/activate",
                null);
        }

        // =========================================================
        // RENEWAL & RETENTION
        // =========================================================

        public async Task<List<RenewalDto>?> GetRenewalsAsync()
        {
            return await GetAsync<List<RenewalDto>>(
                "api/Renewals");
        }

        public async Task<List<RenewalDto>?> GetRenewalsSilentAsync()
        {
            return await GetSilentAsync<List<RenewalDto>>(
                "api/Renewals");
        }

        public async Task<RenewalDto?> GetRenewalAsync(
            int renewalId)
        {
            return await GetAsync<RenewalDto>(
                $"api/Renewals/{renewalId}");
        }

        public async Task<List<RenewalDto>?> GetTenantRenewalsAsync(
            int tenantId)
        {
            return await GetAsync<List<RenewalDto>>(
                $"api/Renewals/Tenant/{tenantId}");
        }

        public async Task<List<RenewalDto>?> GetTenantRenewalsSilentAsync(
            int tenantId)
        {
            return await GetSilentAsync<List<RenewalDto>>(
                $"api/Renewals/Tenant/{tenantId}");
        }

        public async Task<RenewalDto?> CreateRenewalAsync(
            RenewalRequest request)
        {
            return await PostAsync<RenewalDto>(
                "api/Renewals",
                request);
        }

        public async Task<RenewalDto?> UpdateRenewalAsync(
            int renewalId,
            RenewalRequest request)
        {
            return await PutAsync<RenewalDto>(
                $"api/Renewals/{renewalId}",
                request);
        }

        public async Task<bool> DeleteRenewalAsync(
            int renewalId)
        {
            return await DeleteAsync(
                $"api/Renewals/{renewalId}");
        }

        public static readonly string[] RenewalStatuses =
        {
            "Pending",
            "Approved",
            "Declined",
            "Completed"
        };

        // =========================================================
        // TENANT REPORT
        // =========================================================

        public async Task<TenantReportResponse?> GetTenantReportAsync()
        {
            return await GetAsync<TenantReportResponse>(
                "api/Reports/tenants");
        }

        // =========================================================
        // POST
        // =========================================================

        public async Task<T?> PostAsync<T>(
            string endpoint,
            object? body)
        {
            return await SendAsync<T>(
                HttpMethod.Post,
                endpoint,
                body,
                showError: true);
        }

        public async Task<T?> PostAsync<T>(
            string endpoint,
            object? body,
            bool showError)
        {
            return await SendAsync<T>(
                HttpMethod.Post,
                endpoint,
                body,
                showError);
        }

        // =========================================================
        // POST BOOLEAN
        // =========================================================

        public async Task<bool> PostAsync(
            string endpoint,
            object? body)
        {
            var result =
                await SendAsync<ApiMessage>(
                    HttpMethod.Post,
                    endpoint,
                    body,
                    showError: true);

            return result != null;
        }

        public async Task<bool> PostAsync(
            string endpoint,
            object? body,
            bool showError)
        {
            var result =
                await SendAsync<ApiMessage>(
                    HttpMethod.Post,
                    endpoint,
                    body,
                    showError);

            return result != null;
        }

        // =========================================================
        // PUT
        // =========================================================

        public async Task<T?> PutAsync<T>(
            string endpoint,
            object? body)
        {
            return await SendAsync<T>(
                HttpMethod.Put,
                endpoint,
                body,
                showError: true);
        }

        public async Task<T?> PutAsync<T>(
            string endpoint,
            object? body,
            bool showError)
        {
            return await SendAsync<T>(
                HttpMethod.Put,
                endpoint,
                body,
                showError);
        }

        // =========================================================
        // PUT BOOLEAN
        // =========================================================

        public async Task<bool> PutAsync(
            string endpoint,
            object? body)
        {
            var result =
                await SendAsync<ApiMessage>(
                    HttpMethod.Put,
                    endpoint,
                    body,
                    showError: true);

            return result != null;
        }

        public async Task<bool> PutAsync(
            string endpoint,
            object? body,
            bool showError)
        {
            var result =
                await SendAsync<ApiMessage>(
                    HttpMethod.Put,
                    endpoint,
                    body,
                    showError);

            return result != null;
        }

        // =========================================================
        // DELETE
        // =========================================================

        public async Task<bool> DeleteAsync(
            string endpoint)
        {
            LastErrorMessage =
                string.Empty;

            LastStatusCode =
                null;

            try
            {
                using var request =
                    new HttpRequestMessage(
                        HttpMethod.Delete,
                        endpoint);

                using var response =
                    await _httpClient.SendAsync(
                        request);

                LastStatusCode =
                    (int)response.StatusCode;

                var responseJson =
                    await response.Content
                        .ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                LastErrorMessage =
                    ExtractApiErrorMessage(
                        responseJson);

                if (string.IsNullOrWhiteSpace(
                    LastErrorMessage))
                {
                    LastErrorMessage =
                        $"Request failed with status " +
                        $"{(int)response.StatusCode} " +
                        $"{response.StatusCode}.";
                }

                if (response.StatusCode ==
                    System.Net.HttpStatusCode.Unauthorized)
                {
                    ClearToken();
                }

                MessageBox.Show(
                    $"Request failed.\n\n" +
                    $"Status: " +
                    $"{(int)response.StatusCode} " +
                    $"{response.StatusCode}\n\n" +
                    $"Message:\n" +
                    $"{LastErrorMessage}",
                    "PBCRM2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
            catch (HttpRequestException ex)
            {
                LastErrorMessage =
                    ex.Message;

                MessageBox.Show(
                    $"Could not connect to the API.\n\n" +
                    $"{ex.Message}",
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
            catch (TaskCanceledException ex)
            {
                LastErrorMessage =
                    ex.Message;

                MessageBox.Show(
                    $"The API request timed out " +
                    $"or was cancelled.\n\n" +
                    $"{ex.Message}",
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return false;
            }
            catch (Exception ex)
            {
                LastErrorMessage =
                    ex.Message;

                MessageBox.Show(
                    $"Request error.\n\n" +
                    $"{ex.Message}",
                    "PBCRM2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
        }

        // =========================================================
        // MAIN HTTP REQUEST METHOD
        // =========================================================

        private async Task<T?> SendAsync<T>(
            HttpMethod method,
            string endpoint,
            object? body,
            bool showError = true)
        {
            LastErrorMessage =
                string.Empty;

            LastStatusCode =
                null;

            try
            {
                using var request =
                    new HttpRequestMessage(
                        method,
                        endpoint);

                // -------------------------------------------------
                // REQUEST BODY
                // -------------------------------------------------

                if (body != null)
                {
                    var json =
                        JsonSerializer.Serialize(
                            body,
                            new JsonSerializerOptions
                            {
                                PropertyNamingPolicy =
                                    JsonNamingPolicy.CamelCase
                            });

                    request.Content =
                        new StringContent(
                            json,
                            Encoding.UTF8,
                            "application/json");
                }

                // -------------------------------------------------
                // SEND REQUEST
                // -------------------------------------------------

                using var response =
                    await _httpClient.SendAsync(
                        request);

                LastStatusCode =
                    (int)response.StatusCode;

                var responseJson =
                    await response.Content
                        .ReadAsStringAsync();

                // -------------------------------------------------
                // API ERROR
                // -------------------------------------------------

                if (!response.IsSuccessStatusCode)
                {
                    LastErrorMessage =
                        ExtractApiErrorMessage(
                            responseJson);

                    if (string.IsNullOrWhiteSpace(
                        LastErrorMessage))
                    {
                        LastErrorMessage =
                            $"Request failed with status " +
                            $"{(int)response.StatusCode} " +
                            $"{response.StatusCode}.";
                    }

                    if (response.StatusCode ==
                        System.Net.HttpStatusCode.Unauthorized)
                    {
                        ClearToken();
                    }

                    if (showError)
                    {
                        MessageBox.Show(
                            $"Request failed.\n\n" +
                            $"Status: " +
                            $"{(int)response.StatusCode} " +
                            $"{response.StatusCode}\n\n" +
                            $"Message:\n" +
                            $"{LastErrorMessage}",
                            "PBCRM2",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }

                    return default;
                }

                // -------------------------------------------------
                // STRING RESPONSE
                // -------------------------------------------------

                if (typeof(T) == typeof(string))
                {
                    return (T)(object)responseJson;
                }

                // -------------------------------------------------
                // EMPTY RESPONSE
                // -------------------------------------------------

                if (string.IsNullOrWhiteSpace(
                    responseJson))
                {
                    return default;
                }

                // -------------------------------------------------
                // JSON RESPONSE
                // -------------------------------------------------

                return JsonSerializer.Deserialize<T>(
                    responseJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive =
                            true
                    });
            }
            catch (HttpRequestException ex)
            {
                LastErrorMessage =
                    ex.Message;

                if (showError)
                {
                    MessageBox.Show(
                        $"Could not connect to the API.\n\n" +
                        $"{ex.Message}",
                        "Connection Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }

                return default;
            }
            catch (TaskCanceledException ex)
            {
                LastErrorMessage =
                    ex.Message;

                if (showError)
                {
                    MessageBox.Show(
                        $"The API request timed out " +
                        $"or was cancelled.\n\n" +
                        $"{ex.Message}",
                        "Connection Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                return default;
            }
            catch (JsonException ex)
            {
                LastErrorMessage =
                    "The API returned an invalid JSON response.\n\n" +
                    ex.Message;

                if (showError)
                {
                    MessageBox.Show(
                        LastErrorMessage,
                        "Response Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }

                return default;
            }
            catch (Exception ex)
            {
                LastErrorMessage =
                    ex.Message;

                if (showError)
                {
                    MessageBox.Show(
                        $"Request error.\n\n" +
                        $"{ex.Message}",
                        "PBCRM2",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }

                return default;
            }
        }

        // =========================================================
        // EXTRACT API ERROR MESSAGE
        // =========================================================

        private static string ExtractApiErrorMessage(
            string responseJson)
        {
            if (string.IsNullOrWhiteSpace(
                responseJson))
            {
                return string.Empty;
            }

            try
            {
                using var document =
                    JsonDocument.Parse(
                        responseJson);

                var root =
                    document.RootElement;

                // -------------------------------------------------
                // message
                // -------------------------------------------------

                if (root.TryGetProperty(
                    "message",
                    out var messageProperty))
                {
                    if (messageProperty.ValueKind ==
                        JsonValueKind.String)
                    {
                        var message =
                            messageProperty.GetString();

                        if (!string.IsNullOrWhiteSpace(
                            message))
                        {
                            return message;
                        }
                    }
                }

                // -------------------------------------------------
                // error
                // -------------------------------------------------

                if (root.TryGetProperty(
                    "error",
                    out var errorProperty))
                {
                    if (errorProperty.ValueKind ==
                        JsonValueKind.String)
                    {
                        var error =
                            errorProperty.GetString();

                        if (!string.IsNullOrWhiteSpace(
                            error))
                        {
                            return error;
                        }
                    }
                }

                // -------------------------------------------------
                // title
                // -------------------------------------------------

                if (root.TryGetProperty(
                    "title",
                    out var titleProperty))
                {
                    if (titleProperty.ValueKind ==
                        JsonValueKind.String)
                    {
                        var title =
                            titleProperty.GetString();

                        if (!string.IsNullOrWhiteSpace(
                            title))
                        {
                            return title;
                        }
                    }
                }

                // -------------------------------------------------
                // detail
                // -------------------------------------------------

                if (root.TryGetProperty(
                    "detail",
                    out var detailProperty))
                {
                    if (detailProperty.ValueKind ==
                        JsonValueKind.String)
                    {
                        var detail =
                            detailProperty.GetString();

                        if (!string.IsNullOrWhiteSpace(
                            detail))
                        {
                            return detail;
                        }
                    }
                }

                // -------------------------------------------------
                // ASP.NET VALIDATION ERRORS
                // -------------------------------------------------

                if (root.TryGetProperty(
                    "errors",
                    out var errorsProperty))
                {
                    if (errorsProperty.ValueKind ==
                        JsonValueKind.Object)
                    {
                        var messages =
                            new List<string>();

                        foreach (var property in
                                 errorsProperty.EnumerateObject())
                        {
                            if (property.Value.ValueKind ==
                                JsonValueKind.Array)
                            {
                                foreach (var item in
                                         property.Value.EnumerateArray())
                                {
                                    if (item.ValueKind ==
                                        JsonValueKind.String)
                                    {
                                        var validationMessage =
                                            item.GetString();

                                        if (!string.IsNullOrWhiteSpace(
                                            validationMessage))
                                        {
                                            messages.Add(
                                                validationMessage);
                                        }
                                    }
                                }
                            }
                        }

                        if (messages.Count > 0)
                        {
                            return string.Join(
                                Environment.NewLine,
                                messages);
                        }
                    }
                }
            }
            catch
            {
                // Response was not valid JSON.
            }

            return responseJson.Trim();
        }

        // =========================================================
        // PROTECTED API TEST
        // =========================================================

        public async Task<string?> GetProtectedTestAsync()
        {
            try
            {
                using var response =
                    await _httpClient.GetAsync(
                        "api/Test/protected");

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content
                    .ReadAsStringAsync();
            }
            catch
            {
                return null;
            }
        }
    }

    // =============================================================
    // LOGIN RESPONSE
    // =============================================================

    public class LoginResponse
    {
        public string Message { get; set; } =
            string.Empty;

        public string Token { get; set; } =
            string.Empty;

        public string UserId { get; set; } =
            string.Empty;

        public string Username { get; set; } =
            string.Empty;

        public string FullName { get; set; } =
            string.Empty;

        public string Email { get; set; } =
            string.Empty;

        public int? CompanyId { get; set; }

        public int? BranchId { get; set; }

        public List<string> Roles { get; set; } =
            new();
    }

    // =============================================================
    // GENERAL API MESSAGE
    // =============================================================

    public class ApiMessage
    {
        public string Message { get; set; } =
            string.Empty;
    }

    // =============================================================
    // COMPANY DTO
    // =============================================================

    public class CompanyDto
    {
        public int Id { get; set; }

        public string CompanyName { get; set; } =
            string.Empty;

        public override string ToString()
        {
            return CompanyName;
        }
    }

    // =============================================================
    // GENERAL BRANCH DTO
    // =============================================================

    public class BranchDto
    {
        public int Id { get; set; }

        public string BranchName { get; set; } =
            string.Empty;

        public override string ToString()
        {
            return BranchName;
        }
    }

    // =============================================================
    // ADMIN BRANCH DTO
    // =============================================================

    public class AdminBranchDto
    {
        public int Id { get; set; }

        public string BranchName { get; set; } =
            string.Empty;

        public string Address { get; set; } =
            string.Empty;

        public string ContactNumber { get; set; } =
            string.Empty;

        public bool IsActive { get; set; }

        // =========================================================
        // MANAGER ASSIGNMENT
        // =========================================================

        public string? ManagerId { get; set; }

        public string ManagerName { get; set; } =
            "Unassigned";

        // =========================================================
        // STATUS
        // =========================================================

        public string Status =>
            IsActive
                ? "Active"
                : "Inactive";

        public override string ToString()
        {
            return BranchName;
        }
    }

    // =============================================================
    // MANAGER DTO
    // =============================================================

    public class ManagerDto
    {
        public string Id { get; set; } =
            string.Empty;

        public string FullName { get; set; } =
            string.Empty;

        public string Email { get; set; } =
            string.Empty;

        public int? BranchId { get; set; }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(
                    FullName))
                {
                    return FullName;
                }

                if (!string.IsNullOrWhiteSpace(
                    Email))
                {
                    return Email;
                }

                return "Unnamed Manager";
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }

    // =============================================================
    // BRANCH SAVE REQUEST
    // =============================================================

    public class BranchSaveRequest
    {
        // =========================================================
        // BRANCH INFORMATION
        // =========================================================

        public string BranchName { get; set; } =
            string.Empty;

        public string Address { get; set; } =
            string.Empty;

        public string ContactNumber { get; set; } =
            string.Empty;

        // =========================================================
        // EXISTING MANAGER
        // =========================================================

        public string? ManagerId { get; set; }

        // =========================================================
        // NEW MANAGER MODE
        // =========================================================

        public bool CreateNewManager { get; set; }

        // =========================================================
        // NEW MANAGER INFORMATION
        // =========================================================

        public string? ManagerFullName { get; set; }

        public string? ManagerUsername { get; set; }

        public string? ManagerEmail { get; set; }

        public string? ManagerPassword { get; set; }

        public string? ManagerConfirmPassword { get; set; }
    }

    // =============================================================
    // BRANCH CREATE RESPONSE
    // =============================================================

    public class BranchCreateResponse
    {
        public string Message { get; set; } =
            string.Empty;

        public int Id { get; set; }

        public string BranchName { get; set; } =
            string.Empty;

        public string Address { get; set; } =
            string.Empty;

        public string ContactNumber { get; set; } =
            string.Empty;

        public bool IsActive { get; set; }

        public string? ManagerId { get; set; }

        public string ManagerName { get; set; } =
            "Unassigned";
    }

    // =============================================================
    // BRANCH UPDATE RESPONSE
    // =============================================================

    public class BranchUpdateResponse
    {
        public string Message { get; set; } =
            string.Empty;

        public int Id { get; set; }

        public string BranchName { get; set; } =
            string.Empty;

        public string Address { get; set; } =
            string.Empty;

        public string ContactNumber { get; set; } =
            string.Empty;

        public bool IsActive { get; set; }

        public string? ManagerId { get; set; }

        public string ManagerName { get; set; } =
            "Unassigned";
    }

    // =============================================================
    // BRANCH STATUS RESPONSE
    // =============================================================

    public class BranchStatusResponse
    {
        public string Message { get; set; } =
            string.Empty;

        public int Id { get; set; }

        public string BranchName { get; set; } =
            string.Empty;

        public bool IsActive { get; set; }
    }

    // =============================================================
    // RENEWAL DTO
    // =============================================================

    public class RenewalDto
    {
        public int Id { get; set; }

        public int TenantId { get; set; }

        public string TenantName { get; set; } =
            string.Empty;

        public string TenantEmail { get; set; } =
            string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string RenewalStatus { get; set; } =
            "Pending";

        public DateTime? RenewalDate { get; set; }

        public string? Notes { get; set; }

        public string Status =>
            string.IsNullOrWhiteSpace(
                RenewalStatus)
                ? "Pending"
                : RenewalStatus;

        public bool IsPending =>
            string.Equals(
                RenewalStatus,
                "Pending",
                StringComparison.OrdinalIgnoreCase);

        public bool IsApproved =>
            string.Equals(
                RenewalStatus,
                "Approved",
                StringComparison.OrdinalIgnoreCase);

        public bool IsDeclined =>
            string.Equals(
                RenewalStatus,
                "Declined",
                StringComparison.OrdinalIgnoreCase);

        public bool IsCompleted =>
            string.Equals(
                RenewalStatus,
                "Completed",
                StringComparison.OrdinalIgnoreCase);

        public bool IsExpired =>
            EndDate.Date < DateTime.Today;

        public int DaysUntilExpiration =>
            (EndDate.Date - DateTime.Today).Days;

        public bool IsExpiringSoon =>
            !IsExpired &&
            DaysUntilExpiration <= 30;

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(
                TenantName)
                ? $"Renewal #{Id}"
                : TenantName;
        }
    }

    // =============================================================
    // RENEWAL REQUEST
    // =============================================================

    public class RenewalRequest
    {
        public int TenantId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string RenewalStatus { get; set; } =
            "Pending";

        public DateTime? RenewalDate { get; set; }

        public string? Notes { get; set; }
    }

    // =============================================================
    // TENANT REPORT RESPONSE
    // =============================================================

    public class TenantReportResponse
    {
        public TenantReportSummary Summary { get; set; } =
            new();

        public List<TenantReportItem> Data { get; set; } =
            new();
    }

    // =============================================================
    // TENANT REPORT SUMMARY
    // =============================================================

    public class TenantReportSummary
    {
        public int Total { get; set; }

        public int Active { get; set; }

        public int Inactive { get; set; }

        public int MovedOut { get; set; }
    }

    // =============================================================
    // TENANT REPORT ITEM
    // =============================================================

    public class TenantReportItem
    {
        public int Id { get; set; }

        public string FullName { get; set; } =
            string.Empty;

        public string Sex { get; set; } =
            string.Empty;

        public string ContactNumber { get; set; } =
            string.Empty;

        public string Email { get; set; } =
            string.Empty;

        public string BranchName { get; set; } =
            string.Empty;

        public DateTime MoveInDate { get; set; }

        public DateTime? ActualMoveOutDate { get; set; }

        public string Status { get; set; } =
            string.Empty;
    }
}