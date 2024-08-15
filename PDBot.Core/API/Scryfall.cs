using Newtonsoft.Json.Linq;
using PDBot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.API
{
    public static class Scryfall
    {

        static Dictionary<string, Card> Cache { get; } = new Dictionary<string, Card>();
        static Dictionary<int, Card> IDCache { get; } = new Dictionary<int, Card>();

        public static Card GetCard(string name)
        {
            if (Cache.ContainsKey(name))
            {
                return Cache[name];
            }

            var address = $"cards/named?exact={name}";
            var card = HitAPI(address);
            return card;
        }

        public static Card GetCardFromCatID(int id)
        {
            if (IDCache.ContainsKey(id))
            {
                return IDCache[id];
            }

            System.Threading.Thread.Sleep(30);
            var address = $"cards/mtgo/{id}";
            var card = HitAPI(address);
            return card;
        }

        private static Card HitAPI(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            try
            {
                using var wc = new WebClient
                {
                    BaseAddress = "https://api.scryfall.com/",

                };
                wc.Headers[HttpRequestHeader.UserAgent] = "PDBot";
                var blob = wc.DownloadString(address);
                var json = Newtonsoft.Json.JsonConvert.DeserializeObject(blob) as JObject;
                return ParseJson(json);
            }
            catch (WebException)
            {
                return null;
            }
        }

        private static Card ParseJson(JObject json)
        {
            if (json.Value<string>("object") == "card")
            {
                var card = new Card(json);
                if (card.CatID != -1)
                {
                    IDCache[card.CatID] = card;
                }

                foreach (var name in card.Names)
                {
                    Cache[name] = card;
                }
                return card;
            }
            else
            {
                return null;
            }
        }

        private static IEnumerable<Card> HitMultiCardAPI(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            string blob;
            try
            {

                using var wc = new WebClient
                {
                    BaseAddress = "https://api.scryfall.com/"
                };
                wc.Headers[HttpRequestHeader.UserAgent] = "PDBot";
                Console.WriteLine(address);
                blob = wc.DownloadString(address);
            }
            catch (WebException)
            {
                yield break;
            }
            var json = Newtonsoft.Json.JsonConvert.DeserializeObject(blob) as JObject;
            if (json.Value<string>("object") == "list")
            {
                foreach (var jo in json["data"])
                {
                    var c = ParseJson(jo as JObject);
                    if (c != null)
                    {
                        yield return c;
                    }
                }
                if (json.Value<bool>("has_more"))
                {
                    foreach (var more in HitMultiCardAPI(json.Value<string>("next_page")))
                    {
                        yield return more;
                    }
                }
            }
            else
            {
                var c = ParseJson(json);
                if (c != null)
                    yield return c;
                else
                    yield break;
            }
        }
    }
}
