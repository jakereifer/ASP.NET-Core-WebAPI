using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using Microsoft.AspNetCore.Http;
using TodoApi.Middleware;

namespace TodoApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TodoApi", Description = "This is Jake's Test API", Version = "v1" });
            });
            services.AddDbContext<TodoContext>(opt =>
                                               opt.UseInMemoryDatabase("TodoList"));
            services.AddTransient<FactoryBasedMiddleware>(); // must register factory based

            // If you need to use a service, call the following, but it will create 2 singletons.
            /*
            var sp = services.BuildServiceProvider();
            var logger = sp.GetService<ILogger<Startup>>();
            logger.LogInformation("in CS!");
            */
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoApi v1"));
            }
            app.Use(async (context, next) =>
            {
                // Do work that doesn't write to the Response.
                logger.LogInformation("In first middleware");
                await next.Invoke();
                logger.LogInformation("Back to first middleware");
                // Do logging or other work that doesn't write to the Response.
            });
            app.UseConventionBasedMiddleware();
            app.Map("/error", (hwapp) => hwapp.UseFactoryBasedMiddleware());
            app.Run(async context =>
            {
                logger.LogInformation("In second middleware");
                logger.LogInformation(context.Request.Path);
                await context.Response.WriteAsync("Hello, World!\nPath: " + context.Request.Path);
            });


            // app.UseHttpsRedirection();

            // app.UseRouting();

            // app.UseAuthorization();

            // app.UseEndpoints(endpoints =>
            // {
            //     endpoints.MapControllers();
            // });
        }
    }
}
