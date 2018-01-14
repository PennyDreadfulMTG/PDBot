using Gatherling.Models;
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
        public List<ServerSettings> Servers
        {
            get
            {
                return this[nameof(Servers)] as List<ServerSettings>;
            }
            set
            {
                this[nameof(Servers)] = value;
            }
        }

        public ServerSettings GetServer(string host)
        {
            if (Servers == null)
                Servers = new List<ServerSettings>();
            var val = Servers.SingleOrDefault(s => s.Host == host);
            if (val == null)
            {
                this.Servers.Add(val = new ServerSettings { Host = host, Passkey = "" });
                Save();
            }
            return val;
        }

    }
}
