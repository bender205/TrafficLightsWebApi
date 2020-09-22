using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;
using TrafficLights.Model.Interfaces;

namespace TrafficLights.Model.Entities
{
    public class TrafficLightEntity : ITrafficLight
    {
        [Display(Name = "Id")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Color")]
        [MaxLength(10)]/*
        [JsonConverter(typeof(StringEnumConverter))]*/
        public Colors Color { get; set; } = Colors.Red;
        [Display(Name = "Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "0:yyyy\\MM\\dd HH:mm")]
        public DateTime? Date { get; set; } = DateTime.UtcNow;

    }
}
