using ToDoAI.Application.UseCases.TaskWorkSession.Models;

namespace ToDoAI.Application.UseCases.TaskWorkSession;

public interface ICancelTaskWorkSessionUseCase
{
    Task<TaskWorkSessionBlResult> CancelTaskWorkSession(Guid userId, Guid sessionId,
        CancellationToken cancellationToken);
}
