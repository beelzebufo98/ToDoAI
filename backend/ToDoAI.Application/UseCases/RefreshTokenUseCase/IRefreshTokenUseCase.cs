using ToDoAI.ToDoAI.Application.UseCases.RefreshTokenUseCase.Models;

namespace ToDoAI.ToDoAI.Application.UseCases.RefreshTokenUseCase;

public interface IRefreshTokenUseCase
{
    Task CreateRefreshToken(RefreshTokenBlRequest refreshToken,
        CancellationToken cancellationToken);

    Task<RefreshTokenBlResult?> GetRefreshToken(string refreshTokenHash, CancellationToken cancellationToken);

    Task DeleteRefreshToken(string refreshTokenHash, CancellationToken cancellationToken);
}