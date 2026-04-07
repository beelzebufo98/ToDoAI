using ToDoAI.Application.UseCases.GetTask.Models;
using ToDoAI.Application.UseCases.UpdateTask.Models;

namespace ToDoAI.Application.UseCases.UpdateTask;

public interface IUpdateTaskUseCase
{
    Task<GetTaskResult?> UpdateTask(UpdateTaskBlRequest request, CancellationToken cancellationToken);
}