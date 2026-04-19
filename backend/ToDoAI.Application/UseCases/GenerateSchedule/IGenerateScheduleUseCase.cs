using ToDoAI.Application.UseCases.GenerateSchedule.Models;

namespace ToDoAI.Application.UseCases.GenerateSchedule;

public interface IGenerateScheduleUseCase
{
    Task<GenerateScheduleBlResult> GenerateSchedule(GenerateScheduleBlRequest request, CancellationToken cancellationToken);
}
