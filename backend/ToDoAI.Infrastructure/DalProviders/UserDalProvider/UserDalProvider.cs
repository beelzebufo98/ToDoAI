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
        var user = await toDoAiDb.Users.Where(x=> String.Equals(x.UserName == userName, StringComparer.CurrentCultureIgnoreCase)).FirstOrDefaultAsync(cancellationToken);
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
            Id = userRequest.Id,
            UserName = userRequest.UserName,
            FirstName = userRequest.FirstName,
            LastName = userRequest.LastName,
            PasswordHash = userRequest.PasswordHash,
        };
        
        await toDoAiDb.AddAsync(userEntity,  cancellationToken);
    }
    
    public async Task<LoginUserDal?> GetUser(string userName, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var user = await toDoAiDb.Users.Where(x=> String.Equals(x.UserName == userName, StringComparer.CurrentCultureIgnoreCase)).FirstOrDefaultAsync(cancellationToken);
        if (user is null)
        {
            return null;
        }
        return GetLoginUserDal(user);
    }
    
    private static LoginUserDal GetLoginUserDal(UserEntity user)
    {
        return new LoginUserDal
        {
            UserId = user.Id,
            UserName = user.UserName,
            FirstName = user.FirstName,
            PasswordHash = user.PasswordHash,
        };
    }
}