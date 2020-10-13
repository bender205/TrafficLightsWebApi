using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using TrafficLights.Model.Entities;

namespace TrafficLights.Data.DataAccess
{
    public class TraficLightsContext : IdentityDbContext<UserIdentityEntity>
    {
        private readonly IPasswordHasher<UserIdentityEntity> _passwordHasher;
        public DbSet<TrafficLightEntity> Lights { get; set; }

        public TraficLightsContext(IServiceProvider services, DbContextOptions<TraficLightsContext> options) : base(options)
        {
            // passwordHasher
            _passwordHasher = services.GetRequiredService<IPasswordHasher<UserIdentityEntity>>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var users = new[]
            {
                new UserIdentityEntity
                {
                    Id = "46632e13-c099-4a0f-b418-60b0d0fbe775",
                    UserName = "user",
                    FirstName = "Petro",
                    LastName = "Ze"
                },
                new UserIdentityEntity
                {
                    Id = "46632e13-c099-4a0f-b418-60b0d0fbe776",
                    UserName = "admin",
                    FirstName = "Gordon",
                    LastName = "Freeman"
                },
            };

            users[0].PasswordHash = _passwordHasher.HashPassword(users[0], "user");
            users[1].PasswordHash = _passwordHasher.HashPassword(users[1], "admin");

            modelBuilder.Entity<UserIdentityEntity>().HasData(users);


            modelBuilder.Entity<IdentityRoleClaim<string>>().HasData(
                new List<IdentityRoleClaim<string>>()
                {
                    new IdentityRoleClaim<string>()
                    {
                        Id = 1,
                        RoleId = "1",
                        ClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                        ClaimValue = "user"
                    },
                    new IdentityRoleClaim<string>()
                    {
                        Id = 2,
                        RoleId = "2",
                        ClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                        ClaimValue = "admin"
                    },
                });

            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole()
                {
                    Id = "1",
                    Name = "user",
                    NormalizedName = "USER"
                },
                new IdentityRole()
                {
                    Id = "2",
                    Name = "admin",
                    NormalizedName = "ADMIN"
                }
            );

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>()
                {
                    RoleId = "1",
                    UserId = "46632e13-c099-4a0f-b418-60b0d0fbe775"
                },
                new IdentityUserRole<string>()
                {
                    RoleId = "2",
                    UserId = "46632e13-c099-4a0f-b418-60b0d0fbe776"
                }
            );
        }
    }
}
