using System;
using System.Linq;
using Newtonsoft.Json;

namespace Weatherman.API.Geocoding
{
    public class GeoViewport
    {
        [JsonProperty("northeast")]
        public GeoCoordinate Northeast { get; set; }

        [JsonProperty("southwest")]
        public GeoCoordinate Southwest { get; set; }
    }
}
