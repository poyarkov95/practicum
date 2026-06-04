using Application.Abstractions.Services;
using Infrastructure.Services;
using Infrastructure.Services.Hosted;

namespace EventApplication.Extensions;

/// <summary>
/// Регистрация сервисов через расширение
/// </summary>
public static class ServiceRegistration
{
    public static void AddServices(this IServiceCollection services)
    {
        // Регистрация сервисов
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        
        // Hosted сервисы
        services.AddHostedService<BookingWorker>();
    }
}