using ToDoAI.Application.UseCases.AssistTask.Models;

namespace ToDoAI.Application.UseCases.AssistTask;

public interface IAssistUseCase
{
    Task<AssistTaskBlResponse> GetAssistTask(AssistTaskBlRequest request,
        CancellationToken cancellationToken);
}