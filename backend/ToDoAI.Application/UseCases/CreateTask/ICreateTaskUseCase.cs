using ToDoAI.Application.UseCases.CreateTask.Models;

namespace ToDoAI.Application.UseCases.CreateTask;

public interface ICreateTaskUseCase
{
    Task<CreateTaskResult> CreateTask(CreateTaskBlRequest task, CancellationToken cancellationToken);
}