using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Gatherling.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Gatherling.VersionedApis
{
    internal class V2 : V1
    {
        public V2(ServerSettings settings, CookieContainer cookies) : base(settings, cookies)
        {
        }

        public override int ApiVersion => 2;

        public override async Task<Event[]> GetActiveEventsAsync()
        {
            using (var api = CreateWebClient())
            {
                var json = JContainer.Parse(await api.DownloadStringTaskAsync("/ajax.php?action=active_events"));
                if (json.Type == JTokenType.Object)
                {
                    var dict = json as JObject;

                    var events = new List<Event>();
                    foreach (var item in dict)
                    {
                        events.Add(LoadEvent(item.Key, item.Value as JObject));
                    }
                    return events.ToArray();
                }
                return new Event[0];
            }
        }

        public override async Task<Round> GetCurrentPairings(string eventName)
        {
            using (var api = CreateWebClient())
            {
                var json = JObject.Parse(await api.DownloadStringTaskAsync("/ajax.php?action=active_events"));
                json = json[eventName] as JObject;
                return Round.FromJson(json["matches"] as JArray);
            }
        }

        private Event LoadEvent(string key, JObject value)
        {
            Event @event = new Event(key, value, this);
            try
            {

            if (@event.Channel == null)
            {
                @event.Channel = RoomForSeries(@event.Series);
            }
            }
            catch (Exception c)
            {
                Console.WriteLine(c);
            }
            return @event;
        }
    }
}
