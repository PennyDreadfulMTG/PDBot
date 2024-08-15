using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PDBot.Core.API
{
    public static partial class BuggedCards
    {
        public class Bug
        {
            [JsonProperty("card")]
            public string CardName { get; set; }
            [JsonProperty("category")]
            public string Classification { get; set; }
            [JsonProperty("description")]
            public string Description { get; set; }
            [JsonProperty("last_updated")]
            public string LastConfirmed { get; set; }
            [JsonProperty("multiplayer_only")]
            public bool Multiplayer { get; set; }
            [JsonProperty("help_wanted")]
            public bool HelpWanted { get; set; }
            [JsonProperty("url")]
            public string Url { get; set; }
            [JsonProperty("last_verified")]
            public string LastVerified { get; set; }

            public override string ToString()
            {
                return $"{CardName} - {Description}";
            }
        }

        public static List<Bug> Bugs { get; } = new List<Bug>();

        static DateTime LastUpdate;

        public static Bug[] CheckForNewList()
        {
            try
            {

                if (DateTime.Now.Subtract(LastUpdate).TotalHours > 1)
                {
                    using (WebClient wc = new WebClient())
                    {
                        var blob = wc.DownloadString("https://pennydreadfulmtg.github.io/modo-bugs/bugs.json");
                        lock (Bugs)
                        {
                            Bugs.Clear();
                            Bug[] bugarray = JsonConvert.DeserializeObject<Bug[]>(blob);
                            Bugs.AddRange(bugarray);
                            LastUpdate = DateTime.Now;
                            return bugarray;
                        }
                        
                    }
                }
            }
            catch (WebException c)
            {
                Console.WriteLine($"Failed to update bugged cards\n{c}");
            }
            return [.. Bugs];
        }

        public static Bug IsCardBugged(string CardName)
        {
            CheckForNewList();
            lock (Bugs)
            {
                return Bugs.FirstOrDefault(n => n.CardName == CardName);
            }
        }
    }
}
