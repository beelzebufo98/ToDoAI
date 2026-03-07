using Microsoft.EntityFrameworkCore;
using ToDoAI.ToDoAI.Domain.Entities;
using ToDoAI.ToDoAI.Infrastructure.DalProviders.UserDalProvider.Models;
using ToDoAI.ToDoAI.Infrastructure.Data;

namespace ToDoAI.ToDoAI.Infrastructure.DalProviders.UserDalProvider;

public sealed class UserDalProvider : IUserDalProvider
{
    private readonly IDbContextFactory<ToDoAIDbContext> _dbContextFactory;

    public UserDalProvider(IDbContextFactory<ToDoAIDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<bool> CheckUserExists(string userName, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var user = await toDoAiDb.Users.FirstOrDefaultAsync(cancellationToken);
        if (user is null)
        {
            return false;
        }
        return true;
    }

    public async Task CreateUser(RegisterUserRequestDal userRequest, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var userEntity = new UserEntity
        {
            Id = Guid.NewGuid(),
            UserName = userRequest.UserName,
            FirstName = userRequest.FirstName,
            LastName = userRequest.LastName,
            PasswordHash = userRequest.PasswordHash,
        };
        
        await toDoAiDb.AddRangeAsync(userEntity,  cancellationToken);
    }

    //TODO: доработать под дал модель
    public async Task<UserEntity?> GetUser(string userName, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var user = await toDoAiDb.Users.FirstOrDefaultAsync(cancellationToken);
        if (user is null)
        {
            return null;
        }
        return user;
    }
}