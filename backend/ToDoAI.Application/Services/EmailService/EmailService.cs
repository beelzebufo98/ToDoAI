using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using ToDoAI.Application.Services.EmailService.Settings;

namespace ToDoAI.Application.Services.EmailService;

public sealed class EmailService : IEmailService
{
    private static readonly JsonSerializerOptions ResendJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly EmailSettings _settings;
    private readonly HttpClient _httpClient;

    public EmailService(IOptions<EmailSettings> settings, HttpClient httpClient)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
    }

    public Task SendEmailConfirmationAsync(string toEmail, string code, CancellationToken ct)
        => SendAsync(
            toEmail,
            subject: "Подтверждение email — ToDoAI",
            body: $"Ваш код подтверждения: <b>{code}</b><br/>Код действителен 15 минут.",
            ct);

    public Task SendPasswordResetAsync(string toEmail, string code, CancellationToken ct)
        => SendAsync(
            toEmail,
            subject: "Сброс пароля — ToDoAI",
            body: $"Ваш код для сброса пароля: <b>{code}</b><br/>Код действителен 15 минут.",
            ct);

    private Task SendAsync(string toEmail, string subject, string body, CancellationToken ct)
    {
        if (!_settings.Enabled)
        {
            throw new InvalidOperationException("Отправка email отключена в настройках.");
        }

        return _settings.Provider switch
        {
            EmailProvider.Smtp => SendViaSmtpAsync(toEmail, subject, body, ct),
            EmailProvider.Resend => SendViaResendAsync(toEmail, subject, body, ct),
            _ => throw new InvalidOperationException($"Неподдерживаемый email provider: {_settings.Provider}.")
        };
    }

    private async Task SendViaSmtpAsync(string toEmail, string subject, string body, CancellationToken ct)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();

        await client.ConnectAsync(
            _settings.Host,
            _settings.Port,
            MapSocketSecurityMode(_settings.SocketSecurityMode),
            ct);

        if (!string.IsNullOrWhiteSpace(_settings.UserName))
        {
            await client.AuthenticateAsync(_settings.UserName, _settings.Password, ct);
        }

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(quit: true, ct);
    }

    private async Task SendViaResendAsync(string toEmail, string subject, string body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
        {
            Content = JsonContent.Create(new ResendSendEmailRequest
            {
                From = FormatFromAddress(),
                To = [toEmail],
                Subject = subject,
                Html = BuildHtmlDocument(body)
            }, options: ResendJsonOptions)
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        using var response = await _httpClient.SendAsync(request, ct);
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync(ct);
        throw new InvalidOperationException($"Resend вернул ошибку {(int)response.StatusCode}: {responseBody}");
    }

    private string FormatFromAddress()
    {
        return string.IsNullOrWhiteSpace(_settings.FromName)
            ? _settings.FromAddress
            : $"{_settings.FromName} <{_settings.FromAddress}>";
    }

    private static string BuildHtmlDocument(string body)
    {
        return $"""
                <html lang="ru">
                  <body>
                    {body}
                  </body>
                </html>
                """;
    }

    private static SecureSocketOptions MapSocketSecurityMode(EmailSocketSecurityMode socketSecurityMode) =>
        socketSecurityMode switch
        {
            EmailSocketSecurityMode.None => SecureSocketOptions.None,
            EmailSocketSecurityMode.StartTls => SecureSocketOptions.StartTls,
            EmailSocketSecurityMode.SslOnConnect => SecureSocketOptions.SslOnConnect,
            _ => throw new ArgumentOutOfRangeException(nameof(socketSecurityMode), socketSecurityMode, null)
        };

    private sealed record ResendSendEmailRequest
    {
        public required string From { get; init; }

        public required IReadOnlyCollection<string> To { get; init; }

        public required string Subject { get; init; }

        public required string Html { get; init; }
    }
}
