using System;
using System.Collections.Generic;
using System.Text;
using TrafficLights.Model.Interfaces;

namespace TrafficLights.Model
{
    public class TrafficLight : ITrafficLight
    {
        public int Id { get; set; } = 0;
        public Colors Color { get; set; } = Colors.Red;
        public bool IsSwitchingDown { get; set; } = true;
        public DateTime? Date { get; set; } = DateTime.UtcNow;
        public TrafficLight()
        {
        }

    }
}
