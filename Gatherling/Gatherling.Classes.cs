using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gatherling
{
    public class InfoBotSettings : ApplicationSettingsBase
    {
        [UserScopedSetting]
        public List<Server> Servers
        {
            get
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
                this.Servers.Add(val = new Server { Host = host, Passkey = "" });
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
}
