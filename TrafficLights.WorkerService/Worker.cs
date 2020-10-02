using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TrafficLights.Core.Hubs;
using TrafficLights.Data;
using TrafficLights.Data.DataAccess;
using TrafficLights.Model;
using TrafficLights.Model.Entities;
using TrafficLights.Model.Interfaces;

namespace TrafficLights.WorkerService
{
    public class TrafficLightsService
    {
        object  lockObject = new object();
        public List<TrafficLight> _activeTrafficLights = new List<TrafficLight>();

        public void AddTrafficLight(TrafficLight trafficLight)
        {
            if (!_activeTrafficLights.Any(t => t.Id == trafficLight.Id))
            {
                lock (lockObject)
                {
                    if (!_activeTrafficLights.Any(t => t.Id == trafficLight.Id))
                    {
                        _activeTrafficLights.Add(trafficLight);
                    }
                }
            }
        }
        public async Task<TrafficLight> GetTrafficLightByIdAsync(int id)
        {
            return await Task.Run(
                () =>
            {
                // Todo is it a good practice to return null
                return this._activeTrafficLights.Where(t => t.Id == id) .FirstOrDefault();
            });
            
        }

        public async Task<bool> ContainTrafficLightByIdAsync(int id)
        {
           return  await Task.Run(() =>
            {
                return this._activeTrafficLights.Any(t => t.Id == id);
            });
        }

    }

    public class Worker : BackgroundService
    {
        #region Fields
        private readonly ILogger<Worker> _logger;

        private readonly IHubContext<TraficLightsHub> _hubContext;

        private readonly IServiceProvider  _serviceProvider;
        private readonly TrafficLightsService _trafficLightsService;
        #endregion
        #region Constructors
        public Worker(ILogger<Worker> logger, IHubContext<TraficLightsHub> hubContext, IServiceProvider serviceProvider, TrafficLightsService trafficLightsService)
        {
            _logger = logger;
            _hubContext = hubContext;
            _serviceProvider = serviceProvider;
            _trafficLightsService = trafficLightsService;
        }
        #endregion
        #region Methods
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var trafficLight in _trafficLightsService._activeTrafficLights)
                {
                    _logger.LogInformation($"traffic light {trafficLight.Id} changed color to {trafficLight.Color}, date: {trafficLight.Date} ! \n");
                    await SwitchNextColor(trafficLight);
                    await SaveTrafficLightToDBAsync(trafficLight, _serviceProvider, CancellationToken.None);
                }
                await Task.Delay(2000, stoppingToken);
            }
        }

        public async Task SwitchNextColor(TrafficLight currentTrafficLight)
        {
            if (currentTrafficLight != null)
            {
                if (currentTrafficLight.Color == Colors.Red)
                {
                    currentTrafficLight.Color = Colors.Yellow;
                    currentTrafficLight.IsSwitchingDown = true;
                }
                else if (currentTrafficLight.Color == Colors.Yellow && currentTrafficLight.IsSwitchingDown)
                {
                    currentTrafficLight.Color = Colors.Green;
                    currentTrafficLight.IsSwitchingDown = false;
                }
                else if (currentTrafficLight.Color == Colors.Yellow && !currentTrafficLight.IsSwitchingDown)
                {
                    currentTrafficLight.Color = Colors.Red;
                }
                else if (currentTrafficLight.Color == Colors.Green)
                {
                    currentTrafficLight.Color = Colors.Yellow;
                }
                await ChangeDateTimeToCurrent(currentTrafficLight);
                try
                {//TODO fix sending params
                    TrafficLightEntity hubLight = new TrafficLightEntity() { Id = currentTrafficLight.Id, Color = currentTrafficLight.Color, Date = currentTrafficLight.Date };
                    await _hubContext.Clients.All.SendAsync("ReceiveColor", hubLight);
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        private async Task ChangeDateTimeToCurrent(ITrafficLight trafficLight)
        {
            await Task.Run(() =>
            {
                trafficLight.Date = DateTime.UtcNow;
            });
        }
        private async Task SaveTrafficLightToDBAsync(TrafficLight currentTrafficLight, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            try
            {
                var services = serviceProvider.CreateScope().ServiceProvider;
                var repository = services.GetRequiredService<TrafficLightRepository>();
                var lightsContext = services.GetRequiredService<TraficLightsContext>();
                var firstTraficLight = lightsContext.Lights.Where(l => l.Id == currentTrafficLight.Id).FirstOrDefault();
                firstTraficLight.Color = currentTrafficLight.Color;
                firstTraficLight.Date = DateTime.Now;
                await lightsContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion
    }
}
