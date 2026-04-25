using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider.Models;
using ToDoAI.Domain.Entities;

namespace ToDoAI.Infrastructure.DalProviders.GetTaskDalProvider.Mappers;

public static class TaskEntityMapper
{
    public static TaskDal GetTaskDal(this TaskEntity taskEntity)
    {
        return new TaskDal
        {
            Id = taskEntity.Id,
            Title = taskEntity.Title,
            Description = taskEntity.Description,
            EstimatedMinutes = taskEntity.EstimatedMinutes,
            ActualSpentMinutes = taskEntity.ActualSpentMinutes,
            ComplexityLevel = taskEntity.ComplexityLevel,
            Priority = taskEntity.Priority,
            DeadlineAt = taskEntity.DeadlineAt,
            CreatedAt = taskEntity.CreatedAt,
            UpdatedAt = taskEntity.UpdatedAt,
            ActualStartDate = taskEntity.ActualStartDate,
            ActualEndDate = taskEntity.ActualEndDate,
            WorkStatus = taskEntity.WorkStatus
        };
    }
}
