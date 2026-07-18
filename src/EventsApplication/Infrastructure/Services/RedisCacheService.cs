using System.Text.Json;
using Application.Abstractions.Services.Interface;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Infrastructure.Services;

public class RedisCacheService(IDatabase database, ILogger<RedisCacheService> logger) : ICacheService
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var value = await database.StringGetAsync(key);
            return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
        }
        catch (RedisConnectionException ex)
        {
            logger.LogError(ex, "Redis connection error while getting key {Key}", key);
            return default;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting key {Key} from Redis", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            await database.StringSetAsync(key, serializedValue, ttl);
        }
        catch (RedisConnectionException ex)
        {
            logger.LogError(ex, "Redis connection error while setting key {Key}", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting key {Key} in Redis", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await database.KeyDeleteAsync(key);
        }
        catch (RedisConnectionException ex)
        {
            logger.LogError(ex, "Redis connection error while removing key {Key}", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing key {Key} from Redis", key);
        }
    }
}