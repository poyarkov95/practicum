using System.Security.Cryptography;
using System.Text;
using Application.Services.Interface;

namespace Infrastructure.Services.Implementation;

public class PasswordHashGenerator : IPasswordHashGenerator
{
    public string GenerateHash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes); 
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var inputBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        string inputHash = Convert.ToHexString(inputBytes);

        return string.Equals(inputHash, passwordHash, StringComparison.Ordinal);
    }
}