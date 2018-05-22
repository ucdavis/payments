using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Security.CAS;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Models.Configuration;
using Payments.Core.Services;
using Payments.Mvc.Models.Configuration;
using Payments.Mvc.Services;

namespace Payments.Mvc
{
    public class Startup
    {

        public Startup(IHostingEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private readonly IHostingEnvironment _environment;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<Settings>(Configuration.GetSection("Settings"));
            services.Configure<CyberSourceSettings>(Configuration.GetSection("CyberSource"));
            services.Configure<SparkpostSettings>(Configuration.GetSection("Sparkpost"));

            // setup entity framework
            if (!_environment.IsDevelopment() || Configuration.GetSection("Dev:UseSql").Value == "True")
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
                );
            }
            else
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite("Data Source=payments.db")
                );
            }

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication()
                .AddCAS("UCDavis", options =>
                {
                    options.CasServerUrlBase = Configuration["Settings:CasBaseUrl"];
                });

            services.AddMvc();

            // application services
            services.AddTransient<IDataSigningService, DataSigningService>();
            services.AddTransient<IEmailService, SparkpostEmailService>();
            services.AddTransient<IDirectorySearchService, IetWsSearchService>();
            services.AddTransient<IFinancialService, FinancialService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    // HotModuleReplacement = true,
                    // ReactHotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "pay-invoice",
                    template: "{pay}/{id}",
                    defaults: new { controller = "payments", action="pay" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
