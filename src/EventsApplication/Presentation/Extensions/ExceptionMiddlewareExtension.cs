using Presentation.Middleware;

namespace Presentation.Extensions;

public static class ExceptionMiddlewareExtension
{
    public static IApplicationBuilder AddExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}