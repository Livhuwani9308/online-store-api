using online_store_api.Common;

namespace online_store_api.Middleware
{
    public class GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                context.Response.StatusCode = 500;

                await context.Response.WriteAsJsonAsync(
                    new ServiceResponse<string>
                    {
                        IsSuccess = false,
                        StatusCode = 500,
                        Message = "An unexpected error occurred.",
                        Data = null
                    });
            }
        }
    }
}
