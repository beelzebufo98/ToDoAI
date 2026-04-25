using Microsoft.EntityFrameworkCore;
using Npgsql;
using ToDoAI.Application.Abstractions.DalProviders.TaskWorkSessionDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.TaskWorkSessionDalProvider.Models;
using ToDoAI.Domain.Entities;
using ToDoAI.Domain.Enums;
using ToDoAI.Infrastructure.Data;

namespace ToDoAI.Infrastructure.DalProviders.TaskWorkSessionDalProvider;

public sealed class TaskWorkSessionDalProvider(IDbContextFactory<ToDoAIDbContext> dbContextFactory)
    : ITaskWorkSessionDalProvider
{

    public async Task<GetTaskWorkSessionsDalResult> GetTaskWorkSessions(Guid userId, DateOnly sessionDate, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        var dayStartUtc = sessionDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var nextDayStartUtc = dayStartUtc.AddDays(1);

        var sessionEntities = await dbContext.TaskWorkSessions
            .AsNoTracking()
            .Include(x => x.Task)
            .Where(x => x.UserId == userId
                        && x.StartedAt >= dayStartUtc
                        && x.StartedAt < nextDayStartUtc)
            .OrderBy(x => x.StartedAt)
            .ToListAsync(ct);

        var totalCompletedSessions = sessionEntities.Count(x => x.Status == TaskWorkSessionStatus.Completed);
        var completedTasksMinutes = sessionEntities
            .Where(x => x.Status == TaskWorkSessionStatus.Completed)
            .Sum(x => x.SpentMinutes ?? 0);
        var totalSessions = sessionEntities.Count;

        var sessionList = new List<TaskWorkSessionDal>(totalSessions);

        foreach (var session in sessionEntities)
        {
            var dalResult = new TaskWorkSessionDal
            {
                SessionId = session.Id,
                UserId = session.UserId,
                TaskId = session.TaskId,
                ScheduleBlockId = session.ScheduleBlockId,
                Status = session.Status,
                Source = session.Source,
                StartedAt = session.StartedAt,
                EndedAt = session.EndedAt,
                SpentMinutes = session.SpentMinutes,
                CreatedAt = session.CreatedAt,
                Title = session.Task.Title,
                TaskEstimatedMinutes = session.Task.EstimatedMinutes,
                TaskActualSpentMinutes = session.Task.ActualSpentMinutes,
            };
            sessionList.Add(dalResult);
        }

        return new GetTaskWorkSessionsDalResult
        {
            TaskWorkSessions = sessionList,
            TotalCompletedSessions = totalCompletedSessions,
            TotalSessions = totalSessions,
            TotalCompletedMinutes = completedTasksMinutes,
        };
    }
    
    public async Task<TaskWorkSessionDal?> GetTaskWorkSession(Guid userId, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        
        var session = await dbContext.TaskWorkSessions
            .AsNoTracking()
            .Include(x => x.Task)
            .Where(x => x.UserId == userId && x.Status == TaskWorkSessionStatus.Open)
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync(ct);

        if (session is null)
        {
            return null;
        }
        
        var result = new TaskWorkSessionDal
        {
            SessionId = session.Id,
            UserId = session.UserId,
            TaskId = session.TaskId,
            ScheduleBlockId = session.ScheduleBlockId,
            Status = session.Status,
            Source = session.Source,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            SpentMinutes = session.SpentMinutes,
            CreatedAt = session.CreatedAt,
            Title = session.Task.Title,
            TaskEstimatedMinutes = session.Task.EstimatedMinutes,
            TaskActualSpentMinutes = session.Task.ActualSpentMinutes,
        };
        
        return result;
    }
    public async Task<CancelTaskWorkSessionDalResult> CancelTaskWorkSessionAsync(Guid userId, Guid sessionId,
        CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var taskSession = await dbContext.TaskWorkSessions
            .Include(x => x.Task)
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId && x.Task.UserId == userId, ct);
        
        if (taskSession is null)
        {
            return new CancelTaskWorkSessionDalResult
            {
                ErrorCode = ErrorCodes.SessionNotFound
            };
        }

        if (taskSession.Status != TaskWorkSessionStatus.Open)
        {
            return new CancelTaskWorkSessionDalResult
            {
                ErrorCode = ErrorCodes.InvalidTaskWorkSessionStatus
            };
        }
        
        taskSession.EndedAt = DateTimeOffset.UtcNow;
        taskSession.SpentMinutes = null;
        taskSession.Status = TaskWorkSessionStatus.Cancelled;
        
        await dbContext.SaveChangesAsync(ct);
        
        return new CancelTaskWorkSessionDalResult
        {
            Session = new TaskWorkSessionDal
            {
                SessionId = taskSession.Id,
                UserId = taskSession.UserId,
                TaskId = taskSession.TaskId,
                ScheduleBlockId = taskSession.ScheduleBlockId,
                Status = taskSession.Status,
                Source = taskSession.Source,
                StartedAt = taskSession.StartedAt,
                EndedAt = taskSession.EndedAt,
                SpentMinutes = taskSession.SpentMinutes,
                CreatedAt = taskSession.CreatedAt,
                TaskEstimatedMinutes = taskSession.Task.EstimatedMinutes,
                TaskActualSpentMinutes = taskSession.Task.ActualSpentMinutes
            }
        };
    }

    public async Task<StopTaskWorkSessionDalResult> StopTaskWorkSessionAsync(Guid userId, Guid sessionId, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
        var taskSession = await dbContext.TaskWorkSessions
            .Include(x => x.Task)
            .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId && x.Task.UserId == userId, ct);

        if (taskSession is null)
        {
            return new StopTaskWorkSessionDalResult
            {
                ErrorCode = ErrorCodes.SessionNotFound
            };
        }

        if (taskSession.Status != TaskWorkSessionStatus.Open)
        {
            return new StopTaskWorkSessionDalResult
            {
                ErrorCode = ErrorCodes.InvalidTaskWorkSessionStatus
            };
        }
        
        taskSession.EndedAt = DateTimeOffset.UtcNow;
        taskSession.SpentMinutes =  Math.Max(1, (int)Math.Ceiling((taskSession.EndedAt - taskSession.StartedAt).Value.TotalMinutes));
        taskSession.Status = TaskWorkSessionStatus.Completed;
        taskSession.Task.ActualSpentMinutes += taskSession.SpentMinutes.Value;
        taskSession.Task.UpdatedAt = DateTimeOffset.UtcNow;
        
        await dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        
        return new StopTaskWorkSessionDalResult
        {
            Session = new TaskWorkSessionDal
            {
                SessionId = taskSession.Id,
                UserId = taskSession.UserId,
                TaskId = taskSession.TaskId,
                ScheduleBlockId = taskSession.ScheduleBlockId,
                Status = taskSession.Status,
                Source = taskSession.Source,
                StartedAt = taskSession.StartedAt,
                EndedAt = taskSession.EndedAt,
                SpentMinutes = taskSession.SpentMinutes,
                CreatedAt = taskSession.CreatedAt,
                TaskEstimatedMinutes = taskSession.Task.EstimatedMinutes,
                TaskActualSpentMinutes = taskSession.Task.ActualSpentMinutes
            }
        };
    }
    public async Task<TaskWorkSessionDal?> CreateTaskWorkSession(CreateWorkSessionDalRequest request, CancellationToken ct)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        
        var taskSession = await dbContext.TaskWorkSessions.FirstOrDefaultAsync(x => x.UserId == request.UserId && x.Status == TaskWorkSessionStatus.Open, ct);
        if (taskSession is not null)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var sessionEntity = new TaskWorkSessionEntity
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            TaskId = request.TaskId,
            ScheduleBlockId = request.ScheduleId,
            Status = TaskWorkSessionStatus.Open,
            Source = request.ScheduleId != null ? TaskWorkSessionSource.ScheduleBlock : TaskWorkSessionSource.Timer,
            StartedAt = now,
            EndedAt = null,
            SpentMinutes = null,
            CreatedAt = now
        };
        await dbContext.TaskWorkSessions.AddAsync(sessionEntity, ct);
        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
                                                {
                                                    SqlState: PostgresErrorCodes.UniqueViolation
                                                })
        {
            return null;
        }

        var result = new TaskWorkSessionDal
        {
            SessionId = sessionEntity.Id,
            UserId = sessionEntity.UserId,
            TaskId = sessionEntity.TaskId,
            ScheduleBlockId = sessionEntity.ScheduleBlockId,
            Status = TaskWorkSessionStatus.Open,
            Source = sessionEntity.Source,
            StartedAt = sessionEntity.StartedAt,
            EndedAt = sessionEntity.EndedAt,
            SpentMinutes = sessionEntity.SpentMinutes,
            CreatedAt = sessionEntity.CreatedAt
        };
        
        return result;
    }
}
