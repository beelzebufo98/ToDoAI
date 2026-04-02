using Microsoft.EntityFrameworkCore;
using ToDoAI.ToDoAI.Domain.Entities;
using ToDoAI.ToDoAI.Infrastructure.DalProviders.RefreshTokenDalProvider.Models;
using ToDoAI.ToDoAI.Infrastructure.Data;

namespace ToDoAI.ToDoAI.Infrastructure.DalProviders.RefreshTokenDalProvider;

public sealed class RefreshTokenDalProvider : IRefreshTokenDalProvider
{
    private readonly IDbContextFactory<ToDoAIDbContext> _dbContextFactory;

    public RefreshTokenDalProvider(IDbContextFactory<ToDoAIDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task CreateRefreshToken(RefreshTokenRequestDal refreshToken, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var refreshSession = new RefreshSessionEntity
        {
            Id = Guid.NewGuid(),
            UserId = refreshToken.UserId,
            TokenHash = refreshToken.RefreshTokenHash,
            ExpiresAt = refreshToken.ExpiresAt,
            CreatedAt = refreshToken.CreatedAt
        };

        await toDoAiDb.RefreshSessions.AddAsync(refreshSession, cancellationToken);
        await toDoAiDb.SaveChangesAsync(cancellationToken);
    }

    public async Task<RefreshTokenDal?> GetRefreshToken(string refreshTokenHash, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var refreshSession = await toDoAiDb.RefreshSessions
            .FirstOrDefaultAsync(x => x.TokenHash == refreshTokenHash, cancellationToken);

        if (refreshSession is null)
        {
            return null;
        }

        return new RefreshTokenDal
        {
            UserId = refreshSession.UserId,
            RefreshTokenHash = refreshSession.TokenHash,
            ExpiresAt = refreshSession.ExpiresAt,
            CreatedAt = refreshSession.CreatedAt
        };
    }

    public async Task DeleteRefreshToken(string refreshTokenHash, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var refreshSession = await toDoAiDb.RefreshSessions
            .FirstOrDefaultAsync(x => x.TokenHash == refreshTokenHash, cancellationToken);

        if (refreshSession is null)
        {
            return;
        }

        toDoAiDb.RefreshSessions.Remove(refreshSession);
        await toDoAiDb.SaveChangesAsync(cancellationToken);
    }
}
