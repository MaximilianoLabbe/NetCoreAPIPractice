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

            BusinessValidationException validationException =>
                CreateValidationProblemDetails(
                    httpContext,
                    validationException),

            _ => CreateInternalServerErrorProblemDetails(
                httpContext)
        };

        LogException(
            exception,
            problemDetails.Status);

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
        var problemDetails = CreateProblemDetails(
            httpContext,
            StatusCodes.Status409Conflict,
            "Resource conflict",
            exception.Message);

        problemDetails.Extensions["resource"] = exception.Resource;
        problemDetails.Extensions["field"] = exception.Field;
        problemDetails.Extensions["value"] = exception.Value;

        return problemDetails;
    }

    private static ProblemDetails CreateNotFoundProblemDetails(
      HttpContext httpContext,
      ResourceNotFoundException exception)
    {
        var problemDetails = CreateProblemDetails(
            httpContext,
            StatusCodes.Status404NotFound,
            "Resource not found",
            exception.Message);

        problemDetails.Extensions["resource"] = exception.Resource;
        problemDetails.Extensions["key"] = exception.Key;

        return problemDetails;
    }

    private static ProblemDetails CreateInternalServerErrorProblemDetails(
     HttpContext httpContext)
    {
        return CreateProblemDetails(
            httpContext,
            StatusCodes.Status500InternalServerError,
            "Internal server error",
            "An unexpected error occurred while processing the request.");
    }

    private void LogException(
    Exception exception,
    int? statusCode)
    {
        switch (statusCode)
        {
            case StatusCodes.Status400BadRequest:
                _logger.LogInformation(
                    exception,
                    "Validation error. ExceptionType: {ExceptionType}",
                    exception.GetType().Name);
                break;
            case StatusCodes.Status404NotFound:
                _logger.LogInformation(
                    exception,
                    "Resource not found. ExceptionType: {ExceptionType}",
                    exception.GetType().Name);
                break;

            case StatusCodes.Status409Conflict:
                _logger.LogWarning(
                    exception,
                    "Resource conflict. ExceptionType: {ExceptionType}",
                    exception.GetType().Name);
                break;

            default:
                _logger.LogError(
                    exception,
                    "Unhandled exception. ExceptionType: {ExceptionType}",
                    exception.GetType().Name);
                break;
        }
    }

    private static ProblemDetails CreateProblemDetails(
    HttpContext httpContext,
    int status,
    string title,
    string detail)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
    }

    private static ProblemDetails CreateValidationProblemDetails(
    HttpContext httpContext,
    BusinessValidationException exception)
    {
        var problemDetails = CreateProblemDetails(
            httpContext,
            StatusCodes.Status400BadRequest,
            "Validation error",
            exception.Message);

        problemDetails.Extensions["field"] = exception.Field;
        problemDetails.Extensions["value"] = exception.Value;

        return problemDetails;
    }
}