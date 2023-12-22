using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PDBot.Core.Data;
using PDBot.Core.Interfaces;
using PDBot.Data;
using Sentry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.GameObservers
{
    public abstract class BaseLegalityChecker
    {
        public abstract string FormatName { get; }

        public abstract string MoreInfo { get; }

        public string[] LegalCards { get; private set; }

        public static List<string> Transforms { get; private set; }

        protected abstract string LegalListUrl { get; }

        public static List<string> NotTransforms = new List<string>();

        public abstract bool ShouldJoin(IMatch match);

        public abstract Task<IGameObserver> GetInstanceForMatchAsync(IMatch match);

        readonly List<string> warnings = new List<string>();

        public int IllegalCount { get; private set; }

        public bool PreventReboot => warnings.Any();

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
                    warnings.Add(name);
                    IllegalCount++;
                    const int nWarnings = 3;
                    if (IllegalCount < nWarnings)
                    {
                        return $"[sR]{name}[sR] is not legal in {FormatName}.";
                    }
                    else if (IllegalCount == nWarnings)
                    {
                        return $"[sR]{name}[sR] is not legal in {FormatName}.\n" +
                                "[sG] In order to prevent further spamming, I'll stop warning you now.\n" +
                                (string.IsNullOrEmpty(MoreInfo) ? "" : $"[sG] For more information about {FormatName}, see {MoreInfo}");
                    }

                }
            }
            return null;
        }

        public bool IsCardLegal(string name)
        {
            if (LegalCards == null)
            {
                using var webClient = new WebClient();
                LegalCards = webClient.DownloadString(LegalListUrl).Split('\n');
                LegalCards = LegalCards.Select(n => new CardName(n)).SelectMany(cn => cn.Names).ToArray();
            }
            if (LegalCards.Contains(name, StringComparer.InvariantCultureIgnoreCase))
                return true;
            if (IsRearFace(name))
                return true;

            return false;
        }

        public static bool IsRearFace(string name)
        {
            if (Transforms == null)
            {
                Transforms = new List<string>();
                if (File.Exists("transforms.txt"))
                {
                    Transforms.AddRange(File.ReadAllLines("transforms.txt"));
                }
            }

            if (name == "Erayo, Soratami Ascendant's Essence")
                name = "Erayo's Essence";

            if (Transforms.Contains(name))
                return true;

            if (NotTransforms.Contains(name))
                return false;

            var url = $"https://api.scryfall.com/cards/named?exact={name}";
            using var wc = new WebClient();
            try
            {
                var blob = wc.DownloadString(url);
                var json = JsonConvert.DeserializeObject(blob) as JObject;
                JObject face;
                bool IsTransform = false;

                if (json.Value<string>("type_line") == "Dungeon")
                    IsTransform = true;

                switch (json.Value<string>("layout"))
                {
                    case "transform":
                    case "flip":
                        face = json["card_faces"].First(f => f.Value<string>(nameof(name)) == name) as JObject;
                        IsTransform = !face.TryGetValue("mana_cost", out var cost) || string.IsNullOrEmpty(face.Value<string>("mana_cost"));
                        break;
                    case "modal_dfc":
                        IsTransform = json["card_faces"].First().Value<string>(nameof(name)) != name;
                        break;
                    case "meld":
                        face = json;
                        IsTransform = !face.TryGetValue("mana_cost", out cost) || string.IsNullOrEmpty(face.Value<string>("mana_cost"));
                        break;
                    case "adventure":
                        face = json["card_faces"].First(f => f.Value<string>(nameof(name)) == name) as JObject;
                        IsTransform = face.Value<string>("type_line").Contains("Adventure");
                        break;
                    default:
                        if (!IsTransform)
                            return false;
                        break;
                }

                if (IsTransform)
                {
                    Transforms.Add(name);
                    try
                    {
                        File.AppendAllLines("transforms.txt", new string[] { name });
                    }
                    catch (IOException)
                    {

                    }
                    return true;
                }
                else
                {
                    NotTransforms.Add(name);
                    return false;
                }
            }
            catch (WebException c)
            {
                NotTransforms.Add(name);
                Console.WriteLine(name);
                SentrySdk.CaptureException(c);
            }

            return false;

        }

        public void ProcessWinner(string winner, int gameID)
        {
        }
    }
}
