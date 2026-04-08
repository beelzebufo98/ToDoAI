namespace ToDoAI.Application.Abstractions.DalProviders.UserDalProvider.Models;

public sealed record LoginUserDal
{
    public required Guid UserId { get; init; }
    
    public required string UserName { get; init; }
    
    public required string FirstName { get; init; }
    
    public required string PasswordHash { get; init; }
}
