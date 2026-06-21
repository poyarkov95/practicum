namespace Application.Abstractions.Persistence.Repositories;

public interface IUserRepository
{
    /// <summary>
    /// Получить пользователя по логину
    /// </summary>
    Task<Domain.Entities.User?> GetUserByLogin(string login);
    
    /// <summary>
    /// Добавить пользователя в базу данных
    /// </summary>
    Task AddUserAsync(Domain.Entities.User user);
    
    /// <summary>
    /// Получить пользователя по идентификатору
    /// </summary>
    Task<Domain.Entities.User> GetUser(Guid userId);
}