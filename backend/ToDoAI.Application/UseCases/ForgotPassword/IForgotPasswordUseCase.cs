namespace ToDoAI.Application.UseCases.ForgotPassword;

public interface IForgotPasswordUseCase
{
    Task ForgotPassword(string email, CancellationToken cancellationToken);
}
