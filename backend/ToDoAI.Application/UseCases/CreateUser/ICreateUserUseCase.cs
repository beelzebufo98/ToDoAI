using ToDoAI.ToDoAI.Application.UseCases.CreateUser.Models;

namespace ToDoAI.ToDoAI.Application.UseCases.CreateUser;

public interface ICreateUserUseCase
{
    Task<RegisterUserResult> CreateUser(RegisterUserBlRequest user,  CancellationToken cancellationToken);
}