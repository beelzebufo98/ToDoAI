using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using ToDoAI.Application.Common;
using ToDoAI.Application.Abstractions.DalProviders.EmailConfirmationDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.EmailConfirmationDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider.Models;
using ToDoAI.Application.Services.EmailService;
using ToDoAI.Application.Services.EmailService.Settings;
using ToDoAI.Application.UseCases.CreateUser.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.CreateUser;

public sealed class CreateUserUseCase : ICreateUserUseCase
{
    private readonly IUserDalProvider  _userDalProvider;
    private readonly IEmailConfirmationDalProvider _emailConfirmationDalProvider;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<CreateUserUseCase> _logger;

    public CreateUserUseCase(
        IUserDalProvider userDalProvider,
        IEmailConfirmationDalProvider emailConfirmationDalProvider,
        IEmailService emailService,
        IOptions<EmailSettings> emailSettings,
        ILogger<CreateUserUseCase> logger)
    {
        _userDalProvider = userDalProvider;
        _emailConfirmationDalProvider = emailConfirmationDalProvider;
        _emailService = emailService;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }
    public async Task<RegisterUserResult> CreateUser(RegisterUserBlRequest user, CancellationToken cancellationToken)
    {
        var normalizedEmail = user.Email.Trim();
        var userExist = await _userDalProvider.CheckUserExists(user.UserName, cancellationToken);
        var emailExists = await _userDalProvider.CheckUserEmailExists(normalizedEmail, cancellationToken);
        if (userExist || emailExists)
        {
            return new RegisterUserResult
            {
                Success = false,
                Error = ErrorCodes.UserExists
            };
        }
        var userId = Guid.NewGuid();
        var userHash = new UserHash
        {
            Id = userId,
            UserName = user.UserName,
            FirstName = user.FirstName,
        };
        
        var passwordHash = new PasswordHasher<UserHash>().HashPassword(userHash, user.Password);

        var requestDal = new RegisterUserRequestDal
        {
            Id = userId,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = normalizedEmail,
            PasswordHash = passwordHash
        };

        await _userDalProvider.CreateUser(requestDal, cancellationToken);
        var emailConfirmationCode = ConfirmationCodeHelper.Generate();
        var now = DateTimeOffset.UtcNow;
        await _emailConfirmationDalProvider.ReplaceEmailConfirmation(new EmailConfirmationRequestDal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CodeHash = ConfirmationCodeHelper.Hash(emailConfirmationCode),
            ExpiresAt = now.AddMinutes(15),
            SentAt = now,
            Attempts = 0
        }, cancellationToken);

         //TODO: убрать, так как не даем зарегатьсч без подвержденного email
        if (_emailSettings.Enabled)
        {
            await _emailService.SendEmailConfirmationAsync(normalizedEmail, emailConfirmationCode, cancellationToken);
        }
        else
        {
            _logger.LogInformation(
                "Email confirmation sending is disabled. User {UserId} created without sending confirmation email.",
                userId);
        }

        return new RegisterUserResult
        {
            Success = true
        };
    }
}
