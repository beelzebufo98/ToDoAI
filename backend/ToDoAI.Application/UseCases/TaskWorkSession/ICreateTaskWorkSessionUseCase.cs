using ToDoAI.Application.UseCases.TaskWorkSession.Models;

namespace ToDoAI.Application.UseCases.TaskWorkSession;

public interface ICreateTaskWorkSessionUseCase
{
    Task<TaskWorkSessionBlResult> CreateTaskWorkSession(CreateWorkSessionBlRequest request,
        CancellationToken cancellationToken);
}