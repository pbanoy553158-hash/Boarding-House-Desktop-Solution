using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace PBCRM2.WinForms.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7241/")
        };
    }

    public async Task<LoginResponse?> LoginAsync(
        string username,
        string password)
    {
        try
        {
            var request = new
            {
                username,
                password
            };

            var json = JsonSerializer.Serialize(request);

            using var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                "api/Auth/login",
                content);

            var responseJson =
                await response.Content.ReadAsStringAsync();

            // API returned an error
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show(
                    $"Login failed.\n\n" +
                    $"Status: {(int)response.StatusCode} {response.StatusCode}\n\n" +
                    $"Server response:\n{responseJson}",
                    "Login Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return null;
            }

            // Successful response
            var result =
                JsonSerializer.Deserialize<LoginResponse>(
                    responseJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (result == null)
            {
                MessageBox.Show(
                    "The server returned an empty login response.",
                    "Login Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return null;
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show(
                $"Could not connect to the API.\n\n" +
                $"{ex.Message}",
                "Connection Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return null;
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

    public void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);
    }

    public async Task<string?> GetProtectedTestAsync()
    {
        var response = await _httpClient.GetAsync(
            "api/Test/protected");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }
}

public class LoginResponse
{
    public string Message { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int? BranchId { get; set; }

    public List<string> Roles { get; set; } = new();
    public object Role { get; internal set; }
}