using ToDoAI.ToDoAI.Infrastructure.DalProviders.UserDalProvider.Models;

namespace ToDoAI.ToDoAI.Infrastructure.DalProviders.UserDalProvider;

public interface IUserDalProvider
{
    Task<bool> CheckUserExists(string userName, CancellationToken cancellationToken);
    Task CreateUser(RegisterUserRequestDal userRequest, CancellationToken cancellationToken);
    Task<LoginUserDal?> GetUser(string userName, CancellationToken cancellationToken);
}