using System.Net;
using System.Net.Http.Json;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NKSilk.Application.Common.Interfaces;

namespace NKSilk.Infrastructure.Notifications;

/// <summary>
/// Production-ready notification transport. Sends real email over SMTP when an
/// <c>Email:SmtpHost</c> is configured, and SMS via an HTTP gateway when <c>Sms:ApiUrl</c>
/// is configured; otherwise it logs the payload (dev simulator). A failure on either
/// channel is swallowed so it never breaks the calling use case.
/// </summary>
public class ConfigurableNotificationSender : INotificationSender
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<ConfigurableNotificationSender> _log;

    public ConfigurableNotificationSender(IConfiguration config, IHttpClientFactory httpFactory,
        ILogger<ConfigurableNotificationSender> log)
    {
        _config = config;
        _httpFactory = httpFactory;
        _log = log;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        var host = _config["Email:SmtpHost"];
        if (string.IsNullOrWhiteSpace(host))
        {
            _log.LogInformation("[EMAIL → {To}] {Subject} — {Body}", toEmail, subject, body);
            return;
        }

        try
        {
            using var msg = new MailMessage(_config["Email:From"] ?? "no-reply@nksilk.local", toEmail, subject, body);
            using var client = new SmtpClient(host, int.TryParse(_config["Email:SmtpPort"], out var p) ? p : 587)
            {
                EnableSsl = !bool.TryParse(_config["Email:EnableSsl"], out var ssl) || ssl
            };
            var user = _config["Email:User"];
            if (!string.IsNullOrWhiteSpace(user))
                client.Credentials = new NetworkCredential(user, _config["Email:Password"]);
            await client.SendMailAsync(msg, ct);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "SMTP send to {To} failed", toEmail);
        }
    }

    public async Task SendSmsAsync(string? toPhone, string message, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(toPhone)) return;

        var apiUrl = _config["Sms:ApiUrl"];
        if (string.IsNullOrWhiteSpace(apiUrl))
        {
            _log.LogInformation("[SMS → {To}] {Message}", toPhone, message);
            return;
        }

        try
        {
            var client = _httpFactory.CreateClient("sms");
            var payload = new
            {
                apiKey = _config["Sms:ApiKey"],
                sender = _config["Sms:Sender"],
                to = toPhone,
                message
            };
            await client.PostAsJsonAsync(apiUrl, payload, ct);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "SMS send to {To} failed", toPhone);
        }
    }
}
