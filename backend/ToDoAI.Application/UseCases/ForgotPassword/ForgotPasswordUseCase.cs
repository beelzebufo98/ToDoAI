using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using ToDoAI.Application.Abstractions.DalProviders.PasswordResetDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.PasswordResetDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Services.EmailService;

namespace ToDoAI.Application.UseCases.ForgotPassword;

public sealed class ForgotPasswordUseCase : IForgotPasswordUseCase
{
    private static readonly TimeSpan PasswordResetLifetime = TimeSpan.FromMinutes(15);

    private readonly IUserDalProvider _userDalProvider;
    private readonly IPasswordResetDalProvider _passwordResetDalProvider;
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordUseCase> _logger;

    public ForgotPasswordUseCase(
        IUserDalProvider userDalProvider,
        IPasswordResetDalProvider passwordResetDalProvider,
        IEmailService emailService,
        ILogger<ForgotPasswordUseCase> logger)
    {
        _userDalProvider = userDalProvider;
        _passwordResetDalProvider = passwordResetDalProvider;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task ForgotPassword(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim();
        var user = await _userDalProvider.GetUserByEmail(normalizedEmail, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            _logger.LogInformation("Forgot password requested for unknown email {Email}.", normalizedEmail);
            return;
        }

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var now = DateTimeOffset.UtcNow;

        await _passwordResetDalProvider.ReplacePasswordReset(new PasswordResetRequestDal
        {
            Id = Guid.NewGuid(),
            UserId = user.UserId,
            CodeHash = HashCode(code),
            ExpiresAt = now.Add(PasswordResetLifetime),
            SentAt = now,
            Attempts = 0
        }, cancellationToken);

        await _emailService.SendPasswordResetAsync(user.Email, code, cancellationToken);
    }

    private static string HashCode(string code)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
