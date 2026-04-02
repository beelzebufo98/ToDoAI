using ToDoAI.ToDoAI.Infrastructure.DalProviders.RefreshTokenDalProvider.Models;

namespace ToDoAI.ToDoAI.Infrastructure.DalProviders.RefreshTokenDalProvider;

public interface IRefreshTokenDalProvider
{
    Task CreateRefreshToken(RefreshTokenRequestDal refreshToken, CancellationToken cancellationToken);
    Task<RefreshTokenDal?> GetRefreshToken(string refreshTokenHash, CancellationToken cancellationToken);
    Task DeleteRefreshToken(string refreshTokenHash, CancellationToken cancellationToken);
}
