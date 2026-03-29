using Microsoft.AspNetCore.Diagnostics;
using EventProject.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EventProject.Middlewares;

internal sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Unhandled exception. Method={Method}, Path={Path}, RequestId={RequestId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.Request.Headers["x-request-id"]);

        if (httpContext.Response.HasStarted)
        {
            logger.LogWarning(
                "Заголовки были отправлены клиенту, уже невозможно их поменять: {message}",
                exception.Message);

            return false;
        }

        var problemDetails = new ProblemDetails
        {
            Status = MapStatusCode(exception),
            Type = exception.GetType().Name,
            Title = "Произошла необработанная ошибка",
            Detail = exception.Message
        };

        await httpContext
            .Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static int MapStatusCode(Exception ex)
    {
        return ex switch
        {
            BadRequestException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}