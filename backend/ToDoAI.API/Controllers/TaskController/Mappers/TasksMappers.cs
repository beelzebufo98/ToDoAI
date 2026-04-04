using ToDoAI.ToDoAI.API.Controllers.TaskController.Models;
using ToDoAI.ToDoAI.Application.UseCases.GetTask.Models;

namespace ToDoAI.ToDoAI.API.Controllers.TaskController.Mappers;

public static class TasksMappers
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

    public static TaskWorkStatusBlFilters GetTaskWorkStatusBlFilters(this TaskWorkStatusFilters filter) => filter switch
    {
        TaskWorkStatusFilters.Todo => TaskWorkStatusBlFilters.Todo,
        TaskWorkStatusFilters.New => TaskWorkStatusBlFilters.New,
        TaskWorkStatusFilters.Running => TaskWorkStatusBlFilters.Running,
        TaskWorkStatusFilters.Completed => TaskWorkStatusBlFilters.Completed,
        _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
    };

    public static TaskSortByBlFilters GetSortByBlFilters(this TaskSortByFilters filter) => filter switch
    {
        TaskSortByFilters.CreatedAt => TaskSortByBlFilters.CreatedAt,
        TaskSortByFilters.ComplexityLevel => TaskSortByBlFilters.ComplexityLevel,
        TaskSortByFilters.Priority => TaskSortByBlFilters.Priority,
        _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
    };

    public static TaskTypeSortBl GetTaskTypeSortBl(this TaskTypeSort filter) => filter switch
    {
        TaskTypeSort.Asc => TaskTypeSortBl.Asc,
        TaskTypeSort.Desc => TaskTypeSortBl.Desc,
        _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
    };
}