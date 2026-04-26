using System.Security.Cryptography;
using System.Text;

namespace ToDoAI.Application.Common;

public static class ConfirmationCodeHelper
{
    public static string Hash(string code)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static string Generate() =>
        RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
}