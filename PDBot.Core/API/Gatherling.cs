using Newtonsoft.Json;
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
        public static Gatherling Localhost { get; } = new Gatherling("https://127.0.0.1/gatherling/");

        public InfoBotSettings.Server Settings { get; }

        private Gatherling(string Hostname)
        {
            Settings = new InfoBotSettings().GetServer(Hostname);
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
            var webClient = new WebClient
            {
                BaseAddress = Settings.Host,
            };
            webClient.Headers[HttpRequestHeader.UserAgent] = "infobot/PDBot";
            return webClient;
        }
    }
}
