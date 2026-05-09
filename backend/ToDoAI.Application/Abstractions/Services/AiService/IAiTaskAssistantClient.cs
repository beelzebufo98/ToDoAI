using ToDoAI.Application.Abstractions.Services.AiService.Models;

namespace ToDoAI.Application.Abstractions.Services.AiService;

public interface IAiTaskAssistantClient
{
    Task<AiAssistTaskResult> GetAssistTask(AiAssistTaskRequest request,
        CancellationToken cancellationToken);
}