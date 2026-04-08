using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider.Models;
using ToDoAI.Application.UseCases.GetTask.Models;

namespace ToDoAI.Application.UseCases.GetTask.Mappers;

public static class TaskResultMappers
{
    public static TaskResult GetTaskResult(this TaskDal taskDal)
    {
        return new TaskResult
        {
            Id = taskDal.Id,
            Title = taskDal.Title,
            Description = taskDal.Description,
            EstimatedMinutes = taskDal.EstimatedMinutes,
            ComplexityLevel = taskDal.ComplexityLevel,
            Priority = taskDal.Priority,
            CreatedAt = taskDal.CreatedAt,
            UpdatedAt = taskDal.UpdatedAt,
            ActualStartDate = taskDal.ActualStartDate,
            ActualEndDate = taskDal.ActualEndDate,
            WorkStatus = taskDal.WorkStatus
        };
    }
}