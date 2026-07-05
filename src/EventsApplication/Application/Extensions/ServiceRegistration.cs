using Application.Abstractions.Services.Hosted;
using Application.Abstractions.Services.Implementation;
using Application.Abstractions.Services.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

/// <summary>
/// Регистрация сервисов через расширение
/// </summary>
public static class ServiceRegistration
{
    public static void AddServices(this IServiceCollection services)
    {
        // Регистрация сервисов
        services.AddScoped<IEventService, EventService>();
        
        // Hosted сервисы
        services.AddHostedService<EventWorker>();
    }
}