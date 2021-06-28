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
            // Copy original request body to a new stream that allows seeking
            var requestBodyStream = new MemoryStream();
            var originalRequestBody = context.Request.Body;
            await originalRequestBody.CopyToAsync(requestBodyStream);
            requestBodyStream.Seek(0, SeekOrigin.Begin);
            // Read and log body
            var requestBodyText = await new StreamReader(requestBodyStream).ReadToEndAsync();
            _logger.LogInformation("Request: \n" + requestBodyText);
            // Seek beginning of stream and set it back as the body
            requestBodyStream.Seek(0, SeekOrigin.Begin);
            context.Request.Body = requestBodyStream;

            // Set response body to a memory stream
            var bodyStream = context.Response.Body;
            var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            // Call next Middleware
            await next(context);

            // Read response body and then reset the stream to the beginning
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            var responseBodyText = await new StreamReader(responseBodyStream).ReadToEndAsync();
            _logger.LogInformation("Response: \n" + responseBodyText);
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(bodyStream); // why?https://www.sulhome.com/blog/10/log-asp-net-core-request-and-response-using-middleware#:~:text=Log%20Request%20Middleware%20The%20log%20request%20middleware%20extracts,exist.%20Here%20is%20the%20definition%20for%20this%20middleware.
        }
    }

    public static class LoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingMiddleware>();
        }
    }
}
