using Application.Abstractions.Services.Interface;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class ServiceRegistration
{
    public static void AddServices(this IServiceCollection services)
    {
        // Регистрация репозиториев
        services.AddScoped<ICacheService, RedisCacheService>();
    }
}