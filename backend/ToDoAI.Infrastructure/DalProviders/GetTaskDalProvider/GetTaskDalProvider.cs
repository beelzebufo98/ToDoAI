using Microsoft.EntityFrameworkCore;
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

    public async Task<IReadOnlyCollection<TaskDal>> GetTasks(Guid userId, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        
        var tasks = await toDoAiDb.Tasks.Where(x => x.UserId == userId && x.WorkStatus != WorkStatus.Deleted).ToListAsync(cancellationToken);
        
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
