using System.Net;


namespace CatalogMicroservice.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = "An unexpected error occurred.";

            switch (exception)
            {
                case ApplicationException e:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = e.Message;
                    break;
                case KeyNotFoundException e:
                    statusCode = (int)HttpStatusCode.NotFound;
                    message = e.Message;
                    break;
                case UnauthorizedAccessException e:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    message = e.Message;
                    break;
                case ArgumentException e:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = e.Message;
                    break;
                case InvalidOperationException e:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = e.Message;
                    break;
                default:
                    break;
            }

            context.Response.StatusCode = statusCode;

            var result = new
            {
                context.Response.StatusCode,
                Message = message
            };

            return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
        }
    }
}
