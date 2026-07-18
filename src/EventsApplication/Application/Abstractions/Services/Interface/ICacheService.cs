namespace Application.Abstractions.Services.Interface;

public interface ICacheService
{
    /// <summary>
    /// Получить объект из кэша
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    
    /// <summary>
    /// Сохранить значение в кэш
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default);
    
    /// <summary>
    /// Удалить значение из кэша 
    /// </summary>
    Task RemoveAsync(string key, CancellationToken ct = default);
}