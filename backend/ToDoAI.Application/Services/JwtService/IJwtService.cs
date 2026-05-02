using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider.Models;

namespace ToDoAI.Application.Services.JwtService;

public interface IJwtService
{
    string GenerateAccessToken(UserDal account);

    string GenerateRefreshToken(UserDal account);

    string HashRefreshToken(string token);
    
    Guid GetUserIdFromToken(string token);
}
