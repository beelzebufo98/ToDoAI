using Microsoft.EntityFrameworkCore;
using ToDoAI.Application.Abstractions.DalProviders.CreateTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.CreateTaskDalProvider.Models;
using ToDoAI.Domain.Entities;
using ToDoAI.Infrastructure.Data;

namespace ToDoAI.Infrastructure.DalProviders.CreateTaskDalProvider;

public sealed class CreateTaskDalProvider : ICreateTaskDalProvider
{
    private readonly IDbContextFactory<ToDoAIDbContext> _dbContextFactory;

    public CreateTaskDalProvider(IDbContextFactory<ToDoAIDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<CreateTaskDal> CreateTask(CreateTaskReqDal task, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var taskEntity = GetTaskEntity(task);
        
        await toDoAiDb.Tasks.AddAsync(taskEntity, cancellationToken);
        await toDoAiDb.SaveChangesAsync(cancellationToken);
        return new CreateTaskDal(taskEntity.Id);
    }

    private TaskEntity GetTaskEntity(CreateTaskReqDal task)
    {
        return new TaskEntity
        {
            Id = task.TaskId,
            Title = task.Title,
            Description = task.Description,
            EstimatedMinutes = task.EstimatedMinutes,
            ComplexityLevel = task.ComplexityLevel,
            Priority = task.PriorityLevel,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            WorkStatus = task.Status,
            UserId = task.UserId
        };
    }
}
