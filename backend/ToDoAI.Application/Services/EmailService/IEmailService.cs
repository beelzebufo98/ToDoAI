namespace ToDoAI.Application.Services.EmailService;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string toEmail, string code, CancellationToken ct);
    Task SendPasswordResetAsync(string toEmail, string code, CancellationToken ct);
}