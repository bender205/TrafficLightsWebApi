using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.SignalR;
using Notifications.Core.Hubs;
using Notifications.Models;
using TrafficLights.Shared.Models;
using TrafficLights.WorkerService.Proto;

namespace Notifications.Services
{
    public class NotificationService : TrafficLights.WorkerService.Proto.NotificationService.NotificationServiceBase
    {
        private readonly IHubContext<TraficLightsHub> _hubContext;
        public NotificationService(IHubContext<TraficLightsHub> hubContext)
        {
            this._hubContext = hubContext;
        }
        public override async Task<EmptyReply> ChangeColor(ChangeColorRequest request, ServerCallContext context)
        {
            
           
            TrafficLightHubEntity entity = new TrafficLightHubEntity(){Id = request.Id, Color = request.Color, Date = request.Date.ToDateTime()};
            await _hubContext.Clients.All.SendAsync(request.Method, entity);
            return new EmptyReply();
        }
    }
}
