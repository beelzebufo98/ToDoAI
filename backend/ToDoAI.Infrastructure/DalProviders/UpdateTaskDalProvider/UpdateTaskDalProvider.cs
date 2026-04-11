using Microsoft.EntityFrameworkCore;
using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UpdateTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UpdateTaskDalProvider.Models;
using ToDoAI.Infrastructure.DalProviders.GetTaskDalProvider.Mappers;
using ToDoAI.Infrastructure.Data;

namespace ToDoAI.Infrastructure.DalProviders.UpdateTaskDalProvider;

public sealed class UpdateTaskDalProvider : IUpdateTaskDalProvider
{
    private readonly IDbContextFactory<ToDoAIDbContext> _dbContextFactory;

    public UpdateTaskDalProvider(IDbContextFactory<ToDoAIDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<TaskDal?> UpdateTask(UpdateTaskDalRequest updateTask, CancellationToken cancellation)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellation);
        
        var taskEntity = await dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == updateTask.TaskId && t.UserId == updateTask.UserId, cancellation);

        if (taskEntity is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(updateTask.Title))
        {
            taskEntity.Title = updateTask.Title;
        }

        if (!string.IsNullOrWhiteSpace(updateTask.Description))
        {
            taskEntity.Description = updateTask.Description;
        }

        if (updateTask.EstimatedMinutes.HasValue)
        {
            taskEntity.EstimatedMinutes = updateTask.EstimatedMinutes.Value;
        }

        if (updateTask.ComplexityLevel.HasValue)
        {
            taskEntity.ComplexityLevel = updateTask.ComplexityLevel.Value;
        }

        if (updateTask.Priority.HasValue)
        {
            taskEntity.Priority = updateTask.Priority.Value;
        }

        if (updateTask.DeadlineAt.HasValue)
        {
            taskEntity.DeadlineAt = updateTask.DeadlineAt.Value;
        }

        taskEntity.UpdatedAt = DateTimeOffset.UtcNow;
        
        await dbContext.SaveChangesAsync(cancellation);

        return taskEntity.GetTaskDal();
    }
}
