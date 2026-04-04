namespace ToDoAI.ToDoAI.API.Controllers.TaskController.Models;

public sealed class TaskFiltersRequest
{
    public TaskWorkStatusFilters? WorkStatus { get; init; }

    public TaskSortByFilters? SortBy { get; init; } = TaskSortByFilters.CreatedAt;

    public TaskTypeSort? SortType { get; init; } = TaskTypeSort.Desc;
    
    public int? Page { get; init; }

    public int PageSize { get; init; } = 10;
}