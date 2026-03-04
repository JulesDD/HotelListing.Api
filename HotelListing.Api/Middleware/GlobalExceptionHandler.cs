using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HotelListing.Api.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Log the exception details
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        logger.LogError(exception, "An unhandled exception occurred. TraceId: {TraceId}, Path: {Path}, Method: {Method}", 
            traceId, 
            httpContext.Request.Path, 
            httpContext.Request.Method);

        // Create a ProblemDetails response
        var problemDetails = new ProblemDetails
        {
            Title = "An unexpected error occurred.",
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://httpstatuses.com/500",
            Instance = httpContext.Request.Path,
            Detail = httpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment() 
                ? exception.Message: "An unexpected error occurred while processing your request. Please try again later."
            
        };

        // Include the traceId in the response for correlation
        problemDetails.Extensions["traceId"] = traceId;
        if (httpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            problemDetails.Extensions["exceptionType"] = exception.GetType().FullName;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        // Set the response status code and content type
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;


    }
}
