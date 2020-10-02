using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TrafficLights.Model
{
    public class TrafficLightByIdRequest
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public int Id  {  get;  set;  }
    }
}
