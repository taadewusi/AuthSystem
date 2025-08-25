using AuthSystem.Domain.Exceptions;
using System.Text.Json;

namespace AuthSystem.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                DomainException domainEx => new { message = domainEx.Message, statusCode = 400 },
                ArgumentException argEx => new { message = argEx.Message, statusCode = 400 },
                UnauthorizedAccessException => new { message = "Unauthorized access", statusCode = 401 },
                _ => new { message = "An error occurred", statusCode = 500 }
            };

            context.Response.StatusCode = response.statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
