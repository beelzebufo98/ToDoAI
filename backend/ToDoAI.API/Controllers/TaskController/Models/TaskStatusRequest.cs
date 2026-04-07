namespace ToDoAI.API.Controllers.TaskController.Models;

public sealed class TaskStatusRequest
{
    public TaskWorkStatusFilters WorkStatus { get; init; }
}