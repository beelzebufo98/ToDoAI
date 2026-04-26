namespace ToDoAI.Application.UseCases.ResendConfirmationCode;

public interface IResendConfirmationCodeUseCase
{
    Task ResendConfirmationCode(string email, CancellationToken cancellationToken);
}