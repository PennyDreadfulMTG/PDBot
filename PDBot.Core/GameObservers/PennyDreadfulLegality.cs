using Newtonsoft.Json.Linq;
using PDBot.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.GameObservers
{
    class PennyDreadfulLegality : IGameObserver
    {
        public static string[] LegalCards { get; private set; }

        public static List<string> Transforms { get; private set; }
        public static List<string> NotTransforms = new List<string>();

        public bool IsApplicable(string comment, MagicFormat format, Room room)
        {
            if (format == MagicFormat.PennyDreadful || format == MagicFormat.PennyDreadfulCommander)
            {
                return true;
            }
            return false;
        }

        public IGameObserver GetInstanceForMatch(IMatch match)
        {
            return new PennyDreadfulLegality();
        }

        List<string> warnings = new List<string>();

        public int IllegalCount { get; private set; }

        public string HandleLine(GameLogLine gameLogLine)
        {
            foreach (var name in gameLogLine.Cards)
            {
                if (warnings.Contains(name))
                {
                    // Already Acknowledged.
                }
                else if (!IsCardLegal(name))
                {
                    IllegalCount++;
                    int nWarnings = 3;
                    if (IllegalCount < nWarnings)
                    {
                        return $"[sR]{name}[sR] is not legal in Penny Dreadful.";
                    }
                    else if (IllegalCount == nWarnings)
                    {
                        return $"[sR]{name}[sR] is not legal in Penny Dreadful.\n" +
                                "[sG]In order to prevent further spamming, I'll stop warning you now.\n" +
                                "[sG]For more information about Penny Dreadful, see pdmtgo.com or reddit.com/r/PennyDreadfulMTG";
                    }

                    warnings.Add(name);
                }
            }
            return null;
        }

        private static bool IsCardLegal(string name)
        {
            if (LegalCards == null)
            {
                LegalCards = new WebClient().DownloadString("http://pdmtgo.com/legal_cards.txt").Split('\n');
                LegalCards = LegalCards.Select(n => new CardName(n)).SelectMany(cn => cn.Names).ToArray();
            }
            if (LegalCards.Contains(name, StringComparer.InvariantCultureIgnoreCase))
                return true;
            if (IsRearFace(name))
                return true;

            return false;
        }

        private static bool IsRearFace(string name)
        {
            if (Transforms == null)
            {
                Transforms = new List<string>();
                if (File.Exists("transforms.txt"))
                {
                    Transforms.AddRange(File.ReadAllLines("transforms.txt"));
                }

            }

            if (Transforms.Contains(name))
                return true;

            if (NotTransforms.Contains(name))
                return false;

            var url = $"https://api.scryfall.com/cards/named?exact={name}";
            var wc = new WebClient();
            try
            {
                var blob = wc.DownloadString(url);
                JObject json = Newtonsoft.Json.JsonConvert.DeserializeObject(blob) as JObject;
                if (json.Value<string>("layout") == "transform")
                {
                    if (!json.TryGetValue("mana_cost", out var cost) || string.IsNullOrEmpty(json.Value<string>("mana_cost")))
                    {
                        Transforms.Add(name);
                        File.AppendAllText("transforms.txt", name + '\n');
                        return true;
                    }
                    else
                    {
                        NotTransforms.Add(name);
                    }
                }
            }
            catch (WebException c)
            {
                NotTransforms.Add(name);
                Console.WriteLine(name);
                Console.WriteLine(c);
            }

            return false;

        }

        public void ProcessWinner(string winner, int gameID)
        {
        }
    }
}
