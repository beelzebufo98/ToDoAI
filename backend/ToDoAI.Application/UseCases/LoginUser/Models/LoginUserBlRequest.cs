namespace ToDoAI.ToDoAI.Application.UseCases.LoginUser.Models;

public sealed record LoginUserBlRequest()
{
    public required string UserName { get; set; }
    public required  string Password { get; set; }
}