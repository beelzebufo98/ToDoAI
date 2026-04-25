using System.Security.Cryptography;
using System.Text;
using ToDoAI.Application.Abstractions.DalProviders.EmailConfirmationDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.UseCases.ConfirmEmail.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.ConfirmEmail;

public sealed class ConfirmEmailUseCase : IConfirmEmailUseCase
{
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

        var emailConfirmation = await _emailConfirmationDalProvider.GetEmailConfirmation(
            user.UserId,
            HashCode(request.Code.Trim()),
            cancellationToken);

        if (emailConfirmation is null || emailConfirmation.ExpiresAt <= DateTimeOffset.UtcNow)
        {
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

    private static string HashCode(string code)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
