using Dams.ms.auth.Adapters;
using Dams.ms.auth.Data;
using Dams.ms.auth.Helpers;
using IdentityServer4.AccessTokenValidation;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Dams.ms.auth
{
    public class Startup
    {

        private readonly IWebHostEnvironment _env;
        public IConfigurationRoot Configuration { get; }

        public Startup(IWebHostEnvironment env)
        {
            _env = env;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            Log.Logger = new LoggerConfiguration()
           .ReadFrom.Configuration(Configuration.GetSection("Serilog"))
           .CreateLogger();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<IDMDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<UserEntity, IdentityRole<int>>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<IDMDbContext>()
            .AddDefaultTokenProviders();

            services.AddIdentityServer(Options =>
            {
                Options.IssuerUri = Configuration.GetSection("IdentityServer:Authority").Value;
            })
            .AddSigningCredential(new X509Certificate2(Path.Combine(_env.WebRootPath, Configuration.GetSection("IdentityServer:CertName").Value), Configuration.GetSection("IdentityServer:CertPassword").Value, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable))
            .AddInMemoryPersistedGrants()
            .AddInMemoryApiScopes(Config.ApiScopes())
            .AddInMemoryApiResources(Config.GetApiResources())
            .AddInMemoryClients(Config.GetClients())
            .AddAspNetIdentity<UserEntity>()
            .AddProfileService<IdentityProfileService>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllHeaders",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });

            services.AddSingleton(Configuration);
            services.AddHttpContextAccessor();
            services.AddSingleton<IRequestContext, RequestContextAdapter>();

            services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();
            services.Replace(ServiceDescriptor.Scoped<IUserValidator<UserEntity>, CustomUserValidator<UserEntity>>());


            // Identity Server Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
            })
            .AddIdentityServerAuthentication(options =>
            {
                options.Authority = Configuration.GetSection("IdentityServer:Authority").Value;
                options.ApiName = Configuration.GetSection("IdentityServer:ApiName").Value;
                options.ApiSecret = Configuration.GetSection("IdentityServer:ApiSecret").Value;
                options.RequireHttpsMetadata = false;
            });

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });

            services.AddControllers();

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                //options.ApiVersionReader = ApiVersionReader.Combine(
                //    new MediaTypeApiVersionReader("version"),
                //    new HeaderApiVersionReader("x-api-version")
                //);
                options.ReportApiVersions = true;
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Dams Auth Api's",
                    Version = "v1"
                });

                c.SwaggerDoc("v2", new OpenApiInfo
                {
                    Title = "Dams Auth Api's",
                    Version = "v2"
                });

                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.FirstOrDefault());
                c.OperationFilter<RemoveVersionFromParameter>();
                c.DocumentFilter<ReplaceVersionWithExactValueInPath>();
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert (Bearer access_token) into value field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddSerilog();

            app.UseExceptionHandler(appBuilder =>
            {
                appBuilder.Use(async (context, next) =>
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    context.Response.Headers.Add("Access-Control-Allow-Headers", "*");
                    context.Response.Headers.Add("Access-Control-Allow-Method", "*");
                    var error = context.Features[typeof(IExceptionHandlerFeature)] as IExceptionHandlerFeature;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new { status = "error", message = (error != null && error.Error != null) ? error.Error?.Message : "Unable to process request." }));
                });

            });

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "v1");
                c.SwaggerEndpoint("v2/swagger.json", "v2");
            });

            app.UseMiddleware<SerilogMiddleware>();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("AllowAllHeaders");
            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

    }
}
