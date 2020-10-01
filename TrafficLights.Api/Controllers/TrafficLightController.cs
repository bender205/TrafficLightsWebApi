using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TrafficLights.Data;
using TrafficLights.Model;
using TrafficLights.Model.Entities;
using TrafficLights.Model.Interfaces;
using TrafficLights.WorkerService;

namespace TrafficLights.Api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TrafficLightController : ControllerBase
    {
        IServiceProvider Services { get; }
        private readonly TrafficLightsService _trafficLightsService;
        private readonly TrafficLightRepository _repository;

        public TrafficLightController(IServiceProvider serviceProvider, TrafficLightsService trafficLightsService)
        {
            Services = serviceProvider.CreateScope().ServiceProvider;
            _repository = Services.GetRequiredService<TrafficLightRepository>();
            this._trafficLightsService = trafficLightsService;
        }

        // GET api/<TrafficLight>/5
        [HttpGet("{id}")]
        public async Task<ITrafficLight> Get(int id)
        {
            var trafficLightById = await _repository.GetByIdAsync(id, CancellationToken.None);

            if (trafficLightById == null)
            {
                trafficLightById = new TrafficLightEntity()
                {
                    Color = Colors.Red,
                    Date = DateTime.Now
                };

                //TODO replace code below with Interface Realization 
                await _repository.AddTrafficLightAsync(trafficLightById, CancellationToken.None);
                var trafficLightForService = new TrafficLight() { Id = trafficLightById.Id, Color = trafficLightById.Color, Date = trafficLightById.Date, IsSwitchingDown = default };
                _trafficLightsService.AddTrafficLight(trafficLightForService);

                return trafficLightForService;
            }
            var activeTrafficLightFromService = _trafficLightsService._activeTrafficLights.FirstOrDefault(t => t.Id == id);
            if (activeTrafficLightFromService != null)
            {
                return activeTrafficLightFromService;
            }
            else
            {
                var trafficLightForService = new TrafficLight() { Id = trafficLightById.Id, Color = trafficLightById.Color, Date = trafficLightById.Date, IsSwitchingDown = default };
                _trafficLightsService.AddTrafficLight(trafficLightForService);
                return trafficLightById;
            }
        }

        // POST api/<TrafficLight>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<TrafficLight>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<TrafficLight>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
