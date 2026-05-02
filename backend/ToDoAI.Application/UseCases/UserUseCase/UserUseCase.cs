using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.UseCases.UserUseCase.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.UserUseCase;

public sealed class UserUseCase : IUserUseCase
{
    private readonly IUserDalProvider  _userDalProvider;

    public UserUseCase(IUserDalProvider userDalProvider)
    {
        _userDalProvider = userDalProvider;
    }

    public async Task<UserBlResult> GetUser(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userDalProvider.GetUser(userId, cancellationToken);

        if (user is null)
        {
            return new UserBlResult
            {
                Error = ErrorCodes.NotAuthorized
            };
        }

        return new UserBlResult
        {
            UserResult = new UserResult
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            }
        };
    }
}
