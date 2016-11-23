using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Payments.Core;
using Payments.Core.Models;
using Payments.Infrastructure;
using Serilog;

namespace Payments
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets();
            }

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // var connection = @"Server=.\sqlexpress;Database=Payments;Trusted_Connection=True;";
            services
                //.AddEntityFramework()
                //.AddEntityFrameworkSqlServer()
                .AddDbContext<PaymentsContext>(options => options.UseInMemoryDatabase("payments"));
                //.AddDbContext<PaymentsContext>(options => options.UseSqlServer(connection));

            // automapper services
            services.AddAutoMapper(typeof(Startup));
            Mapper.AssertConfigurationIsValid();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // trace logging
            app.UseMiddleware<StackifyMiddleware.RequestTracerMiddleware>();

            // local logging
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // remote logging
            Logging.ConfigureLogging(Configuration);
            loggerFactory.AddSerilog();

            // log a correlation id
            app.UseMiddleware<RequestContextMiddleware>("HttpRequestId");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();

                SeedDatabase(app);
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void SeedDatabase(IApplicationBuilder app)
        {
            var context = app.ApplicationServices.GetService<PaymentsContext>();

            context.Invoices.Add(new Invoice { Title = "Lab Work", TotalAmount = 78.90M});

            context.SaveChanges();
        }
    }
}
