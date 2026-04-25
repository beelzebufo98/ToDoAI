using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.TaskWorkSession.Models;

public sealed record TaskWorkSessionBlResult
{
    public SessionBlResult? SessionBlResult { get; init; }
    
    public ErrorCodes?  ErrorCode { get; init; }
}