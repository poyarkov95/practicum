using Application.Abstractions.Persistence.Repositories;
using Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class RepositoryRegistration
{
    public static void AddRepositories(this IServiceCollection services)
    {
        // Регистрация репозиториев
        services.AddScoped<IEventRepository, EventRepository>();
    }
}