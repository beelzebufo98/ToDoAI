using Microsoft.EntityFrameworkCore;
using ToDoAI.Application.Abstractions.DalProviders.PasswordResetDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.PasswordResetDalProvider.Models;
using ToDoAI.Domain.Entities;
using ToDoAI.Infrastructure.Data;

namespace ToDoAI.Infrastructure.DalProviders.PasswordResetDalProvider;

public sealed class PasswordResetDalProvider : IPasswordResetDalProvider
{
    private readonly IDbContextFactory<ToDoAIDbContext> _dbContextFactory;

    public PasswordResetDalProvider(IDbContextFactory<ToDoAIDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task ReplacePasswordReset(PasswordResetRequestDal request, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var existingPasswordResets = await toDoAiDb.PasswordResets
            .Where(x => x.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        if (existingPasswordResets.Count > 0)
        {
            toDoAiDb.PasswordResets.RemoveRange(existingPasswordResets);
        }

        await toDoAiDb.PasswordResets.AddAsync(new PasswordResetEntity
        {
            Id = request.Id,
            UserId = request.UserId,
            CodeHash = request.CodeHash,
            ExpiresAt = request.ExpiresAt,
            SentAt = request.SentAt,
            Attempts = request.Attempts
        }, cancellationToken);

        await toDoAiDb.SaveChangesAsync(cancellationToken);
    }

    public async Task<PasswordResetDal?> GetPasswordReset(Guid userId, string codeHash, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var passwordReset = await toDoAiDb.PasswordResets
            .FirstOrDefaultAsync(x => x.UserId == userId && x.CodeHash == codeHash, cancellationToken);

        if (passwordReset is null)
        {
            return null;
        }

        return new PasswordResetDal
        {
            Id = passwordReset.Id,
            UserId = passwordReset.UserId,
            CodeHash = passwordReset.CodeHash,
            ExpiresAt = passwordReset.ExpiresAt,
            SentAt = passwordReset.SentAt,
            Attempts = passwordReset.Attempts
        };
    }

    public async Task DeletePasswordResets(Guid userId, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var passwordResets = await toDoAiDb.PasswordResets
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        if (passwordResets.Count == 0)
        {
            return;
        }

        toDoAiDb.PasswordResets.RemoveRange(passwordResets);
        await toDoAiDb.SaveChangesAsync(cancellationToken);
    }
}
