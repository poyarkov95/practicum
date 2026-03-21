using EventApplication.Exception;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace EventApplication.Middleware;

public class GlobalExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (System.Exception ex)
        {
            await HandleException(httpContext, ex);
        }
    }

    private async Task HandleException(HttpContext httpContext, System.Exception ex)
    {
        logger.LogError(
            ex,
            "Unhandled exception. Method={Method}, Path={Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);
            
        if (httpContext.Response.HasStarted)
        {
            return;
        }

        var statusCode = MapStatusCode(ex);
        
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var error = new ProblemDetails
        {
            Status = statusCode,
            Detail = ex.Message
        };

        await httpContext.Response.WriteAsJsonAsync(error);
    }

    private static int MapStatusCode(System.Exception ex)
        => ex switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            EventNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };   
    
}