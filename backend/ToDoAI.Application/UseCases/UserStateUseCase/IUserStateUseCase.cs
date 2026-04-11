using ToDoAI.Application.UseCases.UserStateUseCase.Models;

namespace ToDoAI.Application.UseCases.UserStateUseCase;

public interface IUserStateUseCase
{
    Task<UserStateBlResult> CreateUserState(CreateUserStateBlRequest userStateRequest,
        CancellationToken cancellationToken);
}