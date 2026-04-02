using ToDoAI.ToDoAI.Infrastructure.DalProviders.RefreshTokenDalProvider.Models;

namespace ToDoAI.ToDoAI.Infrastructure.DalProviders.RefreshTokenDalProvider;

public interface IRefreshTokenDalProvider
{
    Task<RefreshTokenDal> CreateRefreshToken(RefreshTokenRequestDal refreshToken, CancellationToken cancellationToken);
}
