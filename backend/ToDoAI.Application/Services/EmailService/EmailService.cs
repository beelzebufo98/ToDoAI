using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using ToDoAI.Application.Services.EmailService.Settings;

namespace ToDoAI.Application.Services.EmailService;

public sealed class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
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

    private async Task SendAsync(string toEmail, string subject, string body, CancellationToken ct)
    {
        if (!_settings.Enabled)
        {
            throw new InvalidOperationException("Отправка email отключена в настройках.");
        }

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

    private static SecureSocketOptions MapSocketSecurityMode(EmailSocketSecurityMode socketSecurityMode) =>
        socketSecurityMode switch
        {
            EmailSocketSecurityMode.None => SecureSocketOptions.None,
            EmailSocketSecurityMode.StartTls => SecureSocketOptions.StartTls,
            EmailSocketSecurityMode.SslOnConnect => SecureSocketOptions.SslOnConnect,
            _ => throw new ArgumentOutOfRangeException(nameof(socketSecurityMode), socketSecurityMode, null)
        };
}
