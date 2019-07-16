using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PDBot.Data;
using Sentry;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.API
{
    public static partial class DecksiteApi
    {
        private static readonly string API_TOKEN;

        static DateTime CachedRotationLastUpdate;
        static Rotation CachedRotation;


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

        public static async Task<Person> GetPersonAsync(string username)
        {
            try
            {

            using (var api = Api)
            {
                var blob = await api.GetStringAsync($"/api/person/{username}");
                return JsonConvert.DeserializeObject<Person>(blob);
            }
            }
            catch (WebException c) when (c.Status == WebExceptionStatus.ProtocolError && (c.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }
            catch (Exception c)
            {
                SentrySdk.CaptureException(c);
                return default;
            }
        }

        internal static object CurrentLeagueName()
        {
            return "this month's league"; // TODO: Provide an API for this on server.
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
                var str = $"{Name} by {Person}";
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

            public async Task<bool> RetireAsync()
            {
                var nameValueCollection = new FormUrlEncodedContent(new KeyValuePair<string, string>[] {
                    new KeyValuePair<string,string>("api_token",API_TOKEN)
                });
                var response = await Api.PostAsync($"/api/league/drop/{Person}", nameValueCollection).ConfigureAwait(false);
                var v = Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync());
                File.WriteAllText("drop.json", v);
                var blob = JToken.Parse(v);
                if (blob.Type == JTokenType.Null)
                {
                    return false;
                }

                var error = ((blob as JObject).TryGetValue("error", out var _));
                return !error;
            }
        }

        static HttpClient Api => new HttpClient
        {
            BaseAddress = new Uri("https://pennydreadfulmagic.com/"),

        };

        public static Deck GetRunSync(string player)
        {
            var task = GetRunAsync(player);
            task.Wait();
            return task.Result;
        }

        public static async Task<Deck> GetRunAsync(string player)
        {
            try
            {

                var v = await Api.GetStringAsync($"/api/league/run/{player}");
                var blob = JToken.Parse(v);
                if (blob.Type == JTokenType.Null)
                {
                    return null;
                }

                var run = new Deck(blob);
                return run;
            }
            catch (WebException c)
            {
                using (var sw = new StreamWriter("leagueerror.txt"))
                {
                    using (Stream response = c.Response.GetResponseStream())
                    {
                        var bytes = new byte[response.Length];
                        await response.ReadAsync(bytes, 0, (int)response.Length);
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

        public static async Task<Deck> GetDeckAsync(int id)
        {
            var blob = await Api.GetStringAsync($"/api/decks/{id}");
            var jObject = JObject.Parse(blob);
            if (jObject.Type == JTokenType.Null)
                return null;
            return new Deck(jObject);
        }

        public static async Task<bool> UploadResultsAsync(Deck winningRun, Deck losingRun, string record, int MatchID)
        {
            var nameValueCollection = new FormUrlEncodedContent(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("api_token", API_TOKEN),
                new KeyValuePair<string, string>("entry", winningRun.Id.ToString()),
                new KeyValuePair<string, string>("opponent", losingRun.Id.ToString()),
                new KeyValuePair<string, string>("result", record),
                new KeyValuePair<string, string>("draws", "0"),
                new KeyValuePair<string, string>("matchID", MatchID.ToString()),
            });
            var response = await Api.PostAsync("/report/", nameValueCollection);
            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> LogUploadedAsync(int id)
        {
            using (var api = Api)
            {
                var url = $"https://logs.pennydreadfulmagic.com/api/matchExists/{id}";
                return JsonConvert.DeserializeObject<bool>(await api.GetStringAsync(url));
            }
        }

        public static async Task UploadLogAsync(int id)
        {
            var f = $"Logs/{id}.txt";
            var lines = File.ReadAllText(f);
            using (var api = Api)
            {
                var keys = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("api_token", API_TOKEN),
                    new KeyValuePair<string, string>("match_id", id.ToString()),
                    new KeyValuePair<string, string>("start_time_utc", new DateTimeOffset(File.GetCreationTimeUtc(f)).ToUnixTimeSeconds().ToString()),
                    new KeyValuePair<string, string>("end_time_utc", new DateTimeOffset(File.GetLastWriteTimeUtc(f)).ToUnixTimeSeconds().ToString()),
                };
                //if (lines.Length < 200)
                    keys.Add(new KeyValuePair<string, string>(nameof(lines), lines));

                var formdata = new FormUrlEncodedContent(keys);
                await api.PostAsync("https://logs.pennydreadfulmagic.com/api/upload", formdata);
            }
        }

        public static async Task<Rotation> GetCachedRotationAsync()
        {
            if (DateTime.Now.Subtract(CachedRotationLastUpdate) > TimeSpan.FromMinutes(1))
            {
                try
                {
                    CachedRotation = await GetRotationAsync();
                    CachedRotationLastUpdate = DateTime.Now;
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to update rotation data.");
                }
            }
            return CachedRotation;
        }

        public static async Task<Rotation> GetRotationAsync()
        {
            var blob = await Api.GetStringAsync($"/api/rotation");
            return JsonConvert.DeserializeObject<Rotation>(blob);
        }

        public static async Task<IEnumerable<CardStat>> PopularCardsAsync()
        {
            var blob = await Api.GetStringAsync($"/api/cards");
            var jArray = JArray.Parse(blob);
            if (jArray.Type == JTokenType.Null)
                return null;
            return from c in jArray.Children() select JsonConvert.DeserializeObject<CardStat>(c.ToString());
        }
    }
}
