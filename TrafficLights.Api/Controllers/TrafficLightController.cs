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
using TrafficLights.Shared.Models;
using TrafficLights.WorkerService;

namespace TrafficLights.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class TrafficLightController : ControllerBase
    {
        IServiceProvider Services { get; }
        private readonly TrafficLightsService _trafficLightsService;
        private readonly TrafficLightRepository _repository;
        private readonly Worker _trafficWorker;

        public TrafficLightController(IServiceProvider serviceProvider, TrafficLightsService trafficLightsService, Worker trafficWorker)
        {
            Services = serviceProvider.CreateScope().ServiceProvider;
            _repository = Services.GetRequiredService<TrafficLightRepository>();
            this._trafficLightsService = trafficLightsService;
            this._trafficWorker = trafficWorker;
        }

        // GET api/<TrafficLight>/5
        [Authorize]
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

                //TODO replace code below with Interface realization 
                await _repository.AddTrafficLightAsync(trafficLightById, CancellationToken.None);
                var trafficLightForService = new Model.TrafficLight()
                {
                    Id = trafficLightById.Id,
                    Color = trafficLightById.Color,
                    Date = trafficLightById.Date,
                    IsSwitchingDown = default
                };
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
                var trafficLightForService = new Model.TrafficLight() { Id = trafficLightById.Id, Color = trafficLightById.Color, Date = trafficLightById.Date, IsSwitchingDown = default };
                _trafficLightsService.AddTrafficLight(trafficLightForService);
                return trafficLightById;
            }
        }
        
        // PUT api/<TrafficLight>/5
        [Authorize(Roles = "admin", Policy = "OnlyForAdmin")]
        [HttpPut("nextcolor")]
        public async Task NextColor([FromQuery] TrafficLightByIdRequest trafficLightByIdRequest)
        {
            int id = trafficLightByIdRequest.Id;
            var containsTrafficLight = await this._trafficLightsService.ContainTrafficLightByIdAsync(id);
            // Todo we can remove containsTrafficLight and ContainTrafficLightByIdAsync call and add trafficLight null check
            if (containsTrafficLight)
            {
                var trafficLight = await this._trafficLightsService.GetTrafficLightByIdAsync(id);
                await this._trafficWorker.SwitchNextColor(trafficLight);
            }
        }

        // POST api/<TrafficLight>
        [HttpPost]
        public void Put([FromBody] string value)
        {
        }
        // DELETE api/<TrafficLight>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
