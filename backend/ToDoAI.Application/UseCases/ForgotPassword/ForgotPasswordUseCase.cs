using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ToDoAI.Application.Common;
using ToDoAI.Application.Abstractions.DalProviders.PasswordResetDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.PasswordResetDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Services.EmailService;
using ToDoAI.Application.Services.EmailService.Settings;

namespace ToDoAI.Application.UseCases.ForgotPassword;

public sealed class ForgotPasswordUseCase : IForgotPasswordUseCase
{
    private static readonly TimeSpan PasswordResetLifetime = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ForgotPasswordCooldown = TimeSpan.FromSeconds(60);

    private readonly IUserDalProvider _userDalProvider;
    private readonly IPasswordResetDalProvider _passwordResetDalProvider;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<ForgotPasswordUseCase> _logger;

    public ForgotPasswordUseCase(
        IUserDalProvider userDalProvider,
        IPasswordResetDalProvider passwordResetDalProvider,
        IEmailService emailService,
        IOptions<EmailSettings> emailSettings,
        ILogger<ForgotPasswordUseCase> logger)
    {
        _userDalProvider = userDalProvider;
        _passwordResetDalProvider = passwordResetDalProvider;
        _emailService = emailService;
        _emailSettings = emailSettings.Value;
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

        if (!user.IsEmailConfirmed)
        {
            _logger.LogInformation(
                "Forgot password requested for unconfirmed email {Email}. Reset email was not sent.",
                normalizedEmail);
            return;
        }

        var existingPasswordReset = await _passwordResetDalProvider.GetPasswordReset(user.UserId, cancellationToken);
        if (existingPasswordReset is not null &&
            existingPasswordReset.SentAt > DateTimeOffset.UtcNow.Subtract(ForgotPasswordCooldown))
        {
            _logger.LogInformation(
                "Forgot password requested too frequently for user {UserId}.",
                user.UserId);
            return;
        }

        var code = ConfirmationCodeHelper.Generate();
        var now = DateTimeOffset.UtcNow;

        await _passwordResetDalProvider.ReplacePasswordReset(new PasswordResetRequestDal
        {
            Id = Guid.NewGuid(),
            UserId = user.UserId,
            CodeHash = ConfirmationCodeHelper.Hash(code),
            ExpiresAt = now.Add(PasswordResetLifetime),
            SentAt = now,
            Attempts = 0
        }, cancellationToken);

        if (_emailSettings.Enabled)
        {
            await _emailService.SendPasswordResetAsync(user.Email, code, cancellationToken);
        }
        else
        {
            _logger.LogInformation(
                "Password reset sending is disabled. Reset code was generated for user {UserId} without sending email.",
                user.UserId);
        }
    }
}
