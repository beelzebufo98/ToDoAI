using ToDoAI.Application.Abstractions.DalProviders.EmailConfirmationDalProvider.Models;

namespace ToDoAI.Application.Abstractions.DalProviders.EmailConfirmationDalProvider;

public interface IEmailConfirmationDalProvider
{
    Task ReplaceEmailConfirmation(EmailConfirmationRequestDal request, CancellationToken cancellationToken);
    Task<EmailConfirmationDal?> GetEmailConfirmation(Guid userId, string codeHash, CancellationToken cancellationToken);
    Task DeleteEmailConfirmations(Guid userId, CancellationToken cancellationToken);
}
