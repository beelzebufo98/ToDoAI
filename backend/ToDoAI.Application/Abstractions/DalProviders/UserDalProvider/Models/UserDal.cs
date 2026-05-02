namespace ToDoAI.Application.Abstractions.DalProviders.UserDalProvider.Models;

public sealed record UserDal
{
    public required Guid UserId { get; init; }
    
    public required string UserName { get; init; }
    
    public required string FirstName { get; init; }
    
    public string? LastName { get; init; }

    public string? Email { get; init; }

    public bool IsEmailConfirmed { get; init; }
    
    public required string PasswordHash { get; init; }
}
