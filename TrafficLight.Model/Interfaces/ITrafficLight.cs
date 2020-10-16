using System;
using System.Collections.Generic;
using System.Text;
using TrafficLights.Shared.Models;

namespace TrafficLights.Model.Interfaces
{
    
    public interface ITrafficLight
    {
        int Id { get; set; }
        public Colors Color { get; set; }
        public DateTime? Date { get; set; }
    }
}
