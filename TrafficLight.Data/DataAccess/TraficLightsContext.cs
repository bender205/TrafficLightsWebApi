using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using TrafficLights.Model.Entities;

namespace TrafficLights.Data.DataAccess
{
    public class TraficLightsContext : DbContext
    {
        public DbSet<TrafficLightEntity> Lights { get; set; }

        public TraficLightsContext(DbContextOptions<TraficLightsContext> options) : base(options)
        {
            
        }
    }
}
