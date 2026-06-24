using Domain.Entities;

namespace Application.User;

public class LoginRequestDto
{
    /// <summary>
    /// Логин пользователя
    /// </summary>
    public string Login { get; set; }
    
    /// <summary>
    /// Пароль
    /// </summary>
    public string Password { get; set; }
    
    /// <summary>
    /// Идентификатор пользователя для Claims
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Роль пользователя
    /// </summary>
    public UserRole Role { get; set; } = UserRole.User;
}