namespace ToDoAI.API.Controllers.TaskWorkSessionController.Models;

public sealed class GetAllTaskSessionByDayResponse
{
    public IReadOnlyCollection<TaskWorkSessionResponse> TaskWorkSessionResponses { get; init; } = [];
}