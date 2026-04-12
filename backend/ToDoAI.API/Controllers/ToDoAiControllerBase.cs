using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace ToDoAI.API.Controllers;

public abstract class ToDoAiControllerBase : ControllerBase
{
    [NonAction]
    protected ActionResult Ok<TResult>(TResult result) where TResult : class
    {
        return base.Ok(new PayloadApiResponse<TResult>(result));
    }

    [NonAction]
    protected ActionResult ClientError<TErrorCode>(ErrorApi<TErrorCode> error, int statusCode = 400)
    {
        return base.StatusCode(statusCode, new ClientErrorApiResponse<TErrorCode>(error));
    }
    
    [NonAction]
    protected ActionResult ServerError() => base.StatusCode(500);

    public class PayloadApiResponse<T> where T : class
    {
        [Required]
        public T Payload { get; set; }

        public PayloadApiResponse(T payload)
        {
            this.Payload = payload;
        }
    }

    public class ErrorApi<TErrorCode>
    {
        public ErrorApi(TErrorCode code)
        {
            this.Code = code;
        }

        public ErrorApi(TErrorCode code, Dictionary<string, string> errors)
        {
            this.Code = code;
            this.Details = errors;
        }
        
        public TErrorCode Code { get; set; }
        
        public Dictionary<string, string>? Details { get; set; }
    }

    public sealed record ClientErrorApiResponse<TErrorCode>(ErrorApi<TErrorCode> Error);
}
