using ToDoAI.Application.UseCases.TaskWorkSession.Models;

namespace ToDoAI.Application.UseCases.TaskWorkSession;

public interface IStopTaskWorkSessionUseCase
{
    Task<TaskWorkSessionBlResult> StopTaskWorkSession(Guid userId, Guid sessionId,
        CancellationToken cancellationToken);
}