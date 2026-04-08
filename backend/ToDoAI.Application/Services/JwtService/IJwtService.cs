using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider.Models;

namespace ToDoAI.Application.Services.JwtService;

public interface IJwtService
{
    string GenerateAccessToken(LoginUserDal account);

    string GenerateRefreshToken(LoginUserDal account);

    string HashRefreshToken(string token);
    
    Guid GetUserIdFromToken(string token);
}
