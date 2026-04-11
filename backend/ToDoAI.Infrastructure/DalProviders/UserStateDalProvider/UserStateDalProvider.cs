using Microsoft.EntityFrameworkCore;
using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider.Models;
using ToDoAI.Infrastructure.DalProviders.UserStateDalProvider.Mappers;
using ToDoAI.Infrastructure.Data;

namespace ToDoAI.Infrastructure.DalProviders.UserStateDalProvider;

public sealed class UserStateDalProvider : IUserStateDalProvider
{
    private readonly IDbContextFactory<ToDoAIDbContext>  _dbContextFactory;

    public UserStateDalProvider(IDbContextFactory<ToDoAIDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<UserStateDal> CreateUserState(UserStateDalRequest request, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var userStateEntity = request.ToUserStateEntity();
        
        await dbContext.States.AddAsync(userStateEntity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        var result = userStateEntity.ToUserStateDal();
        return result;
    }
}