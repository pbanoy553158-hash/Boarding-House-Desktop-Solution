namespace PBCRM2.API.Settings;

public class FeedbackEmailSettings
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } =
        "Percy's Boarding House";

    public string FeedbackBaseUrl { get; set; } =
        string.Empty;
}