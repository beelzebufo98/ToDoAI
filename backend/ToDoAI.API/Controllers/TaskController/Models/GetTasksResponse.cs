using ToDoAI.API.Controllers.TaskController.Models;

namespace ToDoAI.ToDoAI.API.Controllers.TaskController.Models;

public sealed class GetTasksResponse
{
   public IReadOnlyCollection<GetTaskResponse> Tasks { get; set; } = [];
}