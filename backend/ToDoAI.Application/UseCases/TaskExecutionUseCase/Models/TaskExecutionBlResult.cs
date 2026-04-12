using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.TaskExecutionUseCase.Models;

public sealed record TaskExecutionBlResult
{
    public TaskExecutionResult TaskExecutionResult { get; init; } = null!;
    
    public ErrorCodes? ErrorCode { get; init; }
}