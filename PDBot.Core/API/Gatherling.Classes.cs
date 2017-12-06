using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public class Round
        {
            public int RoundNum { get; private set; }

            public List<Pairing> Matches { get; } = new List<Pairing>();

            public static Round FromPaste(string[] lines)
            {
                var round = new Round();
                foreach (var line in lines)
                {
                    var m = Regex.Match(line, @"(?:/me )?Pairings for Round (?<round>\d+)", RegexOptions.Compiled);
                    if (m.Success)
                    {
                        round.RoundNum = int.Parse(m.Groups["round"].Value);
                    }
                    else if ((m = Regex.Match(line, @"^(?:/me )?(?<a>\w+) (?<res>vs.|\d-\d) (?<b>\w+)$", RegexOptions.Compiled)).Success)
                    {
                        var pairing = new Pairing
                        {
                            A = m.Groups["a"].Value,
                            B = m.Groups["b"].Value,
                            Res = m.Groups["res"].Value,
                        };
                        round.Matches.Add(pairing);
                    }
                    else if ((m = Regex.Match(line, @"^(?:/me )?(?<a>\w+) has the BYE$", RegexOptions.Compiled)).Success)
                    {
                        round.Matches.Add(new Pairing
                        {
                            A = m.Groups["a"].Value,
                            B = m.Groups["a"].Value,
                            Res = "BYE",
                        });
                    }
                    else if ((m = Regex.Match(line, @"^(?:/me )?Good luck everyone", RegexOptions.Compiled)).Success)
                    {
                        return round;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(line), line, "Unable to parse line.");
                    }
                }
                return round;
            }
        }

        public class Pairing
        {
            public string A { get; internal set; }
            public string B { get; internal set; }
            public string Res { get; internal set; }

            public override string ToString()
            {
                if (Res == "BYE")
                    return $"{A} has the BYE!";
                return $"{A} {Res} {B}";
            }
        }
    }
}
