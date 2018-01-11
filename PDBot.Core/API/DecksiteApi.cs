using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PDBot.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.API
{
    public static partial class DecksiteApi
    {
        private static string API_TOKEN = null;

        static DecksiteApi()
        {
            if (File.Exists("PDM_API_KEY.txt"))
                API_TOKEN = File.ReadAllText("PDM_API_KEY.txt");
            else
                API_TOKEN = "null";
        }

        public class Card
        {

            public Card(JToken blob)
            {
                try
                {
                    Name = new CardName(blob.SelectToken("names")?.Select(jt => jt.ToObject<string>()));
                }
                catch (Exception)
                {
                    Name = new CardName(blob.Value<string>("name"));
                }
                Quantity = blob.Value<int>("n");
            }

            public int Quantity { get; private set; }
            public CardName Name { get; private set; }
        }

        public class Deck
        {

            public Deck(JToken blob)
            {
                Id = blob.Value<int>("id");
                Name = blob.Value<string>("name");
                Person = blob.Value<string>("person");
                Wins = blob.Value<int>("wins");
                Losses = blob.Value<int>("losses");
                Draws = blob.Value<int>("draws");
                HasRecord = blob.Value<bool>("has_record");
                CanPlay = blob.SelectToken("can_play")?.Select(jt => jt.ToObject<string>())?.ToArray();
                MainBoard = blob.SelectToken("maindeck").Select(c => new Card(c)).ToArray();
                SideBoard = blob.SelectToken("sideboard").Select(c => new Card(c)).ToArray();
                CompetitionName = blob.Value<string>("competition_name");
            }

            public string Name { get; internal set; }
            public string Person { get; internal set; }
            public int Wins { get; internal set; }
            public int Losses { get; internal set; }
            public int Draws { get; internal set; }
            public string[] CanPlay { get; internal set; }
            public bool HasRecord { get; private set; }
            public Card[] MainBoard { get; private set; }
            public Card[] SideBoard { get; private set; }
            public string CompetitionName { get; private set; }
            public int Id { get; private set; }

            public override string ToString()
            {
                string str = $"{Name} by {Person}";
                if (HasRecord)
                {
                    str += $" ({Wins}-{Losses})";
                }
                return str;
            }

            public bool ContainsCard(string name)
            {
                return MainBoard.Any(c => c.Name.Equals(name)) || SideBoard.Any(c => c.Name.Equals(name));
            }

            public bool Retire()
            {
                var nameValueCollection = new NameValueCollection
                {
                    { "api_token", API_TOKEN }
                };
                var v = Encoding.UTF8.GetString(Api.UploadValues($"/api/league/drop/{Person}", nameValueCollection));
                File.WriteAllText("drop.json", v);
                var blob = JToken.Parse(v);
                if (blob.Type == JTokenType.Null)
                {
                    return false;
                }

                bool error = ((blob as JObject).TryGetValue("error", out var _));
                return !error;
            }
        }

        static WebClient Api => new WebClient()
        {
            BaseAddress = "https://pennydreadfulmagic.com/",
            Encoding = Encoding.UTF8
        };

        public static Deck GetRunSync(string player)
        {
            var task = GetRun(player);
            task.Wait();
            return task.Result;
        }

        public static async Task<Deck> GetRun(string player)
        {
            try
            {

                string v = await Api.DownloadStringTaskAsync($"/api/league/run/{player}");
                var blob = JToken.Parse(v);
                if (blob.Type == JTokenType.Null)
                {
                    return null;
                }

                Deck run = new Deck(blob);
                return run;
            }
            catch (WebException c)
            {
                //var stream = File.OpenWrite("leagueerror");
                using (var sw = new StreamWriter("leagueerror.txt"))
                {
                    using (Stream response = c.Response.GetResponseStream())
                    {
                        var bytes = new byte[response.Length];
                        response.Read(bytes, 0, (int)response.Length);
                        sw.Write(bytes);
                    }
                }
                throw;
            }
            catch (Exception c)
            {
                Console.WriteLine(c);
                return null;
            }
        }

        public static Deck GetDeck(int id)
        {
            var blob = Api.DownloadString($"/api/decks/{id}");
            JObject jObject = JObject.Parse(blob);
            if (jObject.Type == JTokenType.Null)
                return null;
            return new Deck(jObject);
        }

        public static void UploadResults(Deck winningRun, Deck losingRun, string record, int MatchID)
        {
            Api.UploadValues("/report/", new System.Collections.Specialized.NameValueCollection
            {
                { "api_token", API_TOKEN },
                { "entry", winningRun.Id.ToString() },
                { "opponent", losingRun.Id.ToString() },
                { "result", record },
                { "draws", "0" },
                { "matchID", MatchID.ToString() },
            });
        }

        public static bool LogUploaded(int id)
        {
            using (var api = Api)
            {
                var url = $"https://logs.pennydreadfulmagic.com/api/matchExists/{id}";
                return JsonConvert.DeserializeObject<bool>(api.DownloadString(url));
            }
        }

        public static void UploadLog(int id)
        {
            var lines = File.ReadAllText($"Logs/{id}.txt");
            using (var api = Api)
            {
                api.UploadValues("https://logs.pennydreadfulmagic.com/api/upload",
                    new NameValueCollection
                    {
                        { "api_token", API_TOKEN },
                        { "match_id", id.ToString() },
                        { "lines",  lines },
                    });
            }
        }

        public static Rotation GetRotation()
        {
            var blob = Api.DownloadString($"/api/rotation");
            return JsonConvert.DeserializeObject<Rotation>(blob);
        }

        public static async Task<Rotation> GetRotationAsync()
        {
            var blob = await Api.DownloadStringTaskAsync($"/api/rotation");
            return JsonConvert.DeserializeObject<Rotation>(blob);
        }

        public static IEnumerable<CardStat> PopularCards()
        {
            var blob = Api.DownloadString($"/api/cards");
            var jArray = JArray.Parse(blob);
            if (jArray.Type == JTokenType.Null)
                return null;
            return from c in jArray.Children() select JsonConvert.DeserializeObject<CardStat>(c.ToString());
        }
    }
}
