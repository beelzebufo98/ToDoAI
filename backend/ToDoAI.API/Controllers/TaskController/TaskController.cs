using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoAI.API.Controllers.TaskController.Mappers;
using ToDoAI.API.Controllers.TaskController.Models;
using ToDoAI.Application.UseCases.CreateTask;
using ToDoAI.Application.UseCases.CreateTask.Models;
using ToDoAI.Application.UseCases.DeleteTask;
using ToDoAI.Application.UseCases.GetTask;
using ToDoAI.Application.UseCases.GetTask.Models;
using ToDoAI.Application.UseCases.UpdateTask;
using ToDoAI.Application.UseCases.UpdateTask.Models;
using ToDoAI.Application.UseCases.UpdateTaskStatus;
using ToDoAI.Application.UseCases.UpdateTaskStatus.Models;
using ToDoAI.Domain.Enums;
using ToDoAI.ToDoAI.API.Controllers.TaskController.Models;

namespace ToDoAI.API.Controllers.TaskController;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/task/")]
public sealed class TaskController : ToDoAiControllerBase
{
    private readonly ICreateTaskUseCase _createTaskUseCase;
    private readonly IGetTaskUseCase _getTaskUseCase;
    private readonly IDeleteTaskUseCase _deleteTaskUseCase;
    private readonly IUpdateTaskStatusUseCase _updateTaskStatusUseCase;
    private readonly IUpdateTaskUseCase _updateTaskUseCase;

    public TaskController(
        ICreateTaskUseCase createTaskUseCase, 
        IGetTaskUseCase getTaskUseCase,
        IDeleteTaskUseCase deleteTaskUseCase,
        IUpdateTaskStatusUseCase updateTaskStatusUseCase,
        IUpdateTaskUseCase updateTaskUseCase)
    {
        _createTaskUseCase = createTaskUseCase;
        _getTaskUseCase = getTaskUseCase;
        _deleteTaskUseCase = deleteTaskUseCase;
        _updateTaskStatusUseCase = updateTaskStatusUseCase;
        _updateTaskUseCase = updateTaskUseCase;
    }
    
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> CreateTask([FromBody] CreateTaskRequest  request, CancellationToken cancellationToken)
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
            DeadlineAt = request.DeadlineAt,
        };
        
        var result = await _createTaskUseCase.CreateTask(taskRequest, cancellationToken);

        if (result.ErrorCode is not null)
        {
            return ClientError(new ErrorApi<ErrorCodes?>(result.ErrorCode));
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

    [HttpGet("{taskId}/get")]
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
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.TaskNotFound));
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
        return Ok(taskResponse);
    }

    [HttpDelete("{taskId}/decline")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> DeleteTask([FromRoute] string taskId, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }
        
        if (!Guid.TryParse(taskId, out var taskIdReq))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.TaskNotFound));
        }
        
        var result  = await _deleteTaskUseCase.DeleteTask(taskIdReq, userId, cancellationToken);
        if (result.ErrorCode is not null)
        {
            var statusCode = result.ErrorCode == ErrorCodes.TaskNotFound
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;
            return ClientError(new ErrorApi<ErrorCodes?>(result.ErrorCode), statusCode);
        }

        return NoContent();
    }

    [HttpPatch("{taskId}/update")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetTaskResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> UpdateTask([FromRoute] string taskId, [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }
        
        if (!Guid.TryParse(taskId, out var taskIdReq))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.TaskNotFound));
        }

        var blRequest = new UpdateTaskBlRequest
        {
            UserId = userId,
            TaskId = taskIdReq,
            Title = request.Title,
            Description = request.Description,
            EstimatedMinutes = request.EstimatedMinutes,
            Priority = request.Priority,
            ComplexityLevel = request.ComplexityLevel,
            DeadlineAt = request.DeadlineAt
        };
        
        var result = await _updateTaskUseCase.UpdateTask(blRequest, cancellationToken);

        if (result.ErrorCode is not null)
        {
            var statusCode = result.ErrorCode == ErrorCodes.TaskNotFound
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;
            return ClientError(new ErrorApi<ErrorCodes?>(result.ErrorCode), statusCode);
        }
        
        var taskResponse = result.TaskResult.GetTask();
        return Ok(taskResponse);
    }

    [HttpPatch("{taskId}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> UpdateStatus([FromRoute] string taskId,[FromBody] TaskStatusRequest taskStatus, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }
        
        if (!Guid.TryParse(taskId, out var taskIdReq))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.TaskNotFound));
        }

        var workStatus = taskStatus.WorkStatus.GetTaskWorkStatusBlFilters();

        var request = new UpdateTaskStatusBlRequest
        {
            UserId = userId,
            TaskId = taskIdReq,
            TaskWorkStatus = workStatus,
        };
        
        var result = await _updateTaskStatusUseCase.UpdateTaskStatus(request, cancellationToken);

        if (result.ErrorCode is not null)
        {
            var statusCode = result.ErrorCode == ErrorCodes.TaskNotFound
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;
            return ClientError(new ErrorApi<ErrorCodes?>(result.ErrorCode), statusCode);
        }
        
        return Ok(result.WorkStatus.ToString());
    }
    
}
