using ToDoAI.Application.Abstractions.DalProviders.TaskWorkSessionDalProvider.Models;

namespace ToDoAI.Application.Abstractions.DalProviders.TaskWorkSessionDalProvider;

public interface ITaskWorkSessionDalProvider
{
    Task<TaskWorkSessionDal?> CreateTaskWorkSession(CreateWorkSessionDalRequest request, CancellationToken ct);

    Task<StopTaskWorkSessionDalResult> StopTaskWorkSessionAsync(Guid userId, Guid sessionId, CancellationToken ct);

    Task<CancelTaskWorkSessionDalResult> CancelTaskWorkSessionAsync(Guid userId, Guid sessionId,
        CancellationToken ct);

    Task<TaskWorkSessionDal?> GetTaskWorkSession(Guid userId, CancellationToken ct);

    Task<GetTaskWorkSessionsDalResult> GetTaskWorkSessions(Guid userId, DateOnly sessionDate, CancellationToken ct);
}
