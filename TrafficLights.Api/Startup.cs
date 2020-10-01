using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using TrafficLights.Core;
using TrafficLights.Core.Hubs;
using TrafficLights.Data;
using TrafficLights.Data.DataAccess;
using TrafficLights.Model;
using TrafficLights.Model.Auth;
using TrafficLights.Model.Entities;
using TrafficLights.Model.Helpers;
using TrafficLights.WorkerService;

namespace TrafficLights.Api
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
            services.AddSignalR();
            services.AddDbContext<TraficLightsContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")),
                ServiceLifetime.Transient);
            services.AddScoped<TrafficLight>();
            services.AddScoped<TrafficLightRepository>();
            services.AddScoped<AuthRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPasswordHasher<RegisterRequest>, PasswordHasher<RegisterRequest>>();

            var identityBuilder = services.AddIdentity<UserIdentityEntity, IdentityRole>();
            identityBuilder.AddEntityFrameworkStores<TraficLightsContext>();
            



            services.AddMediatR(Assembly.GetExecutingAssembly(), Assembly.Load(("TrafficLights.Core")));
            services.AddSingleton<TrafficLightsService>();

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

            //services.AddMvc();

            services.AddHostedService<Worker>();


            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            var tokenAudienceSection = Configuration.GetSection("TokenOptions:Audience");

            var tokenIssuerSection = Configuration.GetSection("TokenOptions:Issuer");

            var certificate = new X509Certificate2(@"C:\Users\Developer\certs\mycert.pfx");
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
                    pattern: "{controller=TrafficLights}/{action=Index}/{id?}");
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapHub<TraficLightsHub>("/lighthub");
                });
            });
        }
    }
}
