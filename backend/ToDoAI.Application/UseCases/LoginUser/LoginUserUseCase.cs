using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ToDoAI.Application.Abstractions.DalProviders.RefreshTokenDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.RefreshTokenDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.Services.AiService;
using ToDoAI.Application.Abstractions.Services.AiService.Models;
using ToDoAI.Application.Services.JwtService;
using ToDoAI.Application.Services.JwtService.Settings;
using ToDoAI.Application.UseCases.CreateUser.Models;
using ToDoAI.Application.UseCases.LoginUser.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.LoginUser;

public sealed class LoginUserUseCase : ILoginUserUseCase
{
    private readonly IUserDalProvider  _userDalProvider;
    private readonly IRefreshTokenDalProvider _refreshTokenDalProvider;
    private readonly IAiMotivationClient _aiMotivationClient;
    private readonly IJwtService  _jwtService;
    private readonly IOptions<AuthSettings> _authSettings;
    private readonly ILogger<LoginUserUseCase> _logger;

    public LoginUserUseCase(IUserDalProvider userDalProvider,
        IRefreshTokenDalProvider refreshTokenDalProvider,
        IAiMotivationClient aiMotivationClient,
        IJwtService jwtService,
        IOptions<AuthSettings> authSettings,
        ILogger<LoginUserUseCase> logger)
    {
        _userDalProvider = userDalProvider;
        _refreshTokenDalProvider = refreshTokenDalProvider;
        _aiMotivationClient = aiMotivationClient;
        _jwtService = jwtService;
        _authSettings = authSettings;
        _logger = logger;
    }

    public async Task<LoginUserResult> LoginUser(LoginUserBlRequest request, CancellationToken cancellationToken)
    {
        var account = await _userDalProvider.GetUser(request.UserName, cancellationToken);
        if (account is null)
        {
            return new LoginUserResult
            {
                Success = false,
                Error = ErrorCodes.UserDoesNotExist
            };
        }

        if (!account.IsEmailConfirmed)
        {
            return new LoginUserResult
            {
                Success = false,
                Error = ErrorCodes.EmailNotConfirmed
            };
        }

        var userHash = new UserHash
        {
            Id = account.UserId,
            UserName = account.UserName,
            FirstName = account.FirstName,
        };
        
        var result = new PasswordHasher<UserHash>().VerifyHashedPassword(userHash, account.PasswordHash,  request.Password);

        if (result == PasswordVerificationResult.Success)
        {
            var accessToken = _jwtService.GenerateAccessToken(account);
            var refreshToken = _jwtService.GenerateRefreshToken(account);
            var refreshTokenHash = _jwtService.HashRefreshToken(refreshToken);

            var refreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(7);
            if (TimeSpan.TryParse(_authSettings.Value.RefreshTokenLifetime, out var refreshTokenLifetime))
            {
                refreshTokenExpiresAt = DateTimeOffset.UtcNow.Add(refreshTokenLifetime);
            }

            await _refreshTokenDalProvider.CreateRefreshToken(new RefreshTokenRequestDal
            {
                UserId = account.UserId,
                RefreshTokenHash = refreshTokenHash,
                ExpiresAt = refreshTokenExpiresAt,
                CreatedAt = DateTimeOffset.UtcNow
            }, cancellationToken);

            var motivationMessage = await GetLoginMotivationMessage(cancellationToken);

            return new LoginUserResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                MotivationMessage = motivationMessage
            };
        }
        return new LoginUserResult
        {
            Success = false,
            Error = ErrorCodes.NotAuthorized
        };
    }

    private async Task<string> GetLoginMotivationMessage(CancellationToken cancellationToken)
    {
        var aiResult = await _aiMotivationClient.GenerateMotivation(
            new AiGenerateMotivationRequest
            {
                Trigger = "login"
            },
            cancellationToken);

        var message = aiResult.Response?.Message?.Trim();
        if (!string.IsNullOrWhiteSpace(message))
        {
            return message;
        }

        return "Хорошее начало дня начинается с одного уверенного шага.";
    }
}
