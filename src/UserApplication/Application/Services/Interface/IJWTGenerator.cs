using Application.User;

namespace Application.Services.Interface;

public interface IJWTGenerator
{
    /// <summary>
    /// Метод генерации токена
    /// </summary>
    string GenerateToken(LoginRequestDto request);
}