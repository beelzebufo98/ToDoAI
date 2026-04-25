using Microsoft.EntityFrameworkCore;
using ToDoAI.Application.Abstractions.DalProviders.EmailConfirmationDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.EmailConfirmationDalProvider.Models;
using ToDoAI.Domain.Entities;
using ToDoAI.Infrastructure.Data;

namespace ToDoAI.Infrastructure.DalProviders.EmailConfirmationDalProvider;

public sealed class EmailConfirmationDalProvider : IEmailConfirmationDalProvider
{
    private readonly IDbContextFactory<ToDoAIDbContext> _dbContextFactory;

    public EmailConfirmationDalProvider(IDbContextFactory<ToDoAIDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task ReplaceEmailConfirmation(EmailConfirmationRequestDal request, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var existingEmailConfirmations = await toDoAiDb.EmailConfirmations
            .Where(x => x.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        if (existingEmailConfirmations.Count > 0)
        {
            toDoAiDb.EmailConfirmations.RemoveRange(existingEmailConfirmations);
        }

        await toDoAiDb.EmailConfirmations.AddAsync(new EmailConfirmationEntity
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

    public async Task<EmailConfirmationDal?> GetEmailConfirmation(Guid userId, string codeHash, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var emailConfirmation = await toDoAiDb.EmailConfirmations
            .FirstOrDefaultAsync(x => x.UserId == userId && x.CodeHash == codeHash, cancellationToken);

        if (emailConfirmation is null)
        {
            return null;
        }

        return new EmailConfirmationDal
        {
            Id = emailConfirmation.Id,
            UserId = emailConfirmation.UserId,
            CodeHash = emailConfirmation.CodeHash,
            ExpiresAt = emailConfirmation.ExpiresAt,
            SentAt = emailConfirmation.SentAt,
            Attempts = emailConfirmation.Attempts
        };
    }

    public async Task DeleteEmailConfirmations(Guid userId, CancellationToken cancellationToken)
    {
        await using var toDoAiDb = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var emailConfirmations = await toDoAiDb.EmailConfirmations
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        if (emailConfirmations.Count == 0)
        {
            return;
        }

        toDoAiDb.EmailConfirmations.RemoveRange(emailConfirmations);
        await toDoAiDb.SaveChangesAsync(cancellationToken);
    }
}
