using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ToDoAI.Application.Common;
using ToDoAI.Application.Abstractions.DalProviders.EmailConfirmationDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.EmailConfirmationDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Services.EmailService;
using ToDoAI.Application.Services.EmailService.Settings;

namespace ToDoAI.Application.UseCases.ResendConfirmationCode;

public sealed class ResendConfirmationCodeUseCase : IResendConfirmationCodeUseCase
{
    private static readonly TimeSpan ConfirmationCodeLifetime = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);

    private readonly IUserDalProvider _userDalProvider;
    private readonly IEmailConfirmationDalProvider _emailConfirmationDalProvider;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<ResendConfirmationCodeUseCase> _logger;

    public ResendConfirmationCodeUseCase(
        IUserDalProvider userDalProvider,
        IEmailConfirmationDalProvider emailConfirmationDalProvider,
        IEmailService emailService,
        IOptions<EmailSettings> emailSettings,
        ILogger<ResendConfirmationCodeUseCase> logger)
    {
        _userDalProvider = userDalProvider;
        _emailConfirmationDalProvider = emailConfirmationDalProvider;
        _emailService = emailService;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task ResendConfirmationCode(string email, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim();
        var user = await _userDalProvider.GetUserByEmail(normalizedEmail, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            _logger.LogInformation("Resend confirmation requested for unknown email {Email}.", normalizedEmail);
            return;
        }

        if (user.IsEmailConfirmed)
        {
            _logger.LogInformation(
                "Resend confirmation requested for already confirmed email {Email}.",
                normalizedEmail);
            return;
        }

        var existingEmailConfirmation = await _emailConfirmationDalProvider.GetEmailConfirmation(user.UserId, cancellationToken);
        if (existingEmailConfirmation is not null &&
            existingEmailConfirmation.SentAt > DateTimeOffset.UtcNow.Subtract(ResendCooldown))
        {
            _logger.LogInformation(
                "Resend confirmation requested too frequently for user {UserId}.",
                user.UserId);
            return;
        }

        var code = ConfirmationCodeHelper.Generate();
        var now = DateTimeOffset.UtcNow;

        await _emailConfirmationDalProvider.ReplaceEmailConfirmation(new EmailConfirmationRequestDal
        {
            Id = Guid.NewGuid(),
            UserId = user.UserId,
            CodeHash = ConfirmationCodeHelper.Hash(code),
            ExpiresAt = now.Add(ConfirmationCodeLifetime),
            SentAt = now,
            Attempts = 0
        }, cancellationToken);

        if (_emailSettings.Enabled)
        {
            await _emailService.SendEmailConfirmationAsync(user.Email, code, cancellationToken);
        }
        else
        {
            _logger.LogInformation(
                "Email confirmation sending is disabled. Confirmation code was regenerated for user {UserId} without sending email.",
                user.UserId);
        }
    }
}
