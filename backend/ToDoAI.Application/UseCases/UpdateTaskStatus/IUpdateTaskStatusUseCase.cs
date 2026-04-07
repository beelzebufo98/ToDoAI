using ToDoAI.Application.UseCases.UpdateTaskStatus.Models;

namespace ToDoAI.Application.UseCases.UpdateTaskStatus;

public interface IUpdateTaskStatusUseCase
{
    Task<UpdateTaskStatusResult?> UpdateTaskStatus(UpdateTaskStatusBlRequest updateTaskStatusBlRequest,
        CancellationToken cancellationToken);
}