using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoAI.ToDoAI.API.Controllers.TaskController.Models;
using ToDoAI.ToDoAI.Application.UseCases.CreateTask;
using ToDoAI.ToDoAI.Application.UseCases.CreateTask.Models;
using ToDoAI.ToDoAI.Domain.Enums;

namespace ToDoAI.ToDoAI.API.Controllers.TaskController;


[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/task/")]
public sealed class BaseTaskController : ToDoAiControllerBase
{
    private readonly ICreateTaskUseCase _createTaskUseCase;

    public BaseTaskController(ICreateTaskUseCase createTaskUseCase)
    {
        _createTaskUseCase = createTaskUseCase;
    }
    
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest,
        Type = typeof(ToDoAiControllerBase.ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> CreateTask([FromBody] CreateTaskRequest  request, CancellationToken cancellationToken )
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }

        var taskRequest = new CreateTaskBlRequest
        {
            UserId = userId,
            Title = request.Title,
            Description = request.Description,
            EstimatedMinutes = request.EstimatedMinutes,
            Priority = request.Priority,
            ComplexityLevel = request.ComplexityLevel,
        };
        
        var result = await _createTaskUseCase.CreateTask(taskRequest, cancellationToken);

        if (result.ErrorCode is not null)
        {
            return  ClientError(new ErrorApi<ErrorCodes?>(result.ErrorCode));
        }
        return Ok(new CreateTaskResponse
        {
            TaskId = result.TaskId!.Value,
        });
    }
}
