using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace PDBot.Core.API
{
    public static class BuggedCards
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

            public override string ToString()
            {
                return $"{CardName} - {Description}";
            }
        }

        public static List<Bug> Bugs { get; } = new List<Bug>();

        static DateTime LastUpdate;

        public static void CheckForNewList()
        {
            try
            {

                if (DateTime.Now.Subtract(LastUpdate).TotalHours > 1)
                {
                    using (WebClient wc = new WebClient())
                    {
                        var blob = wc.DownloadString("https://pennydreadfulmtg.github.io/modo-bugs/bugs.json");
                        Bugs.Clear();
                        Bugs.AddRange(JsonConvert.DeserializeObject<Bug[]>(blob));
                        LastUpdate = DateTime.Now;
                    }
                }
            }
            catch (WebException c)
            {
                Console.WriteLine($"Failed to update bugged cards\n{c}");
            }
        }

        public static Bug IsCardBugged(string CardName)
        {
            CheckForNewList();
            return Bugs.FirstOrDefault(n => n.CardName == CardName);
        }
    }
}
