using EventApplication.Service.Implementation;
using EventApplication.Service.Interface;

namespace EventApplication.Extensions;

/// <summary>
/// Регистрация сервисов через расширение
/// </summary>
public static class ServiceRegistration
{
    public static void AddServices(this IServiceCollection services)
    {
        // Регистрация сервисов
        services.AddSingleton<IEventService, EventService>();
    }
}