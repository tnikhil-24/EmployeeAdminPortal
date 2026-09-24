using EmployeeAdminPortal.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdminPortal.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService problemDetailsService;
        private readonly ILogger<GlobalExceptionHandler> logger;

        public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
        {
            this.problemDetailsService = problemDetailsService;
            this.logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, title, detail) = exception switch
            {
                DuplicateEmailException =>
                    (StatusCodes.Status409Conflict, "Email already in use", exception.Message),

                // Race condition: two requests passed the email check, the unique index rejected the second
                DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } } =>
                    (StatusCodes.Status409Conflict, "Email already in use", "An employee with this email already exists."),

                _ =>
                    (StatusCodes.Status500InternalServerError, "An unexpected error occurred", "Please try again later.")
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Unhandled exception for {Method} {Path}",
                    httpContext.Request.Method, httpContext.Request.Path);
            }
            else
            {
                logger.LogWarning("Handled {ExceptionType}: {Message}", exception.GetType().Name, exception.Message);
            }

            httpContext.Response.StatusCode = statusCode;

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = detail
                }
            });
        }
    }
}