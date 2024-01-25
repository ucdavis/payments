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
using Microsoft.AspNetCore.SpaServices;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using MvcReact;
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
using Payments.Mvc.Swagger;
using Serilog;
using SpaCliMiddleware;
using Swashbuckle.AspNetCore.Swagger;

namespace Payments.Mvc
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

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
            services.Configure<AggieEnterpriseOptions>(Configuration.GetSection("AggieEnterprise"));

            // write env name
            Log.Information($"Environment: {Environment.EnvironmentName}");

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
            services.AddMvc(options =>
            {
                options.Filters.Add<SerilogControllerActionFilter>();
            })
                .AddNewtonsoftJson((options) =>
                    {
                        options.SerializerSettings.Error += (sender, args) =>
                        {
                            Log.Logger.Warning(args.ErrorContext.Error, "JSON Serialization Error: {message}", args.ErrorContext.Error.Message);
                        };
                    });

            // add the csp-report content type to that handled by the JsonInputFormatter
            // must be done after adding mvc - otherwise the json formatter will not be found.
            services.Configure<MvcOptions>(c =>
            {
                var jsonFormatter = c.InputFormatters.OfType<NewtonsoftJsonInputFormatter>()
                    .First(i => i.SupportedMediaTypes.Contains("application/json"));
                jsonFormatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/csp-report"));
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
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Payments API v1",
                    Version = "v1",
                    Description = "Accept and process credit card payments for CA&ES",
                    Contact = new OpenApiContact()
                    {
                        Name = "CAES Application Requests",
                        Email = "apprequests@caes.ucdavis.edu"
                    },
                    License = new OpenApiLicense()
                    {
                        Name = "MIT",
                        Url = new Uri("https://www.github.com/ucdavis/payments/LICENSE")
                    },
                    Extensions =
                    {
                        {"ProjectUrl", new OpenApiString("https://www.github.com/ucdavis/payments")}
                    }
                });

                var xmlFilePath = Path.Combine(AppContext.BaseDirectory, "Payments.Mvc.xml");
                c.IncludeXmlComments(xmlFilePath);
                c.EnableAnnotations();

                var securityScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "ApiKey"
                    },
                    Type = SecuritySchemeType.ApiKey,
                    Description = "API Key Authentication",
                    Name = ApiKeyMiddleware.HeaderKey,
                    In = ParameterLocation.Header,
                    Scheme = "ApiKey"
                };

                c.AddSecurityDefinition("ApiKey", securityScheme);


                c.OperationFilter<SecurityRequirementsOperationFilter>(securityScheme);

                // In production, the React files will be served from this directory
                services.AddSpaStaticFiles(configuration =>
                {
                    configuration.RootPath = "wwwroot";
                });
            });

            // add hybrid mvc/react support
            services.AddMvcReact();

            // email services
            services.AddMjmlServices();
            // email service must be transient because smtpClient can't handle concurrent connections
            services.AddTransient<IEmailService, SparkpostEmailService>();

            // infrastructure services
            services.AddSingleton<IDataSigningService, DataSigningService>();
            services.AddSingleton<IDirectorySearchService, IetWsSearchService>();
            services.AddSingleton<IFinancialService, FinancialService>();
            services.AddTransient<IAggieEnterpriseService, AggieEnterpriseService>();
            services.AddSingleton<ISlothService, SlothService>();
            services.AddSingleton<IStorageService, StorageService>();

            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<INotificationService, NotificationService>();

            // register job services
            // add jobs services
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddScoped<MoneyMovementJob>();

            // Used by dynamic scripts/styles loader
            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory())); // lgtm [cs/local-not-disposed] 

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<MvcReactOptions> mvcReactOptions)
        {
            app.UseMiddleware<CorrelationIdMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error/500");
            }

            app.UseStatusCodePagesWithReExecute("/Error/{0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseMvcReactStaticFiles();
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

                //Finjector
                c.AllowScripts
                    .From("https://finjector.ucdavis.edu");

                // allow google analytics
                c.AllowScripts
                    .From("https://www.googletagmanager.com")
                    .From("https://www.google-analytics.com");

                c.AllowConnections.ToSelf().To("https://www.google-analytics.com");

                c.AllowImages
                    .From("https://www.google-analytics.com");


                c.AllowStyles
                    .From("https://stackpath.bootstrapcdn.com")
                    .From("https://use.fontawesome.com")
                    .From("https://cdn.datatables.net")
                    .From("https://cdnjs.cloudflare.com")
                    .From("https://cdn.jsdelivr.net");
                

                // allow unsafe methods in development
                // otherwise, support nonce (both aren't supported at the same time
                if (Environment.IsDevelopment())
                {
                    c.AllowStyles
                        .AllowUnsafeInline(); // to support webpack style injection

                    c.AllowScripts
                        .AllowUnsafeInline()
                        .AllowUnsafeEval();

                    // allow HMR connections
                    c.AllowConnections
                        .To("wss://localhost:*");
                }
                else
                {
                    c.AllowScripts
                        .AddNonce();

                    // cloudflare rocket-loader
                    c.AllowScripts
                        .From("https://ajax.cloudflare.com");

                    // in prod we load CRA generated, self hosted files
                    c.AllowStyles
                        .AddNonce()
                        .FromSelf();
                }


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
                    var path = context.HttpContext.Request.Path;
                    context.ShouldNotSend = path.StartsWithSegments("/api")
                        || path.StartsWithSegments("/api-docs")
                        || path.StartsWithSegments("/info");
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

            app.UseRouting();

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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // customer routes
                endpoints.MapControllerRoute(
                    name: "pay-responses",
                    pattern: "pay/{action}",
                    defaults: new { controller = "payments" },
                    constraints: new { action = "(receipt|cancel|providernotify)" });

                endpoints.MapControllerRoute(
                    name: "pay-invoice",
                    pattern: "pay/{id}",
                    defaults: new { controller = "payments", action = "pay" });

                endpoints.MapControllerRoute(
                    name: "download-invoice",
                    pattern: "download/{id}",
                    defaults: new { controller = "payments", action = "download" });

                endpoints.MapControllerRoute(
                    name: "invoice-file",
                    pattern: "file/{id}/{fileId}",
                    defaults: new { controller = "payments", action = "file" });

                // non team root routes
                endpoints.MapControllerRoute(
                    name: "non-team-routes",
                    pattern: "{controller}/{action=Index}/{id?}",
                    defaults: new { },
                    constraints: new { controller = "(account|teams|jobs|system)" });

                // fallback to home which will load SPA
                // TODO: likely will need to add some team routes here
                if (env.IsDevelopment())
                {
                    // team level routes
                    endpoints.MapControllerRoute(
                        name: "team-index",
                        pattern: "{team}",
                        defaults: new { controller = "home", action = "teamindex" },
                        constraints: new
                        {
                            team = new CompositeRouteConstraint(new IRouteConstraint[]{
                                new RegexInlineRouteConstraint(Team.SlugRegex),
                                new NotConstraint("(reports|support)"),
                                new RegexInlineRouteConstraint(mvcReactOptions.Value.ExcludeHmrPathsRegex)
                            })
                        });

                    endpoints.MapControllerRoute(
                        name: "team-routes",
                        pattern: "{team}/{controller=Home}/{action=Index}/{id?}",
                        defaults: new { },
                        constraints: new
                        {
                            team = new CompositeRouteConstraint(new IRouteConstraint[]{
                                new RegexInlineRouteConstraint(Team.SlugRegex),
                                new NotConstraint("(reports|support)"),
                                new RegexInlineRouteConstraint(mvcReactOptions.Value.ExcludeHmrPathsRegex)
                            })
                        });

                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}",
                        constraints: new { controller = mvcReactOptions.Value.ExcludeHmrPathsRegex });
                }
                else
                {

                    // team level routes
                    endpoints.MapControllerRoute(
                        name: "team-index",
                        pattern: "{team}",
                        defaults: new { controller = "home", action = "teamindex" },
                        constraints: new
                        {
                            team = new CompositeRouteConstraint(new IRouteConstraint[]{
                                new RegexInlineRouteConstraint(Team.SlugRegex),
                                new NotConstraint("(reports|support)"),
                                new RegexInlineRouteConstraint(mvcReactOptions.Value.ExcludeHmrPathsRegex)
                            })
                        });

                    endpoints.MapControllerRoute(
                        name: "team-routes",
                        pattern: "{team}/{controller=Home}/{action=Index}/{id?}",
                        defaults: new { },
                        constraints: new
                        {
                            team = new CompositeRouteConstraint(new IRouteConstraint[]{
                                new RegexInlineRouteConstraint(Team.SlugRegex),
                                new NotConstraint("(reports|support)"),
                                new RegexInlineRouteConstraint(mvcReactOptions.Value.ExcludeHmrPathsRegex)
                            })
                        });
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}",
                        constraints: new { controller = mvcReactOptions.Value.ExcludeHmrPathsRegex });
                }
            });

            // During development, SPA will kick in for all remaining paths
            app.UseMvcReact();
        }
    }
}
