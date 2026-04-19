using ToDoAI.Application.UseCases.GetSchedule.Models;

namespace ToDoAI.Application.UseCases.GetSchedule;

public interface IGetScheduleUseCase
{
    Task<GetScheduleBlResult> GetSchedule(ScheduleBlRequest request,
        CancellationToken cancellationToken);
}
