namespace ToDoAI.Application.UseCases.GetTask.Models;

public sealed record TaskFiltersBlRequest
{
    public TaskWorkStatusBlFilters? WorkStatus { get; init; }
    
    public TaskTypeSortBl? SortType { get; init; }
    
    public TaskSortByBlFilters? SortBy { get; init; }
    
    public int? Page { get; init; }

    public required int PageSize { get; init; }
}