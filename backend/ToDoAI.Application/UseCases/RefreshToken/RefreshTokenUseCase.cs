using ToDoAI.ToDoAI.Application.UseCases.RefreshToken.Models;
using ToDoAI.ToDoAI.Infrastructure.DalProviders.RefreshTokenDalProvider;
using ToDoAI.ToDoAI.Infrastructure.DalProviders.RefreshTokenDalProvider.Models;

namespace ToDoAI.ToDoAI.Application.UseCases.RefreshToken;

public sealed class RefreshTokenUseCase : IRefreshTokenUseCase
{
    private readonly IRefreshTokenDalProvider  _refreshTokenDalProvider;

    public RefreshTokenUseCase(IRefreshTokenDalProvider refreshTokenDalProvider)
    {
        _refreshTokenDalProvider = refreshTokenDalProvider;
    }
    public async Task CreateRefreshToken(RefreshTokenBlRequest refreshToken,
        CancellationToken cancellationToken)
    {
        var tokenDal = new RefreshTokenRequestDal
        {
            UserId = refreshToken.UserId,
            RefreshTokenHash = refreshToken.RefreshTokenHash,
            ExpiresAt = refreshToken.ExpiresAt,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        await _refreshTokenDalProvider.CreateRefreshToken(tokenDal, cancellationToken);
    }

    public async Task<RefreshTokenBlResult?> GetRefreshToken(string refreshTokenHash, CancellationToken cancellationToken)
    {
        var result = await _refreshTokenDalProvider.GetRefreshToken(refreshTokenHash, cancellationToken);

        if (result is null)
        {
            return null;
        }

        return new RefreshTokenBlResult
        {
            UserId = result.UserId,
            RefreshTokenHash = result.RefreshTokenHash,
            ExpiresAt = result.ExpiresAt,
            CreatedAt = result.CreatedAt
        };
    }

    public async Task DeleteRefreshToken(string refreshTokenHash, CancellationToken cancellationToken)
    {
        await _refreshTokenDalProvider.DeleteRefreshToken(refreshTokenHash, cancellationToken);
    }
}