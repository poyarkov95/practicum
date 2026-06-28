using Application.User;

namespace Application.Abstractions.Services.Interface;

public interface IUserService
{
    /// <summary>
    /// Зарегистрировать пользователя
    /// </summary>
    Task<Guid> Register(LoginRequestDto  loginRequestDto);
    
    /// <summary>
    /// Сгенерировать токен для пользователя
    /// </summary>
    Task<string> Login(LoginRequestDto  loginRequestDto);

    /// <summary>
    /// Получить пользователя по идентификатору
    /// </summary>
    Task<Domain.Entities.User> GetUser(Guid userId);
}