namespace ToDoAI.Application.UseCases.UserUseCase.Models;

public sealed record UserResult
{
    public required Guid UserId { get; set; }
    
    public required string UserName { get; set; }
    
    public required string FirstName { get; set; }
    
    public string? LastName { get; set; }
    
    public string? Email { get; set; }
}
