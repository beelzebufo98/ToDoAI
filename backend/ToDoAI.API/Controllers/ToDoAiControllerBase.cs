using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace ToDoAI.ToDoAI.API.Controllers;

public abstract class ToDoAiControllerBase : ControllerBase
{
    public ActionResult Ok<TResult>(TResult result) where TResult : class
    {
        return (ActionResult) this.Ok((object) new PayloadApiResponse<TResult>(result));
    }

    public ActionResult ClientError<TErrorCode>(ErrorApi<TErrorCode> error, int statusCode = 400)
    {
        return (ActionResult) this.StatusCode(statusCode, (object) new ClientErrorApiResponse<TErrorCode>(error)); 
    }
    
    public ActionResult ServerError() => (ActionResult) this.StatusCode(500, (object) null);

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