using EventApplication.Database;
using EventApplication.Service.Hosted;
using EventApplication.Service.Implementation;
using EventApplication.Service.Interface;
using Microsoft.EntityFrameworkCore;

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