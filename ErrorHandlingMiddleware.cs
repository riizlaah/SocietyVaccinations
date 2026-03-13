using System.Text.Json;

namespace SocietyVaccinations
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var res = ApiResponse<object>.Error(ex.Message, context.Response.StatusCode);
                var jsonRes = JsonSerializer.Serialize(res);
                context.Response.ContentType = "application/json";
                context.Response.WriteAsync(jsonRes);
            }
        }

        
    }
}
