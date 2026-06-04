using Application.Common.DTOs;
using Domain.Exceptions;
using FluentValidation;

namespace EventApplication.Middleware;

public class GlobalExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleException(httpContext, ex);
        }
    }

    private async Task HandleException(HttpContext httpContext, System.Exception ex)
    {
        if (httpContext.Response.HasStarted)
        {
            return;
        }

        var statusCode = MapStatusCode(ex);
        var errorResponse = CreateErrorResponse(ex);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(errorResponse);
    }

    private static int MapStatusCode(System.Exception ex)
        => ex switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            EventNotFoundException => StatusCodes.Status404NotFound,
            BookingNotFoundException => StatusCodes.Status404NotFound,
            NoAvailableSeatsException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };   
    private static ErrorResponse CreateErrorResponse(System.Exception ex)
    {
        var errorResponse = new ErrorResponse
        {
            Message = ex.Message
        };

        if (ex is ValidationException validationException)
        {
            errorResponse.Errors = validationException.Errors
                .Select(error => error.ErrorMessage)
                .ToList();
        }

        return errorResponse;
    }
}