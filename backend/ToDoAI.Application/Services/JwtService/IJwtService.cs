using ToDoAI.ToDoAI.Infrastructure.DalProviders.UserDalProvider.Models;

namespace ToDoAI.ToDoAI.Application.Services.JwtService;

public interface IJwtService
{
    string GenerateAccessToken(LoginUserDal account);

    string GenerateRefreshToken();

    string HashRefreshToken(string token);
}