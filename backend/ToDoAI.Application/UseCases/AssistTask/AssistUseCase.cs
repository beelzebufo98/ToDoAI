using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.Services.AiService;
using ToDoAI.Application.UseCases.AssistTask.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.AssistTask;

public sealed class AssistUseCase : IAssistUseCase
{
    private readonly IUserDalProvider _userDalProvider;
    private readonly IAiTaskAssistantClient _aiTaskAssistantClient;

    public AssistUseCase(
        IUserDalProvider userDalProvider,
        IAiTaskAssistantClient aiTaskAssistantClient)
    {
        _userDalProvider = userDalProvider;
        _aiTaskAssistantClient = aiTaskAssistantClient;
    }

    public async Task<AssistTaskBlResponse> GetAssistTask(AssistTaskBlRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userDalProvider.GetUser(request.UserId, cancellationToken);
        if (user is null)
        {
            return new AssistTaskBlResponse
            {
                Error = ErrorCodes.NotAuthorized
            };
        }

        var assistResult = await _aiTaskAssistantClient.GetAssistTask(
            new()
            {
                Title = request.Title,
                Description = request.Description,
                DeadlineAt = request.DeadlineAt
            },
            cancellationToken);

        return new AssistTaskBlResponse
        {
            SuggestedTitle = assistResult.SuggestedTitle,
            SuggestedDescription = assistResult.SuggestedDescription,
            SuggestedEstimatedMinutes = assistResult.SuggestedEstimatedMinutes,
            SuggestedComplexityLevel = assistResult.SuggestedComplexityLevel,
            SuggestedPriority = assistResult.SuggestedPriority,
            Reasoning = assistResult.Reasoning
        };
    }
}