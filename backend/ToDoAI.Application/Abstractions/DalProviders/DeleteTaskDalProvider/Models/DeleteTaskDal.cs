namespace ToDoAI.Application.Abstractions.DalProviders.DeleteTaskDalProvider.Models;

public sealed record DeleteTaskDal
{
    public Guid TaskId { get; init; }
}