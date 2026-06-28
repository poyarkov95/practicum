using Application.Abstractions.Services.Interface;
using Infrastructure.Services.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class ServiceRegistration
{
    public static void AddServices(this IServiceCollection services)
    {
        // Регистрация репозиториев
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJWTGenerator, JWTGenerator>();
        services.AddScoped<IPasswordHashGenerator, PasswordHashGenerator>();
    }
}