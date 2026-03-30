using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ToDoAI.ToDoAI.API.Controllers.TaskController.Models;
using ToDoAI.ToDoAI.Domain.Enums;

namespace ToDoAI.ToDoAI.API.Controllers.TaskController;


[ApiController]
[EnableCors]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/task/")]
public sealed class BaseTaskController : ControllerBase
{
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest,
        Type = typeof(ToDoAiControllerBase.ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> CreateTask([FromBody] CreateTaskRequest  request, CancellationToken cancellationToken )
    {
        return Ok();
    }
}