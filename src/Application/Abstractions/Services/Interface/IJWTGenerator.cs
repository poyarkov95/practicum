using Application.User;

namespace Application.Abstractions.Services.Interface;

public interface IJWTGenerator
{
    /// <summary>
    /// Метод генерации токена
    /// </summary>
    string GenerateToken(LoginRequestDto request);
}