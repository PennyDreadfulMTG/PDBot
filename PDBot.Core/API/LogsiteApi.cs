using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace PDBot.Core.API
{
    public class LogsiteApi
    {
        public class StatsJson
        {
            [JsonProperty("formats")]
            public Dictionary<string, FormatStats> Formats { get; set; }
        }

        public class FormatStats
        {
            [JsonProperty("name")]
            public string Name { get; internal set; }
            [JsonProperty("num_matches")]
            public int NumMatches { get; set; }

            [JsonProperty("last_week")]
            public TimeframeStats LastWeek { get; set; }
            [JsonProperty("last_month")]
            public TimeframeStats LastMonth { get; set; }

        }
        public class TimeframeStats
        {
            [JsonProperty("num_matches")]
            public int NumMatches { get; set; }
            [JsonProperty("recent_players")]
            public string[] Players { get; set; }
        }
        static WebClient Api => new WebClient
        {
            BaseAddress = "https://logs.pennydreadfulmagic.com/",
            //BaseAddress = "http://localhost:5000/",
            Encoding = Encoding.UTF8
        };

        public static async System.Threading.Tasks.Task<StatsJson> GetStatsAsync()
        {
            using (var api = Api)
            {
                var v = await api.DownloadStringTaskAsync("/stats.json");
                return JsonConvert.DeserializeObject<StatsJson>(v);
            }
        }
    }
}
