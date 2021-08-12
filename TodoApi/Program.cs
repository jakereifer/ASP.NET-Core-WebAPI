using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace TodoApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // SeriLog Creation
            Log.Logger = new LoggerConfiguration()
                // .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                // .WriteTo.File("Log.txt")
                .CreateBootstrapLogger();// Use instead of .CreateLogger() for two-stage initialization

            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
                throw new Exception("Throwing nonsense exception");
            }  catch (Exception ex) {
                Log.Fatal(ex, "Host terminated unexpectedly");
            } finally {
                Log.CloseAndFlush();
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                // Use Serilog as the final logger (completely replacing bootstrap logger)
                .UseSerilog((context, services, configuration) => {
                    // Add serilog app insights
                    // var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
                    // telemetryConfiguration.InstrumentationKey = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
                    configuration
                        .ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services)
                        // .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces)
                        .Enrich.FromLogContext();
                        // .WriteTo.File("Log.txt")

                })

                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // webBuilder.UseUrls("https://localhost:5011");
                    webBuilder.UseStartup<Startup>();
                });
    }
}
