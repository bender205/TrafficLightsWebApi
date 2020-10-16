using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TrafficLights.Shared.Models;
using TrafficLights.WorkerService.Proto;

namespace Notifications.Models
{
    public class TrafficLightHubEntity
    {
        
        public int Id { get; set; }
        public Color Color { get; set; } = Color.Red;
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "0:yyyy\\MM\\dd HH:mm")]
        public DateTime? Date { get; set; } /*= DateTime.UtcNow;*/
    }
}
