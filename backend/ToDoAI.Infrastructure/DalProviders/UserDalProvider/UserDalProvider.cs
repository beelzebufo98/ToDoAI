using Microsoft.EntityFrameworkCore;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider.Models;
using ToDoAI.Domain.Entities;
using ToDoAI.Infrastructure.Data;

namespace ToDoAI.Infrastructure.DalProviders.UserDalProvider;

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
        var normalizedUserName = userName.ToLowerInvariant();
        var user = await toDoAiDb.Users
            .Where(u => u.UserName.ToLower() == normalizedUserName)
            .FirstOrDefaultAsync(cancellationToken);
        if (user is null)
        {
            return false;
        }
        return true;
    }

    public async Task<bool> CheckUserEmailExists(string email, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var normalizedEmail = email.ToLowerInvariant();
        var user = await toDoAiDb.Users
            .Where(u => u.Email != null && u.Email.ToLower() == normalizedEmail)
            .FirstOrDefaultAsync(cancellationToken);

        return user is not null;
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
            Email = userRequest.Email,
            PasswordHash = userRequest.PasswordHash,
        };
        
        await toDoAiDb.AddAsync(userEntity,  cancellationToken);
        await toDoAiDb.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<UserDal?> GetUser(string userName, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var normalizedUserName = userName.ToLowerInvariant();
        var user = await toDoAiDb.Users
            .Where(u => u.UserName.ToLower() == normalizedUserName)
            .FirstOrDefaultAsync(cancellationToken);
        if (user is null)
        {
            return null;
        }
        return GetLoginUserDal(user);
    }

    public async Task<UserDal?> GetUser(Guid userId, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var user = await toDoAiDb.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        return GetLoginUserDal(user);
    }

    public async Task<UserDal?> GetUserByEmail(string email, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var normalizedEmail = email.ToLowerInvariant();
        var user = await toDoAiDb.Users
            .FirstOrDefaultAsync(x => x.Email != null && x.Email.ToLower() == normalizedEmail, cancellationToken);
        if (user is null)
        {
            return null;
        }

        return GetLoginUserDal(user);
    }

    public async Task UpdatePassword(Guid userId, string passwordHash, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var user = await toDoAiDb.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            return;
        }

        user.PasswordHash = passwordHash;
        await toDoAiDb.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateEmailConfirmed(Guid userId, bool isEmailConfirmed, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var user = await toDoAiDb.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            return;
        }

        user.IsEmailConfirmed = isEmailConfirmed;
        await toDoAiDb.SaveChangesAsync(cancellationToken);
    }
    
    private static UserDal GetLoginUserDal(UserEntity user)
    {
        return new UserDal
        {
            UserId = user.Id,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            IsEmailConfirmed = user.IsEmailConfirmed,
            PasswordHash = user.PasswordHash,
        };
    }
}
