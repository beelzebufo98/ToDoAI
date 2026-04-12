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
}
