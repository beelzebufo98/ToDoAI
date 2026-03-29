using ToDoAI.ToDoAI.Infrastructure.DalProviders.UserDalProvider.Models;

namespace ToDoAI.ToDoAI.Application.Services.JwtService;

public interface IJwtService
{
    string GenerateToken(LoginUserDal account);
}