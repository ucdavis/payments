using AspNetCore.Security.CAS;
using jsreport.AspNetCore;
using jsreport.Binary;
using jsreport.Local;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Models.Configuration;
using Payments.Core.Services;
using Payments.Mvc.Authorization;
using Payments.Mvc.Handlers;
using Payments.Mvc.Identity;
using Payments.Mvc.Models.Configuration;
using Payments.Mvc.Models.Roles;
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
                .AddUserManager<ApplicationUserManager>()
                .AddDefaultTokenProviders();

            services.AddAuthentication()
                .AddCAS("UCDavis", options =>
                {
                    options.CasServerUrlBase = Configuration["Settings:CasBaseUrl"];
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyCodes.TeamAdmin, policy => policy.Requirements.Add(new VerifyTeamPermission(TeamRole.Codes.Admin)));
                options.AddPolicy(PolicyCodes.TeamEditor, policy => policy.Requirements.Add(new VerifyTeamPermission(TeamRole.Codes.Admin, TeamRole.Codes.Editor)));
            });
            services.AddScoped<IAuthorizationHandler, VerifyTeamPermissionHandler>();

            services.AddMvc();

            // add pdf reporting server
            services.AddJsReport(new LocalReporting()
                .UseBinary(JsReportBinary.GetBinary())
                .AsUtility()
                .Create());

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
                    HotModuleReplacement = true,
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
                    template: "pay/{id}",
                    defaults: new { controller = "payments", action="pay" });

                routes.MapRoute(
                    name: "admin-routes",
                    template: "{controller}/{action=Index}/{id?}",
                    defaults: new { },
                    constraints: new { controller = "(account|system)" });

                routes.MapRoute(
                    name: "team-routes",
                    template: "{team}/{controller=Home}/{action=Index}/{id?}",
                    defaults: new { },
                    constraints: new
                    {
                        controller = "(home|invoices|teams|financialAccounts)",
                    });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
