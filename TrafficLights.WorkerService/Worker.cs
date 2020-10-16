using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TrafficLights.Data;
using TrafficLights.Data.DataAccess;
using TrafficLights.Model;
using TrafficLights.Model.Entities;
using TrafficLights.Model.Interfaces;
using TrafficLights.Shared.Models;
using TrafficLights.WorkerService.Proto;

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
            
                // Todo is it a good practice to return null
                return this._activeTrafficLights .FirstOrDefault(t => t.Id == id);

        }

        public Task<bool> ContainTrafficLightByIdAsync(int id)
        {
            var result = _activeTrafficLights.Any(t => t.Id == id);
            return Task.FromResult(result);
        }

    }

    public class Worker : BackgroundService
    {
        #region Fields
        private readonly ILogger<Worker> _logger;

        private IConfiguration _configuration;
      //  private readonly IHubContext<TraficLightsHub> _hubContext;
      // private readonly GrpcChannel _channel;
        private readonly string _address;

        private readonly IServiceProvider  _serviceProvider;
        private readonly TrafficLightsService _trafficLightsService;
        #endregion
        #region Constructors
        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, TrafficLightsService trafficLightsService)
        {
           /* var gRpConfigurationSection = configuration.GetSection("gRPC");*/
           //this._configuration = configuration;
           //this._address = _configuration.GetValue<string>("Grpc:Address");
           _address = "https://notifications/";
            _logger = logger;
            _serviceProvider = serviceProvider;
            _trafficLightsService = trafficLightsService;
           
            
        }
        #endregion
        #region Methods

        public void SendNotification(TrafficLightEntity trafficLight)
        {
            
               using var channel = GrpcChannel.ForAddress(_address);
               var client = new NotificationService.NotificationServiceClient(channel);
            //"ReceiveColor", trafficLight.Id, trafficLight.Color, trafficLight.Date
            Color color;
            if (trafficLight.Color == Colors.Red)
            {
                color = Color.Red;
            }
            else if (trafficLight.Color == Colors.Yellow)
            {
                color = Color.Yellow;
            }
            else
            {
                color = Color.Green;
            }
            client.ChangeColor(new ChangeColorRequest(){Id = trafficLight.Id, Color = color, Date = trafficLight.Date.Value.ToTimestamp(), Method = "receivecolor" });

        }
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
                  SendNotification(hubLight);
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
