using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider.Models;

namespace ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider;

public interface IUserStateDalProvider
{
    Task<UserStateDal> CreateUserState(UserStateDalRequest request, CancellationToken cancellationToken);
}