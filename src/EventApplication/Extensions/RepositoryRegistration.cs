using Application.Abstractions.Persistence.Repositories;
using Infrastructure.Persistence.Repositories;

namespace EventApplication.Extensions;

public static class RepositoryRegistration
{
    public static void AddRepositories(this IServiceCollection services)
    {
        // Регистрация репозиториев
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
    }
}