using System;
using System.Collections.Generic;
using System.IO;
using AspNetCore.Security.CAS;
using jsreport.AspNetCore;
using jsreport.Binary;
using jsreport.Local;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Jobs;
using Payments.Core.Models.Configuration;
using Payments.Core.Services;
using Payments.Mvc.Authorization;
using Payments.Mvc.Handlers;
using Payments.Mvc.Helpers;
using Payments.Mvc.Identity;
using Payments.Mvc.Logging;
using Payments.Mvc.Models.Configuration;
using Payments.Mvc.Models.Roles;
using Payments.Mvc.Services;
using Serilog;
using StackifyLib;
using Swashbuckle.AspNetCore.Swagger;

namespace Payments.Mvc
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            Environment = env;
        }

        public IConfigurationRoot Configuration { get; }

        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // add various options
            services.Configure<Settings>(Configuration.GetSection("Settings"));
            services.Configure<CyberSourceSettings>(Configuration.GetSection("CyberSource"));
            services.Configure<FinanceSettings>(Configuration.GetSection("Finance"));
            services.Configure<SlothSettings>(Configuration.GetSection("Sloth"));
            services.Configure<SparkpostSettings>(Configuration.GetSection("Sparkpost"));
            services.Configure<StorageSettings>(Configuration.GetSection("Storage"));
            services.Configure<PaymentsApiSettings>(Configuration.GetSection("PaymentsApi"));

            // setup entity framework / database
            if (!Environment.IsDevelopment() || Configuration.GetSection("Dev:UseSql").Value == "True")
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

            // add identity stores/providers
            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddUserManager<ApplicationUserManager>()
                .AddDefaultTokenProviders();

            // add cas auth
            services.AddAuthentication()
                .AddCAS("UCDavis", options =>
                {
                    options.CasServerUrlBase = Configuration["Settings:CasBaseUrl"];
                });

            // add policy auth
            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyCodes.ApiKey, policy => policy.Requirements.Add(new VerifyApiKeyRequirement()));
                options.AddPolicy(PolicyCodes.ServiceKey, policy => policy.Requirements.Add(new VerifyServiceKeyRequirement()));
                options.AddPolicy(PolicyCodes.TeamAdmin, policy => policy.Requirements.Add(new VerifyTeamPermission(TeamRole.Codes.Admin)));
                options.AddPolicy(PolicyCodes.TeamEditor, policy => policy.Requirements.Add(new VerifyTeamPermission(TeamRole.Codes.Admin, TeamRole.Codes.Editor)));
                options.AddPolicy(PolicyCodes.FinancialOfficer, policy => policy.Requirements.Add(new VerifyTeamPermission(TeamRole.Codes.FinanceOfficer)));
            });
            services.AddScoped<IAuthorizationHandler, VerifyApiKeyRequirementHandler>();
            services.AddScoped<IAuthorizationHandler, VerifyServiceKeyRequirementHandler>();
            services.AddScoped<IAuthorizationHandler, VerifyTeamPermissionHandler>();

            // add application services
            services.AddMvc();
            services.AddDistributedMemoryCache();
            services.AddSession();

            // add pdf reporting server
            services.AddJsReport(new LocalReporting()
                .UseBinary(JsReportBinary.GetBinary())
                .Configure(c =>
                {
                    c.AllowLocalFilesAccess = true;
                    c.FileSystemStore().BaseUrlAsWorkingDirectory();
                    return c;
                })
                .RunInDirectory(Environment.ContentRootPath)
                .AsUtility()
                .Create());

            // add swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Payments API v1",
                    Version = "v1",
                    Description = "Accept and process credit card payments for CA&ES",
                    Contact = new Contact()
                    {
                        Name = "John Knoll",
                        Email = "jpknoll@ucdavis.edu"
                    },
                    License = new License()
                    {
                        Name = "MIT",
                        Url = "https://www.github.com/ucdavis/payments/LICENSE"
                    },
                    Extensions =
                    {
                        {"ProjectUrl", "https://www.github.com/ucdavis/payments"}
                    }
                });

                var xmlFilePath = Path.Combine(AppContext.BaseDirectory, "Payments.Mvc.xml");
                c.IncludeXmlComments(xmlFilePath);
                c.EnableAnnotations();

                c.AddSecurityDefinition("apiKey", new ApiKeyScheme()
                {
                    Description = "API Key Authentication",
                    Name = ApiKeyMiddleware.HeaderKey,
                    In = "header",
                    Type = "apiKey"
                });

                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "apiKey", new string[] { } }
                });

                c.OperationFilter<FileOperationFilter>();
            });

            // infrastructure services
            services.AddSingleton<IDataSigningService, DataSigningService>();
            services.AddSingleton<IEmailService, SparkpostEmailService>();
            services.AddSingleton<IDirectorySearchService, IetWsSearchService>();
            services.AddSingleton<IFinancialService, FinancialService>();
            services.AddSingleton<ISlothService, SlothService>();
            services.AddSingleton<IStorageService, StorageService>();

            services.AddScoped<INotificationService, NotificationService>();

            // register job services
            // add jobs services
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddScoped<MoneyMovementJob>();
            services.AddScoped<TaxReportJob>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IApplicationLifetime appLifetime)
        {
            // setup logging
            LoggingConfiguration.Setup(Configuration);
            app.ConfigureStackifyLogging(Configuration);
            loggerFactory.AddSerilog();

            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<LoggingIdentityMiddleware>();

            // setup tracing
            app.UseMiddleware<StackifyMiddleware.RequestTracerMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    ReactHotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Error/500");
            }

            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            app.UseStaticFiles();

            // various authentication middlewares
            app.UseAuthentication();
            app.UseMiddleware<ApiKeyMiddleware>();
            app.UseMiddleware<ServiceKeyMiddleware>();

            app.UseSession();

            // add swagger docs and ui
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "api-docs/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "api-docs";
                c.SwaggerEndpoint("/api-docs/v1/swagger.json", "Payments API v1");
            });

            app.UseMvc(routes =>
            {
                // customer routes
                routes.MapRoute(
                    name: "pay-responses",
                    template: "pay/{action}",
                    defaults: new { controller = "payments" },
                    constraints: new { action = "(receipt|cancel|providernotify)" });

                routes.MapRoute(
                    name: "pay-invoice",
                    template: "pay/{id}",
                    defaults: new { controller = "payments", action = "pay" });

                routes.MapRoute(
                    name: "invoice-file",
                    template: "file/{id}/{fileId}",
                    defaults: new { controller = "payments", action = "file" });

                // non team root routes
                routes.MapRoute(
                    name: "non-team-routes",
                    template: "{controller}/{action=Index}/{id?}",
                    defaults: new { },
                    constraints: new { controller = "(account|teams|jobs|system)" });

                // team level routes
                routes.MapRoute(
                    name: "team-index",
                    template: "{team}",
                    defaults: new { controller = "home", action = "teamindex" },
                    constraints: new
                    {
                        team = new CompositeRouteConstraint(new IRouteConstraint[]{
                            new RegexInlineRouteConstraint(Team.SlugRegex),
                            new NotConstraint("(reports|support)"),
                        })
                    });

                routes.MapRoute(
                    name: "team-routes",
                    template: "{team}/{controller=Home}/{action=Index}/{id?}",
                    defaults: new { },
                    constraints: new
                    {
                        team = new CompositeRouteConstraint(new IRouteConstraint[]{
                            new RegexInlineRouteConstraint(Team.SlugRegex),
                            new NotConstraint("(reports|support)"),
                        })
                    });

                // le default fallback for controllers that are excluded by the team = NotConstraint above
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
