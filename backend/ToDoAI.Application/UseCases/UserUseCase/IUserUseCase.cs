using ToDoAI.Application.UseCases.UserUseCase.Models;

namespace ToDoAI.Application.UseCases.UserUseCase;

public interface IUserUseCase
{
    Task<UserBlResult> GetUser(Guid userId, CancellationToken cancellationToken);
}