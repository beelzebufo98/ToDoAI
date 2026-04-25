using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider.Models;

namespace ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;

public interface IUserDalProvider
{
    Task<bool> CheckUserExists(string userName, CancellationToken cancellationToken);
    Task<bool> CheckUserEmailExists(string email, CancellationToken cancellationToken);
    Task CreateUser(RegisterUserRequestDal userRequest, CancellationToken cancellationToken);
    Task<LoginUserDal?> GetUser(string userName, CancellationToken cancellationToken);
    Task<LoginUserDal?> GetUser(Guid userId, CancellationToken cancellationToken);
    Task<LoginUserDal?> GetUserByEmail(string email, CancellationToken cancellationToken);
    Task UpdatePassword(Guid userId, string passwordHash, CancellationToken cancellationToken);
    Task UpdateEmailConfirmed(Guid userId, bool isEmailConfirmed, CancellationToken cancellationToken);
}
