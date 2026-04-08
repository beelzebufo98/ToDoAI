using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider.Models;
using ToDoAI.Application.UseCases.CreateUser.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.CreateUser;

public sealed class CreateUserUseCase : ICreateUserUseCase
{
    private readonly IUserDalProvider  _userDalProvider;
    private readonly ILogger<CreateUserUseCase> _logger;

    public CreateUserUseCase(IUserDalProvider userDalProvider, ILogger<CreateUserUseCase> logger)
    {
        _userDalProvider = userDalProvider;
        _logger = logger;
    }
    public async Task<RegisterUserResult> CreateUser(RegisterUserBlRequest user, CancellationToken cancellationToken)
    {
        var userExist = await _userDalProvider.CheckUserExists(user.UserName, cancellationToken);
        if (userExist)
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
            PasswordHash = passwordHash
        };

        await _userDalProvider.CreateUser(requestDal, cancellationToken);

        return new RegisterUserResult
        {
            Success = true
        };
    }
}
