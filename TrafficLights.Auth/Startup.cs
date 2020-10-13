using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TrafficLights.Core;
using TrafficLights.Data;
using TrafficLights.Data.DataAccess;
using TrafficLights.Model.Auth;
using TrafficLights.Model.Entities;
using TrafficLights.Model.Helpers;

namespace TrafficLights.Auth
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TraficLightsContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")),
                ServiceLifetime.Transient);

            services.AddScoped<AuthRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPasswordHasher<RegisterRequest>, PasswordHasher<RegisterRequest>>();

            var identityBuilder = services.AddIdentity<UserIdentityEntity, IdentityRole>();
            identityBuilder.AddEntityFrameworkStores<TraficLightsContext>();

            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                  builder =>
                                  {
                                      builder.WithOrigins("http://localhost:4200")
                                      .AllowAnyMethod()
                                        .AllowAnyHeader()
                                        .AllowCredentials();
                                  });
            });

            services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.IgnoreNullValues = true);

           /* 
            services.AddSingleton<Worker>();
            services.AddHostedService<Worker>(provider => provider.GetService<Worker>());*/

            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            var tokenAudienceSection = Configuration.GetSection("TokenOptions:Audience");

            var tokenIssuerSection = Configuration.GetSection("TokenOptions:Issuer");
            var certificatePath = Configuration.GetSection("SertificatePath:Certificate").Value;
            var certificate = new X509Certificate2(certificatePath);
            var securityKey = new X509SecurityKey(certificate);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = tokenIssuerSection.Value,
                        ValidateAudience = true,
                        ValidAudience = tokenAudienceSection.Value,
                        ValidateLifetime = true,
                        IssuerSigningKey = securityKey
                    };
                });

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("OnlyForAdmin", policy =>
                {
                    // policy.RequireClaim("Role", "admin");
                    policy.RequireRole("admin", "Admin");
                });
                options.AddPolicy("OnlyForUser", policy =>
                {
                    policy.RequireRole("user", "User");
                    /*policy.RequireClaim("role", "user");*/
                });
            });

        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(MyAllowSpecificOrigins);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Users}/{action=login}/{id?}");
              
            });
        }
    }
}
