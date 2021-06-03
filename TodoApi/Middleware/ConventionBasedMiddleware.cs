using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TodoApi.Middleware {

    /// <summary>
    /// Checks to see if password query is present (mocking an auth code). If not,
    /// we reroute to /error
    ///
    /// </summary>
    public class ConventionBasedMiddleware {

        private readonly RequestDelegate _next;

        public ConventionBasedMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILogger<ConventionBasedMiddleware> logger) {
            logger.LogInformation("In Conventional Middleware!");
            const string password = "password";
            var query = context.Request.Query[password];
            if (string.IsNullOrWhiteSpace(query)) {
                logger.LogInformation("Missing 'password' - Redirecting to /error");
                context.Request.Path = new PathString("/error");
            }

            await _next(context);
        }
    }

    public static class ConventionBasedMiddlewareExtensions
    {
        public static IApplicationBuilder UseConventionBasedMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ConventionBasedMiddleware>();
        }
    }
}
