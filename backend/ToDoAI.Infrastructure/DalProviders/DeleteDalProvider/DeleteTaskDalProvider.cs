using Microsoft.EntityFrameworkCore;
using ToDoAI.Application.Abstractions.DalProviders.DeleteTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.DeleteTaskDalProvider.Models;
using ToDoAI.Domain.Enums;
using ToDoAI.Infrastructure.Data;

namespace ToDoAI.Infrastructure.DalProviders.DeleteDalProvider;

public sealed class DeleteTaskDalProvider : IDeleteTaskDalProvider
{
    private readonly IDbContextFactory<ToDoAIDbContext>  _dbContextFactory;

    public DeleteTaskDalProvider(IDbContextFactory<ToDoAIDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<DeleteTaskDal?> DeleteTask(Guid userId, Guid taskId, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var task = await dbContext.Tasks.FirstOrDefaultAsync(t => t.UserId == userId && t.Id == taskId, cancellationToken);
        if (task == null)
        {
            return null;
        }
        
        task.WorkStatus = WorkStatus.Deleted;
        task.DeletedAt = DateTimeOffset.UtcNow;
        task.UpdatedAt = DateTimeOffset.UtcNow;
        dbContext.Update(task);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return new DeleteTaskDal
        {
            TaskId = taskId,
        };
    }
}
