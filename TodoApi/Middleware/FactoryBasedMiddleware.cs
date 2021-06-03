using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TodoApi.Middleware {

    /// <summary>
    /// Will throw an exception or write response saying password is missing
    /// </summary>
    public class FactoryBasedMiddleware: IMiddleware {

        private readonly ILogger<FactoryBasedMiddleware> _logger;

        public FactoryBasedMiddleware(ILogger<FactoryBasedMiddleware> logger) {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next) {
            var errorMessage = "Missing password in query string";
            _logger.LogError(errorMessage);
            throw new Exception(errorMessage); //
            // await context.Response.WriteAsync("Missing password in query string!");
        }
    }

    public static class FactoryBasedMiddlewareExtensions
    {
        public static IApplicationBuilder UseFactoryBasedMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FactoryBasedMiddleware>();
        }
    }
}
