using Microsoft.EntityFrameworkCore;
using ToDoAI.ToDoAI.Application.UseCases.GetTask.Models;
using ToDoAI.ToDoAI.Domain.Entities;
using ToDoAI.ToDoAI.Domain.Enums;
using ToDoAI.ToDoAI.Infrastructure.DalProviders.GetTaskDalProvider.Models;
using ToDoAI.ToDoAI.Infrastructure.Data;

namespace ToDoAI.ToDoAI.Infrastructure.DalProviders.GetTaskDalProvider;

public sealed class GetTaskDalProvider : IGetTaskDalProvider
{
    private readonly IDbContextFactory<ToDoAIDbContext> _dbContextFactory;

    public GetTaskDalProvider(IDbContextFactory<ToDoAIDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<TaskDal?> GetTask(Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        
        var task = await toDoAiDb.Tasks.FirstOrDefaultAsync(
            x => x.Id == taskId && x.UserId == userId && x.WorkStatus != WorkStatus.Deleted,
            cancellationToken);

        if (task is null)
        {
            return null;
        }
        
        var taskDal = GetTaskDal(task);
        return taskDal;
    }

    public async Task<IReadOnlyCollection<TaskDal>> GetTasks(Guid userId, TaskFiltersBlRequest filters, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        IQueryable<TaskEntity> query = toDoAiDb.Tasks
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.WorkStatus != WorkStatus.Deleted);

        if (filters.WorkStatus is not null)
        {
            switch (filters.WorkStatus)
            {
                case TaskWorkStatusBlFilters.New:
                    query = query.Where(x => x.WorkStatus == WorkStatus.New);
                    break;
                case TaskWorkStatusBlFilters.Todo:
                    query = query.Where(x => x.WorkStatus == WorkStatus.Todo);
                    break;
                case TaskWorkStatusBlFilters.Running:
                    query = query.Where(x => x.WorkStatus == WorkStatus.Running);
                    break;
                case TaskWorkStatusBlFilters.Completed:
                    query = query.Where(x => x.WorkStatus == WorkStatus.Completed);
                    break;
                default:
                    break;
            }
        }

        bool isDescending = false;
        if (filters.SortType is not null)
        {
            switch (filters.SortType)
            {
                case TaskTypeSortBl.Asc:
                    isDescending = false;
                    break;
                case TaskTypeSortBl.Desc:
                    isDescending = true;
                    break;
                default:
                    isDescending = false;
                    break;
            }
        }

        if (filters.SortBy is not null)
        {
            switch (filters.SortBy)
            {
                case TaskSortByBlFilters.CreatedAt:
                    query = isDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt);
                    break;
                case TaskSortByBlFilters.ComplexityLevel:
                    query = isDescending ? query.OrderByDescending(x => x.ComplexityLevel) : query.OrderBy(x => x.ComplexityLevel);
                    break;
                case TaskSortByBlFilters.Priority:
                    query = isDescending ? query.OrderByDescending(x => x.Priority) : query.OrderBy(x => x.Priority);
                    break;
                default:
                    break;
            }
        }
        else
        {
            query = isDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt);
        }

        if (filters.Page is not null)
        {
            query = query.Skip((filters.Page.Value - 1) * filters.PageSize);
        }

        query = query.Take(filters.PageSize);

        var tasks = await query.ToListAsync(cancellationToken);
        var taskList = tasks.Select(GetTaskDal).ToList();
        return taskList;
    }

    private TaskDal GetTaskDal(TaskEntity taskEntity)
    {
        return new TaskDal
        {
            Id = taskEntity.Id,
            Title = taskEntity.Title,
            Description = taskEntity.Description,
            EstimatedMinutes = taskEntity.EstimatedMinutes,
            ComplexityLevel = taskEntity.ComplexityLevel,
            Priority = taskEntity.Priority,
            CreatedAt = taskEntity.CreatedAt,
            UpdatedAt = taskEntity.UpdatedAt,
            ActualStartDate = taskEntity.ActualStartDate,
            ActualEndDate = taskEntity.ActualEndDate,
            WorkStatus = taskEntity.WorkStatus
        };
    }
}
