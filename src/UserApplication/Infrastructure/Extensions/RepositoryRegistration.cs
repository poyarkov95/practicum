using Application.Persistence.Repositories;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class RepositoryRegistration
{
    public static void AddRepositories(this IServiceCollection services)
    {
        // Регистрация репозиториев
        services.AddScoped<IUserRepository, UserRepository>();
    }
}