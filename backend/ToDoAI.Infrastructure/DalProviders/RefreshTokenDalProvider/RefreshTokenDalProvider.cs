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

    public async Task<RefreshTokenDal> CreateRefreshToken(RefreshTokenRequestDal refreshToken, CancellationToken cancellationToken)
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

        return new RefreshTokenDal
        {
            Id = refreshSession.Id,
            UserId = refreshSession.UserId,
            RefreshTokenHash = refreshSession.TokenHash,
            ExpiresAt = refreshSession.ExpiresAt,
            CreatedAt = refreshSession.CreatedAt
        };
    }
}
