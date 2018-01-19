using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Gatherling.Models
{
    public class Event
    {
        internal IGatherlingApi Gatherling;

        [JsonProperty("name")]
        public string Name { get; set; }


        public string Channel { get; set; }

        public string Series { get; set; }

        public Task<Round> GetCurrentPairings()
        {
            return Gatherling.GetCurrentPairings(Name);
        }

        public override string ToString()
        {
            return $"<{Name}>";
        }

        public override bool Equals(object obj)
        {
            if (obj is Event e)
                return e.Name == this.Name;
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public Event(IGatherlingApi api)
        {

        }

        public Event(string name, JObject data, IGatherlingApi api)
        {
            Gatherling = api;
            Name = name;
            Channel = data.Value<string>("mtgo_room");
            Series = data.Value<string>("series");
        }

    }
}
