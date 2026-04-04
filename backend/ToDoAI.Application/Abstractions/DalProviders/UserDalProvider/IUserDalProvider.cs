using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider.Models;

namespace ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;

public interface IUserDalProvider
{
    Task<bool> CheckUserExists(string userName, CancellationToken cancellationToken);
    Task CreateUser(RegisterUserRequestDal userRequest, CancellationToken cancellationToken);
    Task<LoginUserDal?> GetUser(string userName, CancellationToken cancellationToken);
    Task<LoginUserDal?> GetUser(Guid userId, CancellationToken cancellationToken);
}
