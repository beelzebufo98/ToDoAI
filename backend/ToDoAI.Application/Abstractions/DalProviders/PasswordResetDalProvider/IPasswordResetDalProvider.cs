using ToDoAI.Application.Abstractions.DalProviders.PasswordResetDalProvider.Models;

namespace ToDoAI.Application.Abstractions.DalProviders.PasswordResetDalProvider;

public interface IPasswordResetDalProvider
{
    Task ReplacePasswordReset(PasswordResetRequestDal request, CancellationToken cancellationToken);
    Task<PasswordResetDal?> GetPasswordReset(Guid userId, CancellationToken cancellationToken);
    Task RegisterFailedAttempt(Guid userId, int maxAttempts, CancellationToken cancellationToken);
    Task DeletePasswordResets(Guid userId, CancellationToken cancellationToken);
}
