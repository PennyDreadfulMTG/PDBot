using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.API
{
    partial class Gatherling
    {
        public class InfoBotSettings : ApplicationSettingsBase
        {
            [UserScopedSetting]
            public List<Server> Servers { get
                {
                    return this[nameof(Servers)] as List<Server>;
                }
                set
                {
                    this[nameof(Servers)] = value;
                }
            }

            public Server GetServer(string host)
            {
                if (Servers == null)
                    Servers = new List<Server>();
                var val = Servers.SingleOrDefault(s => s.Host == host);
                if (val == null)
                {
                    this.Servers.Add(new Server { Host = host, Passkey = "" });
                    Save();
                }
                return val;
            }
            public class Server
            {
                public string Host { get; set; }
                public string Passkey { get; set; }
            }
        }

        public class Deck
        {
            [JsonProperty("id")]
            public int Id { get; set; }
            [JsonProperty("found")]
            public bool Found { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("archetype")]
            public string Archetype { get; set; }
        }
    }
}
