using API.Business.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace API.Presentation.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            DuplicateResourceException duplicateException =>
                CreateDuplicateProblemDetails(
                    httpContext,
                    duplicateException),

            ResourceNotFoundException notFoundException =>
                CreateNotFoundProblemDetails(
                    httpContext,
                    notFoundException),

            _ => null
        };

        if (problemDetails is null)
        {
            return false;
        }

        _logger.LogWarning(
            exception,
            "Handled application exception: {ExceptionType}",
            exception.GetType().Name);

        httpContext.Response.StatusCode =
            problemDetails.Status
            ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }

    private static ProblemDetails CreateDuplicateProblemDetails(
        HttpContext httpContext,
        DuplicateResourceException exception)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Resource conflict",
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["resource"] = exception.Resource;
        problemDetails.Extensions["field"] = exception.Field;
        problemDetails.Extensions["value"] = exception.Value;

        return problemDetails;
    }

    private static ProblemDetails CreateNotFoundProblemDetails(
        HttpContext httpContext,
        ResourceNotFoundException exception)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Resource not found",
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["resource"] = exception.Resource;
        problemDetails.Extensions["key"] = exception.Key;

        return problemDetails;
    }
}