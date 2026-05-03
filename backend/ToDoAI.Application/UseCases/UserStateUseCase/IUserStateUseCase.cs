using ToDoAI.Application.UseCases.UserStateUseCase.Models;

namespace ToDoAI.Application.UseCases.UserStateUseCase;

public interface IUserStateUseCase
{
    Task<UserStateBlResult> CreateUserState(CreateUserStateBlRequest userStateRequest,
        CancellationToken cancellationToken);

    Task<UserStateBlResult> GetLatestUserState(Guid userId, CancellationToken cancellationToken);

    Task<UserStateStatisticsBlResult> GetUserStateStatistics(Guid userId, int days,
        CancellationToken cancellationToken);

    Task<UserStateHistoryBlResult> GetUserStateHistory(Guid userId, int limit,
        CancellationToken cancellationToken);
}