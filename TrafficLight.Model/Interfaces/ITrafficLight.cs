using System;
using System.Collections.Generic;
using System.Text;

namespace TrafficLights.Model.Interfaces
{
    public enum Colors
    {
        Red,
        Yellow,
        Green
    }
    public interface ITrafficLight
    {
        int Id { get; set; }
        public Colors Color { get; set; }
        public DateTime? Date { get; set; }
    }
}
