using ToDoAI.Application.UseCases.TaskWorkSession.Models;

namespace ToDoAI.Application.UseCases.TaskWorkSession;

public interface IGetTaskWorkSessionUseCase
{
    Task<TaskWorkSessionBlResult?> GetTaskWorkSession(Guid userId, CancellationToken ct);

    Task<GetAllSessionsByDayBlResult> GetAllSessionsByDay(Guid userId, DateOnly sessionDate,
        CancellationToken ct);
}
