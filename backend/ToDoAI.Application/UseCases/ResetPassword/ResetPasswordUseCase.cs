using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using ToDoAI.Application.Abstractions.DalProviders.PasswordResetDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.RefreshTokenDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.UseCases.CreateUser.Models;
using ToDoAI.Application.UseCases.ResetPassword.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.ResetPassword;

public sealed class ResetPasswordUseCase : IResetPasswordUseCase
{
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

        var passwordReset = await _passwordResetDalProvider.GetPasswordReset(
            user.UserId,
            HashCode(request.Code.Trim()),
            cancellationToken);

        if (passwordReset is null || passwordReset.ExpiresAt <= DateTimeOffset.UtcNow)
        {
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

    private static string HashCode(string code)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
