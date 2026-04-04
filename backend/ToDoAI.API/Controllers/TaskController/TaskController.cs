using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoAI.ToDoAI.API.Controllers.TaskController.Mappers;
using ToDoAI.ToDoAI.API.Controllers.TaskController.Models;
using ToDoAI.ToDoAI.Application.UseCases.CreateTask;
using ToDoAI.ToDoAI.Application.UseCases.CreateTask.Models;
using ToDoAI.ToDoAI.Application.UseCases.GetTask;
using ToDoAI.ToDoAI.Application.UseCases.GetTask.Models;
using ToDoAI.ToDoAI.Domain.Enums;

namespace ToDoAI.ToDoAI.API.Controllers.TaskController;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/task/")]
public sealed class TaskController : ToDoAiControllerBase
{
    private readonly ICreateTaskUseCase _createTaskUseCase;
    private readonly IGetTaskUseCase _getTaskUseCase;

    public TaskController(ICreateTaskUseCase createTaskUseCase, IGetTaskUseCase getTaskUseCase)
    {
        _createTaskUseCase = createTaskUseCase;
        _getTaskUseCase = getTaskUseCase;
    }
    
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
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

    [HttpGet("get")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetTasksResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> GetTasks([FromQuery] TaskFiltersRequest filters, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }

        var filtersBl = new TaskFiltersBlRequest
        {
            WorkStatus = filters.WorkStatus?.GetTaskWorkStatusBlFilters(),
            SortType = filters.SortType?.GetTaskTypeSortBl(),
            SortBy = filters.SortBy?.GetSortByBlFilters(),
            Page = filters.Page,
            PageSize = filters.PageSize,
        };
        
        var result = await _getTaskUseCase.GetTasks(userId, filtersBl, cancellationToken);
        var tasksResponse = result.TaskResults.Select(x => x.GetTask()).ToList();
        return Ok(new GetTasksResponse
        {
            Tasks = tasksResponse
        });
    }

    [HttpGet("get/{taskId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetTaskResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> GetTask([FromRoute] string taskId, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }
        
        if (!Guid.TryParse(taskId, out var taskIdReq))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.TaskNotFound), StatusCodes.Status400BadRequest);
        }
        
        
        var result = await _getTaskUseCase.GetTask(taskIdReq, userId, cancellationToken);

        if (result.ErrorCode is not null)
        {
            var statusCode = result.ErrorCode == ErrorCodes.TaskNotFound
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;
            return ClientError(new ErrorApi<ErrorCodes?>(result.ErrorCode), statusCode);
        }

        var taskResponse = result.TaskResult.GetTask();
        return Ok(new PayloadApiResponse<GetTaskResponse>(taskResponse));
    }
}
