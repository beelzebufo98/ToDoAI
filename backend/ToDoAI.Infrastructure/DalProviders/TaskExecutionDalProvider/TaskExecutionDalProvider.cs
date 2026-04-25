using Microsoft.EntityFrameworkCore;
using ToDoAI.Application.Abstractions.DalProviders.TaskExecutionDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.TaskExecutionDalProvider.Models;
using ToDoAI.Domain.Entities;
using ToDoAI.Infrastructure.Data;

namespace ToDoAI.Infrastructure.DalProviders.TaskExecutionDalProvider;

public sealed class TaskExecutionDalProvider : ITaskExecutionDalProvider
{
    private readonly IDbContextFactory<ToDoAIDbContext>  _dbContextFactory;

    public TaskExecutionDalProvider(IDbContextFactory<ToDoAIDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<TaskExecutionDal?> CreateTaskExecution(TaskExecutionDalRequest request, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        
        var taskExecution = await dbContext.TaskExecutions.FirstOrDefaultAsync(e => e.TaskId == request.TaskId, cancellationToken);
        if (taskExecution is not null)
        {
            return null;
        }
        
        var entity = new TaskExecutionEntity
        {
            Id = Guid.NewGuid(),
            TaskId = request.TaskId,
            ScheduleId = request.ScheduleId,
            ActualMinutes = request.ActualMinutes,
            EnergyAfter = request.EnergyAfter,
            StressAfter = request.StressAfter,
            CreatedAt = request.CreatedAt
        };
        
        await dbContext.TaskExecutions.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new TaskExecutionDal
        {
            TaskExecutionId = entity.Id,
            TaskId = entity.TaskId,
            ScheduleId = entity.ScheduleId,
            ActualMinutes = entity.ActualMinutes,
            EnergyAfter = entity.EnergyAfter,
            StressAfter = entity.StressAfter,
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task<IReadOnlyCollection<RecentTaskExecutionDal>> GetRecentTaskExecutions(
        Guid userId,
        int count,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.TaskExecutions
            .AsNoTracking()
            .Where(e => e.Task.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .Take(count)
            .Select(e => new RecentTaskExecutionDal
            {
                TaskExecutionId = e.Id,
                TaskId = e.TaskId,
                TaskTitle = e.Task.Title,
                TaskEstimatedMinutes = e.Task.EstimatedMinutes,
                TaskComplexityLevel = e.Task.ComplexityLevel,
                TaskPriority = e.Task.Priority,
                ActualMinutes = e.ActualMinutes,
                EnergyAfter = e.EnergyAfter,
                StressAfter = e.StressAfter,
                CreatedAt = e.CreatedAt
            })
            .ToArrayAsync(cancellationToken);
    }
}
