using Application.Common.DTOs;
using Domain.Exceptions;
using FluentValidation;

namespace Presentation.Middleware;

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

    private async Task HandleException(HttpContext httpContext, Exception ex)
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

    private static int MapStatusCode(Exception ex)
        => ex switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            BookingNotFoundException => StatusCodes.Status404NotFound,
            BookingLimitExceededException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };   
    private static ErrorResponse CreateErrorResponse(Exception ex)
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