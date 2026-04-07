namespace ToDoAI.Application.Abstractions.DalProviders.UpdateTaskStatusDalProvider.Models;

public sealed record UpdateTaskStatusDal
{
    public TaskWorkStatusDal  WorkStatus { get; init; }
}