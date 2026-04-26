using Microsoft.AspNetCore.Identity;
using ToDoAI.Application.Common;
using ToDoAI.Application.Abstractions.DalProviders.PasswordResetDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.RefreshTokenDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.UseCases.CreateUser.Models;
using ToDoAI.Application.UseCases.ResetPassword.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.ResetPassword;

public sealed class ResetPasswordUseCase : IResetPasswordUseCase
{
    private const int MaxResetAttempts = 5;

    private readonly IUserDalProvider _userDalProvider;
    private readonly IPasswordResetDalProvider _passwordResetDalProvider;
    private readonly IRefreshTokenDalProvider _refreshTokenDalProvider;

    public ResetPasswordUseCase(
        IUserDalProvider userDalProvider,
        IPasswordResetDalProvider passwordResetDalProvider,
        IRefreshTokenDalProvider refreshTokenDalProvider)
    {
        _userDalProvider = userDalProvider;
        _passwordResetDalProvider = passwordResetDalProvider;
        _refreshTokenDalProvider = refreshTokenDalProvider;
    }

    public async Task<ResetPasswordResult> ResetPassword(ResetPasswordBlRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim();
        var user = await _userDalProvider.GetUserByEmail(normalizedEmail, cancellationToken);
        if (user is null)
        {
            return InvalidRequest();
        }

        var passwordReset = await _passwordResetDalProvider.GetPasswordReset(user.UserId, cancellationToken);

        if (passwordReset is null)
        {
            return InvalidRequest();
        }

        if (passwordReset.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            await _passwordResetDalProvider.DeletePasswordResets(user.UserId, cancellationToken);
            return InvalidRequest();
        }

        var codeHash = ConfirmationCodeHelper.Hash(request.Code.Trim());
        if (passwordReset.CodeHash != codeHash)
        {
            await _passwordResetDalProvider.RegisterFailedAttempt(
                user.UserId,
                MaxResetAttempts,
                cancellationToken);

            return InvalidRequest();
        }

        var userHash = new UserHash
        {
            Id = user.UserId,
            UserName = user.UserName,
            FirstName = user.FirstName,
        };

        var passwordHash = new PasswordHasher<UserHash>().HashPassword(userHash, request.NewPassword);

        await _userDalProvider.UpdatePassword(user.UserId, passwordHash, cancellationToken);
        await _passwordResetDalProvider.DeletePasswordResets(user.UserId, cancellationToken);
        await _refreshTokenDalProvider.DeleteRefreshTokens(user.UserId, cancellationToken);

        return new ResetPasswordResult
        {
            Success = true
        };
    }

    private static ResetPasswordResult InvalidRequest()
    {
        return new ResetPasswordResult
        {
            Success = false,
            Error = ErrorCodes.IncorrectValue
        };
    }

}
