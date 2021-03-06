using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Security.CAS;
using Joonasw.AspNetCore.SecurityHeaders;
using jsreport.AspNetCore;
using jsreport.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mjml.AspNetCore;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Jobs;
using Payments.Core.Models.Configuration;
using Payments.Core.Services;
using Payments.Emails;
using Payments.Mvc.Authorization;
using Payments.Mvc.Handlers;
using Payments.Mvc.Helpers;
using Payments.Mvc.Identity;
using Payments.Mvc.Logging;
using Payments.Mvc.Models;
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
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }

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
            services.Configure<JsReportSettings>(Configuration.GetSection("JsReport"));
            services.Configure<SuperuserSettings>(Configuration.GetSection("Superuser"));

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
                options.AddPolicy(PolicyCodes.TeamEditor, policy => policy.Requirements.Add(new VerifyTeamPermission(TeamRole.Codes.Admin, TeamRole.Codes.Editor, TeamRole.Codes.FinanceOfficer)));
                options.AddPolicy(PolicyCodes.FinancialOfficer, policy => policy.Requirements.Add(new VerifyTeamPermission(TeamRole.Codes.Admin, TeamRole.Codes.FinanceOfficer)));
            });
            services.AddScoped<IAuthorizationHandler, VerifyApiKeyRequirementHandler>();
            services.AddScoped<IAuthorizationHandler, VerifyServiceKeyRequirementHandler>();
            services.AddScoped<IAuthorizationHandler, VerifyTeamPermissionHandler>();

            // add application services
            services.AddMvc(options => {
                // add the csp-report content type to that handled by the JsonInputFormatter
                options
                    .InputFormatters
                    .Where(item => item.GetType() == typeof(JsonInputFormatter))
                    .Cast<JsonInputFormatter>()
                    .Single()
                    .SupportedMediaTypes
                    .Add("application/csp-report");

                options.Filters.Add<SerilogControllerActionFilter>();
            })
                .AddJsonOptions((options) =>
                    {
                        options.SerializerSettings.Error += (sender, args) =>
                        {
                            Log.Logger.Warning(args.ErrorContext.Error, "JSON Serialization Error: {message}", args.ErrorContext.Error.Message);
                        };
                    });

            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddCsp(nonceByteAmount: 32);

            // add pdf reporting server
            services.AddJsReport(new ReportingService(
                Configuration.GetValue<string>("JsReport:ServiceUrl"),
                Configuration.GetValue<string>("JsReport:Username"),
                Configuration.GetValue<string>("JsReport:Password")));

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
                        Name = "CAES Application Requests",
                        Email = "apprequests@caes.ucdavis.edu"
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

            // email services
            services.AddMjmlServices();
            services.AddSingleton<IEmailService, SparkpostEmailService>();

            // infrastructure services
            services.AddSingleton<IDataSigningService, DataSigningService>();
            services.AddSingleton<IDirectorySearchService, IetWsSearchService>();
            services.AddSingleton<IFinancialService, FinancialService>();
            services.AddSingleton<ISlothService, SlothService>();
            services.AddSingleton<IStorageService, StorageService>();

            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<INotificationService, NotificationService>();

            // register job services
            // add jobs services
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddScoped<MoneyMovementJob>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<CorrelationIdMiddleware>();

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
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSerilogRequestLogging();

            // various security middlewares
            app.UseCsp(c =>
            {
                c.ReportViolationsTo("/csp-report");

                c.ByDefaultAllow.FromSelf();

                c.AllowScripts
                    .FromSelf()
                    .From("https://cdnjs.cloudflare.com")
                    .From("https://cdn.jsdelivr.net")
                    .From("https://cdn.datatables.net")
                    .From("https://code.jquery.com")
                    .From("https://stackpath.bootstrapcdn.com")
                    .From("https://ajax.aspnetcdn.com");

                // allow google analytics
                c.AllowScripts
                    .From("https://www.googletagmanager.com")
                    .From("https://www.google-analytics.com");

                c.AllowConnections.ToSelf().To("https://www.google-analytics.com");

                c.AllowImages
                    .From("https://www.google-analytics.com");

                // allow unsafe methods in development
                // otherwise, support nonce (both aren't supported at the same time
                if (Environment.IsDevelopment())
                {
                    c.AllowScripts
                        .AllowUnsafeInline()
                        .AllowUnsafeEval();

                    // allow stackify prefix
                    c.AllowScripts
                        .From("http://127.0.0.1:2012/scripts/stckjs.min.js");
                }
                else
                {
                    c.AllowScripts
                        .AddNonce();

                    // cloudflare rocket-loader
                    c.AllowScripts
                        .From("https://ajax.cloudflare.com");
                }

                c.AllowStyles
                    .AddNonce()
                    .FromSelf()
                    .From("https://stackpath.bootstrapcdn.com")
                    .From("https://use.fontawesome.com")
                    .From("https://cdn.datatables.net")
                    .From("https://cdnjs.cloudflare.com")
                    .From("https://cdn.jsdelivr.net");

                // allow style loader in development
                if (Environment.IsDevelopment())
                {
                    c.AllowStyles
                        .From("blob:");
                }

                c.AllowImages
                    .FromSelf()
                    .From("data:")
                    .From("https://secure.gravatar.com");

                c.AllowFonts
                    .FromSelf()
                    .From("data:")
                    .From("https://use.fontawesome.com");

                c.OnSendingHeader = context =>
                {
                    context.ShouldNotSend = context.HttpContext.Request.Path.StartsWithSegments("/api");
                    context.ShouldNotSend = context.HttpContext.Request.Path.StartsWithSegments("/api-docs");
                    return Task.CompletedTask;
                };
            });
            // we allow same origin iframes for preview windows
            app.UseXFrameOptions(new XFrameOptionsOptions(XFrameOptionsOptions.XFrameOptionsValues.SameOrigin));
            app.UseXXssProtection(new XXssProtectionOptions(enableProtection: true, enableAttackBlock: true));
            app.UseXContentTypeOptions(new XContentTypeOptionsOptions(allowSniffing: false));
            app.UseReferrerPolicy(new ReferrerPolicyOptions(ReferrerPolicyOptions.ReferrerPolicyValue.StrictOrigin));
            app.UseFeaturePolicy(b =>
            {
                b.AllowAccelerometer.FromNowhere();
                b.AllowAutoplay.FromNowhere();
                b.AllowCamera.FromNowhere();
                b.AllowEncryptedMedia.FromNowhere();
                b.AllowFullscreen.FromNowhere();
                b.AllowGeolocation.FromNowhere();
                b.AllowGyroscope.FromNowhere();
                b.AllowMagnetometer.FromNowhere();
                b.AllowMicrophone.FromNowhere();
                b.AllowMidi.FromNowhere();
                b.AllowPayment.FromNowhere();
                b.AllowPictureInPicture.FromNowhere();
                b.AllowSyncXhr.FromNowhere();
                b.AllowUsb.FromNowhere();
            });

            // authentication middlwares
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
                    name: "download-invoice",
                    template: "download/{id}",
                    defaults: new { controller = "payments", action = "download" });

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
