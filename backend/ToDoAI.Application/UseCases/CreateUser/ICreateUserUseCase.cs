using ToDoAI.ToDoAI.Application.UseCases.CreateUser.Models;

namespace ToDoAI.ToDoAI.Application.UseCases.CreateUser;

public interface ICreateUserUseCase
{
    Task CreateUserAsync(RegisterUserBlRequest user);
}