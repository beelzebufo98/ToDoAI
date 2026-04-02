using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

    public string GenerateAccessToken(LoginUserDal account)
    {
        var claims = new List<Claim>
        {
            new Claim("userName", account.UserName),
            new Claim("firstName", account.FirstName),
            new Claim("id", account.UserId.ToString())
        };
        TimeSpan tokenTtl = TimeSpan.Parse("00:01:00");
        if (TimeSpan.TryParse(_settings.Value.AccessTokenLifetime, out var tokenLifetime))
        {
            tokenTtl = tokenLifetime;
        }
        var jwtToken = new JwtSecurityToken(
            expires:  DateTime.UtcNow.Add(tokenTtl),
            claims: claims,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Value.SecretKey)),SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }

    public string GenerateRefreshToken(LoginUserDal account)
    {
        var claims = new List<Claim>
        {
            new Claim("id", account.UserId.ToString())
        };

        TimeSpan tokenTtl = TimeSpan.Parse("7.00:00:00");
        if (TimeSpan.TryParse(_settings.Value.RefreshTokenLifetime, out var refreshTokenLifetime))
        {
            tokenTtl = refreshTokenLifetime;
        }
        
        var jwtToken =  new JwtSecurityToken(
            expires:  DateTime.UtcNow.Add(tokenTtl),
            claims: claims,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Value.SecretKey)),SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }
    
    public string HashRefreshToken(string token)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(token));
            
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    public Guid GetUserIdFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Value.SecretKey))
        }, out _);

        var userIdClaim = principal.FindFirst("id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new SecurityTokenException("Token does not contain a valid user id.");
        }

        return userId;
    }
}