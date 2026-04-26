using ToDoAI.Application.UseCases.ResetPassword.Models;

namespace ToDoAI.Application.UseCases.ResetPassword;

public interface IResetPasswordUseCase
{
    Task<ResetPasswordResult> ResetPassword(ResetPasswordBlRequest request, CancellationToken cancellationToken);
}
