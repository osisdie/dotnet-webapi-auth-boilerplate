using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CoreFX.Abstractions.App_Start;
using CoreFX.Abstractions.Consts;
using CoreFX.Abstractions.Logging;
using CoreFX.Auth.Interfaces;
using CoreFX.Auth.Services;
using CoreFX.Caching.Redis.Extensions;
using CoreFX.Common.Extensions;
using CoreFX.Hosting.Extensions;
using CoreFX.Hosting.Middlewares;
using CoreFX.Notification.Smtp.Extensions;
using FluentValidation.AspNetCore;
using Hello6.Domain.Common;
using Hello6.Domain.Common.Consts;
using Hello6.Domain.Common.Models;
using Hello6.Domain.Contract.HelloServices.Echo;
using Hello6.Domain.Contract.Models.Notification;
using Hello6.Domain.DataAccess.Database;
using Hello6.Domain.DataAccess.Database.Echo.Interfaces;
using Hello6.Domain.Endpoint.Services.HelloServices.SendCommand;
using Hello6.Domain.SDK.Caching.Extensions;
using Hello6.Domain.SDK.Services.AuthServices.Login;
using Hello6.Domain.SDK.Services.HelloServices.SendCommand;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Hello6.Domain.Endpoint
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            SdkRuntime.Configuration = configuration;
            HelloContext.Settings = configuration.GetSection(HelloConst.DefaultSectionName).Get<HelloConfiguration>() ?? new HelloConfiguration();

            // Load ordering: EnvironmentVariable -> hellosettings.json
            HelloContext.Settings.HELLODB_CONN ??= configuration.GetValue<string>(HelloConst.DefaultDatabaseConnectionKey) ?? configuration.GetConnectionString(HelloConst.DefaultDatabaseConnectionKey);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Foundation
            services.AddLogging();
            services.AddOptions();
            services.AddHttpClient();
            services.AddHttpContextAccessor();

            var secret = Configuration.GetValue<string>("AuthConfig:JwtConfig:Secret");
            if (!string.IsNullOrEmpty(secret))
            {
                var secretKey = Encoding.ASCII.GetBytes(secret);
                services.AddAuthentication(auth =>
                {
                    auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(token =>
                {
                    token.RequireHttpsMetadata = false;
                    token.SaveToken = true;
                    token.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                        ValidateIssuer = true,
                        ValidIssuer = "https://auth0.com",
                        ValidateAudience = true,
                        ValidAudience = "https://auth0.com",
                        RequireExpirationTime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidateLifetime = true,
                        //LifetimeValidator = (DateTime? notBefore, DateTime? expires, SecurityToken securityToken,
                        //                 TokenValidationParameters validationParameters) =>
                        //{
                        //    return notBefore <= DateTime.UtcNow && expires >= DateTime.UtcNow;
                        //}
                    };
                });
            }

            // CoreFX DI
            // services.AddRedisCache(Configuration.GetValue<string>(CacheConst.DefaultConnectionKey));
            services.AddRedisCache(Configuration);
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
            services.AddSingleton<ISessionAdmin, SessionAdmin>();
            services.AddEmailService(options =>
            {
                Configuration.GetSection("NotifyConfig:EmailConfig")?.Bind(options);
            }, optional: true);
            services.AddReportService<Hello6_ReportDecordDto>(options =>
            {
                Configuration.GetSection("NotifyConfig:ReportConfig")?.Bind(options);
            }, optional: true);

            // Hello6 DI
            services.AddSingleton<IEchoRepository, EchoRepository>();
            services.AddTransient<IHelloSendCommand_Service, HelloSendCommand_Service>();
            services.AddTransient<IAuthLogin_Service, AuthLogin_MockService>();

            // MVC
            services.AddHealthChecks();
            services.AddControllers()
            //.AddJsonOptions(options => {
            //    options.JsonSerializerOptions.IgnoreNullValues = true;
            //})
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.Converters.Add(new IsoDateTimeConverter());
            })
            .AddFluentValidation(opt =>
            {
                opt.RegisterValidatorsFromAssemblies(new List<Assembly>
                {
                    typeof(HelloSendCommand_RequestValidator).Assembly,
                    typeof(AuthLogin_RequestValidator).Assembly,
                    typeof(Program).Assembly,
                }.Distinct());
            })
            .ConfigureApiBehaviorOptions(opt =>
            {
                opt.InvalidModelStateResponseFactory = c =>
                {
                    return c.ModelState.ToErrorJson400();
                };
            })
            .AddControllersAsServices();

            services.AddSwaggerGen(c =>
            {
                c.ResolveConflictingActions(descriptions =>
                {
                    return descriptions.First();
                });
                c.EnableAnnotations();

                c.SwaggerDoc("v202603", new OpenApiInfo { Title = "Auth v202603", Version = "202603" });
                c.SwaggerDoc("v202104", new OpenApiInfo { Title = "Auth v202104", Version = "202104" });
                c.SwaggerDoc("v202103", new OpenApiInfo { Title = "Auth v202103", Version = "202103" });

                c.DocumentFilter<RemoveDefaultApiVersionRouteDocumentFilter>();

                var xmlPath = Path.Combine(AppContext.BaseDirectory, "Swagger.xml");
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            SdkRuntime.SdkEnv = env.EnvironmentName;
            SdkRuntime.HostingEnv = env;
            LogMgr.LoggerFactory = loggerFactory;

            var envLog4netPath = Path.Combine(SvcConst.DefaultConfigFolder, SvcConst.DefaultLog4netConfigFile.AddingBeforeExtension(env.EnvironmentName));
            var defaultLog4netPath = Path.Combine(SvcConst.DefaultConfigFolder, SvcConst.DefaultLog4netConfigFile);
            if (File.Exists(envLog4netPath))
            {
                loggerFactory.AddLog4Net(envLog4netPath);
            }
            else if (File.Exists(defaultLog4netPath))
            {
                loggerFactory.AddLog4Net(defaultLog4netPath);
            }

            if (env.IsDevelopment() ||
                env.IsEnvironment(EnvConst.Testing) ||
                env.IsEnvironment(EnvConst.Debug))
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v202603/swagger.json", "Auth v202603");
                    c.SwaggerEndpoint("/swagger/v202104/swagger.json", "Auth v202104");
                    c.SwaggerEndpoint("/swagger/v202103/swagger.json", "Auth v202103");
                });
            }

            app.UseRouting();

            app.UseJwtAuthorization();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRequestResponseLogging();
            app.UseExceptionHandlerMiddleware();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
            });
        }
    }
}
