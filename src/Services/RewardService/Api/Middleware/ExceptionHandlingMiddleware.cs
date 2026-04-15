using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySphere.RewardService.Api.Middleware;

/// <summary>
/// Global exception handling middleware.
/// Catches all unhandled exceptions and returns consistent error responses.
/// Logs errors with correlation IDs for distributed tracing.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Get correlation ID for distributed tracing
        var correlationId = context.TraceIdentifier;

        // Log the exception with full details
        _logger.LogError(
            exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}",
            correlationId,
            context.Request.Path,
            context.Request.Method);

        // Determine status code and error details
        var (statusCode, title, detail) = exception switch
        {
            ArgumentNullException nullEx => (
                HttpStatusCode.BadRequest,
                "Missing Required Field",
                nullEx.Message
            ),
            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                "Invalid Argument",
                argEx.Message
            ),
            InvalidOperationException opEx => (
                HttpStatusCode.BadRequest,
                "Invalid Operation",
                opEx.Message
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "Unauthorized",
                "You are not authorized to perform this action"
            ),
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "Resource Not Found",
                "The requested resource was not found"
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "Internal Server Error",
                _environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred. Please try again later."
            )
        };

        // Create RFC 7807 Problem Details response
        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Extensions =
            {
                ["correlationId"] = correlationId,
                ["timestamp"] = DateTime.UtcNow
            }
        };

        // Add stack trace in development
        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
        }

        // Set response
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Extension methods for registering exception handling middleware.
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
