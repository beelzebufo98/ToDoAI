namespace ToDoAI.ToDoAI.Infrastructure.DalProviders.UserDalProvider.Models;

public sealed record RegisterUserRequestDal
{
    public required string UserName { get; init; }
    public required string FirstName { get; init; }
    public string? LastName { get; init; }
    
    public required string PasswordHash { get; init; }
}