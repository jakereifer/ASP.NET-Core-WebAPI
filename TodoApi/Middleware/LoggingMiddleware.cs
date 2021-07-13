using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TodoApi.Middleware {

    /// <summary>
    /// Logs request and response
    /// </summary>
    public class LoggingMiddleware: IMiddleware {

        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(ILogger<LoggingMiddleware> logger) {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next) {

            // Keep references to original streams
            var originalRequestStream = context.Request.Body;
            var originalResponseStream = context.Response.Body;

            // Copy the request stream
            var requestStreamCopy = new MemoryStream();
            await originalRequestStream.CopyToAsync(requestStreamCopy);
            requestStreamCopy.Position = 0; // Make sure its at the beginning of the stream

            var requestReader = new StreamReader(requestStreamCopy);
            var requestContent = await requestReader.ReadToEndAsync(); // Read it in
            _logger.LogInformation($"Request Body:\n{requestContent}");
            requestStreamCopy.Position = 0; // make sure its back at the beginning

            // Set request body pointer to original stream
            context.Request.Body = requestStreamCopy;

            using (var newResponseStream = new MemoryStream()) {
                context.Response.Body = newResponseStream;
                await next(context);
                // Make sure we read from beginning
                newResponseStream.Position = 0;
                var responseReader = new StreamReader(newResponseStream);
                var responseContent = await responseReader.ReadToEndAsync();
                _logger.LogInformation($"Response Body:\n{responseContent}");

                newResponseStream.Position = 0;
                await newResponseStream.CopyToAsync(originalResponseStream);
                // Set response body pointer to original stream (this one is out of scope and the old one has the proper properties)
                context.Response.Body = originalResponseStream;
            }
        }
    }

    public static class LoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingMiddleware>();
        }
    }


    // helper methods
}
