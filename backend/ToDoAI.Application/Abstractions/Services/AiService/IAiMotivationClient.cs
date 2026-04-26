using ToDoAI.Application.Abstractions.Services.AiService.Models;

namespace ToDoAI.Application.Abstractions.Services.AiService;

public interface IAiMotivationClient
{
    Task<AiGenerateMotivationResult> GenerateMotivation(
        AiGenerateMotivationRequest request,
        CancellationToken cancellationToken);
}