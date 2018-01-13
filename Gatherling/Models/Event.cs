using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gatherling.Models
{
    public class Event
    {
        internal GatherlingClient Gatherling;

        [JsonProperty("name")]
        public string Name { get; set; }


        public string Channel { get; set; }

        public string Series { get; set; }

        public Task<Round> GetCurrentPairings()
        {
            return Gatherling.GetCurrentPairings(Name);
        }
    }
}
