using ToDoAI.Application.UseCases.ConfirmEmail.Models;

namespace ToDoAI.Application.UseCases.ConfirmEmail;

public interface IConfirmEmailUseCase
{
    Task<ConfirmEmailResult> ConfirmEmail(ConfirmEmailBlRequest request, CancellationToken cancellationToken);
}
