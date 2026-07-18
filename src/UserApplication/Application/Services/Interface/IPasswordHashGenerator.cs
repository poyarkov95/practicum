namespace Application.Services.Interface;

public interface IPasswordHashGenerator
{
    /// <summary>
    /// Метод хэширования пароля
    /// </summary>
    string GenerateHash(string password);

    /// <summary>
    /// Метод проверки пароля
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);
}