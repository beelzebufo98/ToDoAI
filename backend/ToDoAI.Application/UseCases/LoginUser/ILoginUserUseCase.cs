using ToDoAI.Application.UseCases.LoginUser.Models;

namespace ToDoAI.Application.UseCases.LoginUser;

public interface ILoginUserUseCase
{
    Task<LoginUserResult> LoginUser(LoginUserBlRequest request, CancellationToken cancellationToken);
}