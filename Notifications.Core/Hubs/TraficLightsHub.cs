using MediatR;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Core.Hubs
{

    public class TraficLightsHub : Hub
    {
        private readonly IMediator _mediator;

        public TraficLightsHub(IMediator mediator)
        {
            _mediator = mediator;
        }
       
    }
}
