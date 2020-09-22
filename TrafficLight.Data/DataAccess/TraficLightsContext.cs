using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TrafficLights.Model;
using TrafficLights.Model.Entities;

namespace TrafficLights.Data.DataAccess
{
    public class TraficLightsContext : IdentityDbContext<UserIdentityEntity>
    {
        public TraficLightsContext(DbContextOptions<TraficLightsContext> options) : base(options)
        {
       //     Database.EnsureCreated();
        }
        public DbSet<TrafficLightEntity> Lights { get; set; }

      //  public DbSet<User> Users { get; set; }
    //    public DbSet<UserIdentityEntity> Users { get; set; }

        /* protected override void OnModelCreating(ModelBuilder modelBuilder)
         {
             modelBuilder.Entity<TrafficLightEntity>().ToTable("TraficLights");
             modelBuilder.Entity<User>().ToTable("User");
         }*/
    }
}
