using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PDBot.API.GatherlingExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PDBot.API
{
    public partial class Gatherling
    {
        public static Gatherling GatherlingDotCom { get; } = new Gatherling("https://gatherling.com/");
        public static Gatherling PennyDreadful { get; } = new Gatherling("https://gatherling.pennydreadfulmagic.com/");
        public static Gatherling Pauper { get; } = new Gatherling("https://pdcmagic.com/gatherling/");
        public static Gatherling Localhost { get; } = new Gatherling("http://127.0.0.1/gatherling/", "xxxx");

        public InfoBotSettings.Server Settings { get; }

        private Gatherling(string Hostname, string passkey = null)
        {
            Settings = new InfoBotSettings().GetServer(Hostname);
            cookies = new CookieContainer();
            if (string.IsNullOrEmpty(Settings.Passkey) && !string.IsNullOrEmpty(passkey))
            {
                Settings.Passkey = passkey;
            }
        }

        private readonly CookieContainer cookies;

        private int _apiVersion;
        public int ApiVersion
        {
            get
            {
                if (_apiVersion == 0)
                {
                    try
                    {
                        var jo = JObject.Parse(CreateWebClient().DownloadString("ajax.php?action=api_version"));
                        _apiVersion = jo["version"].Value<int>();
                    }
                    catch (WebException c)
                    {
                        if (c.Status == WebExceptionStatus.ProtocolError)
                        {
                            _apiVersion = 1;
                        }
                    }
                }
                return _apiVersion;
            }
        }

        public async Task<string[]> GetActiveEventsAsync()
        {
            if (ApiVersion >= 1)
            {
                var uri = new Uri(new Uri(Settings.Host), "player.php");
                var playerCP = new HtmlDocument();
                using (var wc = CreateWebClient())
                {
                    playerCP.LoadHtml(await wc.DownloadStringTaskAsync(uri));
                    if (playerCP.PageRequiresLogin())
                    {
                        await AuthenticateAsync().ConfigureAwait(false);
                        playerCP.LoadHtml(await wc.DownloadStringTaskAsync(uri));
                        if (playerCP.PageRequiresLogin())
                        {
                            throw new InvalidOperationException("Can't log in!");
                        }
                    }
                }
                var tables = playerCP.DocumentNode.Descendants("table");
                var activeEvents = tables.First(t => t.Descendants("b").FirstOrDefault(b => b.InnerText.Trim() == "ACTIVE EVENTS") != null);
                var rows = activeEvents.Descendants("tr");
                var paths = rows.Select(tr => tr.Descendants("a").FirstOrDefault()?.Attributes["href"]?.Value);
                return paths.Where(n => n != null).Select(n => n.Replace("eventreport.php?event=", string.Empty)).ToArray();
            }
            return await Task.FromResult(new string[0]);
        }


        public async Task GetCurrentPairings(string eventName)
        {
            if (ApiVersion >= 1)
            {

            }
        }

        public async Task AuthenticateAsync()
        {
            using (var wc = CreateWebClient())
            {
                var response = await wc.UploadValuesTaskAsync("login.php", new System.Collections.Specialized.NameValueCollection
                {
                    { "username", "PDBot" },
                    { "password", Settings.Passkey },
                });
            }
        }

        public async Task<Deck> GetDeckAsync(int deckID)
        {
            using (var wc = CreateWebClient())
            {
                return JsonConvert.DeserializeObject<Deck>(await wc.DownloadStringTaskAsync($"ajax.php?action=deckinfo&deck={deckID}").ConfigureAwait(false));
            }
        }

        public async Task<string> GetVerificationCodeAsync(string playerName)
        {
            var path = $"admin/infobot.php?passkey={Settings.Passkey}&username={playerName}";
            using (var webClient = CreateWebClient())
            {
                var resp = await webClient.DownloadStringTaskAsync(path).ConfigureAwait(false);
                return XDocument.Parse(resp).Root.Value;
            }
        }

        private WebClient CreateWebClient()
        {
            var webClient = new CookieAwareWebClient(cookies)
            {
                BaseAddress = Settings.Host,
            };
            webClient.Headers[HttpRequestHeader.UserAgent] = "infobot/PDBot";
            return webClient;
        }
    }
}
