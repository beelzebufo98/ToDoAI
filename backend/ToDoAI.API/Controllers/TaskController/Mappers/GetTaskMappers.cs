using ToDoAI.ToDoAI.API.Controllers.TaskController.Models;
using ToDoAI.ToDoAI.Application.UseCases.GetTask.Models;

namespace ToDoAI.ToDoAI.API.Controllers.TaskController.Mappers;

public static class GetTaskMappers
{
    public static GetTaskResponse GetTask(this TaskResult result)
    {
        return new GetTaskResponse
        {
            Id = result.Id,
            Title = result.Title,
            Description = result.Description,
            EstimatedMinutes = result.EstimatedMinutes,
            ComplexityLevel = result.ComplexityLevel,
            Priority = result.Priority,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt,
            ActualStartDate = result.ActualStartDate,
            ActualEndDate = result.ActualEndDate,
            WorkStatus = result.WorkStatus
        };
    }
}
