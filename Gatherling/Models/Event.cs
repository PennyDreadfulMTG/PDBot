using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gatherling.Models
{
    public class Event : IEqualityComparer<Event>
    {
        internal IGatherlingApi Gatherling;

        [JsonProperty("name")]
        public string Name { get; set; }


        public string Channel { get; set; }

        public string Series { get; set; }

        public string Host { get; set; }

        public SubEvent Main { get; set; }

        public SubEvent Finals { get; set; }

        public string[] Unreported { get; set; }

        public Standing[] Standings;

        public Task<Round> GetCurrentPairingsAsync()
        {
            return Gatherling.GetCurrentPairings(this);
        }

        public Dictionary<int, Round> Rounds { get; } = new Dictionary<int, Round>();

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

        public bool Equals(Event x, Event y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(Event obj)
        {
            return obj.Name.GetHashCode();
        }

        public Event(IGatherlingApi api)
        {
            Gatherling = api ?? throw new ArgumentNullException(nameof(api));
        }

        public Event(string name, JObject data, IGatherlingApi api)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Event Name is null", nameof(name));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Gatherling = api ?? throw new ArgumentNullException(nameof(api));
            Name = name;
            Channel = data.Value<string>("mtgo_room");
            if (Channel != null && !Channel.StartsWith("#"))
                Channel = "#" + Channel;
            Series = data.Value<string>("series");
            Main = new SubEvent(data.Value<string>("mainstruct"), data.Value<int>("mainrounds"));
            Finals = new SubEvent(data.Value<string>("finalstruct"), data.Value<int>("finalrounds"));
            if (data.ContainsKey("unreported"))
                Unreported = ((JArray)data["unreported"]).Values<string>().ToArray();
            try
            {
                if (data.ContainsKey("standings"))
                {
                    var jArray = ((JArray)data["standings"]);
                    Standings = jArray.Select(t => ((JObject)t).ToObject<Standing>()).ToArray();
                }
            }
            catch (Exception c)
            {
                SentrySdk.WithScope(scope =>
                {
                    scope.SetExtra("event", data);
                    SentrySdk.CaptureException(c);
                });
            }
        }
    }
}
