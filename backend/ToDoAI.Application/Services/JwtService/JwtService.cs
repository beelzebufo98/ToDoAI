using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ToDoAI.ToDoAI.Application.Services.JwtService.Settings;
using ToDoAI.ToDoAI.Infrastructure.DalProviders.UserDalProvider.Models;

namespace ToDoAI.ToDoAI.Application.Services.JwtService;

public sealed class JwtService : IJwtService
{
    
    private readonly IOptions<AuthSettings> _settings;
    public JwtService(IOptions<AuthSettings> settings)
    {
        _settings = settings;
    }

    public string GenerateToken(LoginUserDal account)
    {
        var claims = new List<Claim>
        {
            new Claim("userName", account.UserName),
            new Claim("firstName", account.FirstName),
            new Claim("id", account.UserId.ToString())
        };
        var jwtToken = new JwtSecurityToken(
            expires:  DateTime.UtcNow.Add(_settings.Value.TokenLifetime),
            claims: claims,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Value.SecretKey)),SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }
}