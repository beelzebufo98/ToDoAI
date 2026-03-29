using Microsoft.AspNetCore.Identity;
using ToDoAI.ToDoAI.Application.Services.JwtService;
using ToDoAI.ToDoAI.Application.UseCases.CreateUser.Models;
using ToDoAI.ToDoAI.Application.UseCases.LoginUser.Models;
using ToDoAI.ToDoAI.Domain.Enums;
using ToDoAI.ToDoAI.Infrastructure.DalProviders.UserDalProvider;

namespace ToDoAI.ToDoAI.Application.UseCases.LoginUser;

public sealed class LoginUserUseCase : ILoginUserUseCase
{
    private readonly IUserDalProvider  _userDalProvider;
    private readonly IJwtService  _jwtService;
    private readonly ILogger<LoginUserUseCase> _logger;

    public LoginUserUseCase(IUserDalProvider userDalProvider,
        IJwtService jwtService,
        ILogger<LoginUserUseCase> logger)
    {
        _userDalProvider = userDalProvider;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<LoginUserResult> LoginUser(LoginUserBlRequest request, CancellationToken cancellationToken)
    {
        var account = await _userDalProvider.GetUser(request.Username, cancellationToken);
        if (account is null)
        {
            return new LoginUserResult
            {
                Success = false,
                Error = ErrorCodes.UserDoesNotExist
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
            var token = _jwtService.GenerateToken(account);
            return new LoginUserResult
            {
                Success = true,
                Token = token
            };
        }
        else
        {
            return new LoginUserResult
            {
                Success = false,
                Error = ErrorCodes.NotAuthorized
            };
        }
    }
}