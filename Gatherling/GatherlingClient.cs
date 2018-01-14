using Gatherling.Models;
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

namespace Gatherling
{
    public partial class GatherlingClient
    {
        public static GatherlingClient GatherlingDotCom { get; } = new GatherlingClient("https://gatherling.com/");
        public static GatherlingClient PennyDreadful { get; } = new GatherlingClient("https://gatherling.pennydreadfulmagic.com/");
        public static GatherlingClient Pauper { get; } = new GatherlingClient("https://pdcmagic.com/gatherling/");
        public static GatherlingClient Localhost { get; } = new GatherlingClient("http://127.0.0.1/gatherling/", "xxxx");

        public ServerSettings Settings { get; }

        private GatherlingClient(string Hostname, string passkey = null)
        {
            cookies = new CookieContainer();
            Settings = new ServerSettings
            {
                Host = Hostname,
                Passkey = passkey
            };
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

        public async Task<Event[]> GetActiveEventsAsync()
        {
            if (ApiVersion >= 1)
            {
                var uri = new Uri(new Uri(Settings.Host), "player.php");
                var playerCP = new HtmlDocument();
                await Scrape(uri, playerCP);
                var tables = playerCP.DocumentNode.Descendants("table");
                var activeEvents = tables.First(t => t.Descendants("b").FirstOrDefault(b => b.InnerText.Trim() == "ACTIVE EVENTS") != null);
                var rows = activeEvents.Descendants("tr");
                var paths = rows.Select(tr => tr.Descendants("a").FirstOrDefault()?.Attributes["href"]?.Value);
                return paths.Where(n => n != null).Select(n => n.Replace("eventreport.php?event=", string.Empty)).Select(n => LoadEvent(n)).ToArray();
            }
            return await Task.FromResult(new Event[0]);
        }

        private async Task Scrape(Uri uri, HtmlDocument document)
        {
            using (var wc = CreateWebClient())
            {
                document.LoadHtml(await wc.DownloadStringTaskAsync(uri));
                if (document.PageRequiresLogin())
                {
                    await AuthenticateAsync().ConfigureAwait(false);
                    document.LoadHtml(await wc.DownloadStringTaskAsync(uri));
                    if (document.PageRequiresLogin())
                    {
                        throw new InvalidOperationException("Can't log in!");
                    }
                }
            }
        }

        public async Task<Round> GetCurrentPairings(string eventName)
        {
            if (ApiVersion >= 1)
            {
                var uri = new Uri(new Uri(Settings.Host), "event.php?view=match&name=" + eventName);
                var eventCP = new HtmlDocument();
                await Scrape(uri, eventCP);
                var paste = eventCP.DocumentNode.Descendants("code").FirstOrDefault();
                var lines = from l in paste.ChildNodes
                            where !string.IsNullOrWhiteSpace(l.InnerText)
                            select l.InnerText;
                return Round.FromPaste(lines.ToArray());
            }
            return null;
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

        public Event LoadEvent(string name)
        {
            return new Event
            {
                Name = name,
                Series = name.Trim(' ', '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'),
                Channel = RoomForSeries(name),
                Gatherling = this,
            };
        }

        /// <summary>
        /// Hack for v0 events.
        /// </summary>
        private static string RoomForSeries(string eventName)
        {
            var series = eventName.Trim(' ', '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            switch (series)
            {
                case "Penny Dreadful Thursdays":
                    return "#PDT";
                case "Penny Dreadful Saturdays":
                case "Penny Dreadful Sundays":
                    return "#PDS";
                case "Penny Dreadful Mondays":
                    return "#PDM";
                case "Classic Heirloom":
                    return "#heirloom";
                case "Community Legacy League":
                    return "#CLL";
                case "PauperPower":
                    return "#pauperpower";
                case "Modern Times":
                    return "#modern";
                case "Pauper Classic Tuesdays":
                    return "#pct";
                case "Vintage MTGO Swiss":
                    return "#vintageswiss";
                default:
                    break;
            }
            if (series.StartsWith("CLL Quarterly") || series.StartsWith("Community Legacy League"))
                return "#CLL";

            return null;
        }
    }
}
