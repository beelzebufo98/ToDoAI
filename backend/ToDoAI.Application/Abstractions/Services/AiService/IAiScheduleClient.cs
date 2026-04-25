using ToDoAI.Application.Abstractions.Services.AiService.Models;

namespace ToDoAI.Application.Abstractions.Services.AiService;

public interface IAiScheduleClient
{
    Task<AiGenerateScheduleResult> GenerateSchedule(
        AiGenerateScheduleRequest request,
        CancellationToken cancellationToken);
}
