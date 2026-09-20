using JobApplication.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace JobApplication.API.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (status, title) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                BusinessValidationException => (StatusCodes.Status400BadRequest, "Bad Request"),
                _ => (0, string.Empty)
            };

            if (status == 0)
            {
                // Unexpected: log it and let the default handler return a generic 500 without leaking details.
                _logger.LogError(exception, "Unhandled exception while processing {Path}.", httpContext.Request.Path);
                return false;
            }

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            };

            if (exception is BusinessValidationException validation)
                problem.Extensions["errors"] = validation.Errors;

            if (status == StatusCodes.Status401Unauthorized)
                httpContext.Response.Headers.WWWAuthenticate = "Bearer";

            httpContext.Response.StatusCode = status;
            await httpContext.Response.WriteAsJsonAsync(
                problem, options: null, contentType: "application/problem+json", cancellationToken);
            return true;
        }
    }
}
