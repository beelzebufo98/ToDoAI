using ToDoAI.ToDoAI.Application.UseCases.CreateTask.Models;

namespace ToDoAI.ToDoAI.Application.UseCases.CreateTask;

public interface ICreateTaskUseCase
{
    Task<CreateTaskResult> CreateTask(CreateTaskBlRequest task, CancellationToken cancellationToken);
}