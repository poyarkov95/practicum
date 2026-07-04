using Application.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationDependencyInjection
{
    /// <summary>
    /// Регистрация DbContext и других инфраструктурных сервисов
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddServices();
        return services;
    }
}