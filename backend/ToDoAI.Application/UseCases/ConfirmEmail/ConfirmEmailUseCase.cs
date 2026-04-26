using ToDoAI.Application.Common;
using ToDoAI.Application.Abstractions.DalProviders.EmailConfirmationDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.UseCases.ConfirmEmail.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.ConfirmEmail;

public sealed class ConfirmEmailUseCase : IConfirmEmailUseCase
{
    private const int MaxConfirmationAttempts = 5;

    private readonly IUserDalProvider _userDalProvider;
    private readonly IEmailConfirmationDalProvider _emailConfirmationDalProvider;

    public ConfirmEmailUseCase(
        IUserDalProvider userDalProvider,
        IEmailConfirmationDalProvider emailConfirmationDalProvider)
    {
        _userDalProvider = userDalProvider;
        _emailConfirmationDalProvider = emailConfirmationDalProvider;
    }

    public async Task<ConfirmEmailResult> ConfirmEmail(ConfirmEmailBlRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim();
        var user = await _userDalProvider.GetUserByEmail(normalizedEmail, cancellationToken);
        if (user is null)
        {
            return InvalidRequest();
        }

        if (user.IsEmailConfirmed)
        {
            return new ConfirmEmailResult
            {
                Success = true
            };
        }

        var emailConfirmation = await _emailConfirmationDalProvider.GetEmailConfirmation(user.UserId, cancellationToken);

        if (emailConfirmation is null)
        {
            return InvalidRequest();
        }

        if (emailConfirmation.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            await _emailConfirmationDalProvider.DeleteEmailConfirmations(user.UserId, cancellationToken);
            return InvalidRequest();
        }

        var codeHash = ConfirmationCodeHelper.Hash(request.Code.Trim());
        if (emailConfirmation.CodeHash != codeHash)
        {
            await _emailConfirmationDalProvider.RegisterFailedAttempt(
                user.UserId,
                MaxConfirmationAttempts,
                cancellationToken);

            return InvalidRequest();
        }

        await _userDalProvider.UpdateEmailConfirmed(user.UserId, true, cancellationToken);
        await _emailConfirmationDalProvider.DeleteEmailConfirmations(user.UserId, cancellationToken);

        return new ConfirmEmailResult
        {
            Success = true
        };
    }

    private static ConfirmEmailResult InvalidRequest()
    {
        return new ConfirmEmailResult
        {
            Success = false,
            Error = ErrorCodes.IncorrectValue
        };
    }

}
