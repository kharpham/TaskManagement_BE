using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TaskManagementApp.Middlewares
{
    public class UserIdMiddleware
    {
        private readonly RequestDelegate _next;

        public UserIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    context.Items["UserId"] = userId;
                    System.Diagnostics.Debug.WriteLine($"UserIdMiddleware: User ID set to {userId}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("UserIdMiddleware: User ID claim is missing.");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("UserIdMiddleware: User is not authenticated.");
            }

            await _next(context);
        }
    }
}
