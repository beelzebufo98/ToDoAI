using ToDoAI.Application.UseCases.TaskExecutionUseCase.Models;

namespace ToDoAI.Application.UseCases.TaskExecutionUseCase;

public interface ITaskExecutionUseCase
{
    Task<TaskExecutionBlResult> CreateTaskExecution(TaskExecutionBlRequest taskExecutionBlRequest,
        CancellationToken cancellationToken);
}