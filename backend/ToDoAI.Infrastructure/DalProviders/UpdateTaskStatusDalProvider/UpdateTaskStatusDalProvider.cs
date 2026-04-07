using Microsoft.EntityFrameworkCore;
using ToDoAI.Application.Abstractions.DalProviders.UpdateTaskStatusDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UpdateTaskStatusDalProvider.Models;
using ToDoAI.Domain.Enums;
using ToDoAI.Infrastructure.Data;

namespace ToDoAI.Infrastructure.DalProviders.UpdateTaskStatusDalProvider;

public sealed class UpdateTaskStatusDalProvider :  IUpdateTaskStatusDalProvider
{
    private readonly IDbContextFactory<ToDoAIDbContext>  _dbContextFactory;

    public UpdateTaskStatusDalProvider(IDbContextFactory<ToDoAIDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<UpdateTaskStatusDal?> UpdateTaskStatus(UpdateTaskStatusDalRequest updateTask,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        
        var taskEntity = await dbContext.Tasks.FirstOrDefaultAsync(x => x.UserId == updateTask.UserId && x.Id ==  updateTask.TaskId);

        if (taskEntity == null)
        {
            return null;
        }
        
        var workStatus = GetWorkStatus(updateTask.WorkStatus);

        switch (workStatus)
        {
            case WorkStatus.New:
                taskEntity.WorkStatus = WorkStatus.Todo;
                break;
            case WorkStatus.Todo:
                taskEntity.WorkStatus = WorkStatus.Running;
                var dateTime = DateTimeOffset.Now;
                taskEntity.ActualStartDate = dateTime;
                break;
            case WorkStatus.Running:
                taskEntity.WorkStatus = WorkStatus.Completed;
                break;
                
        }
        
        await dbContext.SaveChangesAsync(cancellationToken);
        var workStatusDal = GetTaskWorkStatusDal(taskEntity.WorkStatus);

        return new UpdateTaskStatusDal
        {
            WorkStatus = workStatusDal,
        };
    }

    private WorkStatus GetWorkStatus(TaskWorkStatusDal workStatus) => workStatus switch
    {
        TaskWorkStatusDal.New => WorkStatus.New,
        TaskWorkStatusDal.Todo => WorkStatus.Todo,
        TaskWorkStatusDal.Running => WorkStatus.Running,
        TaskWorkStatusDal.Completed => WorkStatus.Completed,
        _ => throw new ArgumentOutOfRangeException(nameof(workStatus), workStatus, null)
    };

    private TaskWorkStatusDal GetTaskWorkStatusDal(WorkStatus workStatus) => workStatus switch
    {
        WorkStatus.New => TaskWorkStatusDal.New,
        WorkStatus.Todo => TaskWorkStatusDal.Todo,
        WorkStatus.Running => TaskWorkStatusDal.Running,
        WorkStatus.Completed => TaskWorkStatusDal.Completed,
        _ => throw new ArgumentOutOfRangeException(nameof(workStatus), workStatus, null)
    };
}