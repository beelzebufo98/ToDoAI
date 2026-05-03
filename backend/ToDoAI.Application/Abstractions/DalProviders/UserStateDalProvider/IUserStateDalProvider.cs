using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider.Models;

namespace ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider;

public interface IUserStateDalProvider
{
    Task<UserStateDal> CreateUserState(UserStateDalRequest request, CancellationToken cancellationToken);

    Task<UserStateDal?> GetLatestUserState(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserStateDal>> GetUserStates(Guid userId, int limit,
        CancellationToken cancellationToken);

    Task<UserStateDal> UpdateUserState(UpdateUserStateDalRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<UserStateDal>> GetUserStatesByDays(Guid userId, int days,
        CancellationToken cancellationToken);
}