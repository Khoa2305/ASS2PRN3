using Assignment1.CustomException;
using Assignment1.dto;

namespace Assignment1.Middleware
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        public GlobalExceptionHandler(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException ex)
            {
                AppException appException = ex as AppException;
                context.Response.StatusCode = ((int)appException.StatusCode);
                await context.Response.WriteAsJsonAsync(new ApiResponse<Object>
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = new
                    {
                        Code = ex.Code,
                    }
                });
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new ApiResponse<Object>
                {
                    Success = false,
                    Message = ex.Message,
                });
            }
        }
    }
}
