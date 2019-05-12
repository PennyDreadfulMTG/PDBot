using Gatherling.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Gatherling.VersionedApis
{
    abstract class BaseApi : IGatherlingApi
    {
        private readonly CookieContainer cookies;
        public ServerSettings Settings { get; }

        public abstract int ApiVersion { get; }

        protected BaseApi(ServerSettings settings, CookieContainer cookies)
        {
            Settings = settings;
            this.cookies = cookies;
        }

        protected WebClient CreateWebClient()
        {
            var webClient = new CookieAwareWebClient(cookies)
            {
                BaseAddress = Settings.Host,
            };
            webClient.Headers[HttpRequestHeader.UserAgent] = "infobot/PDBot";
            return webClient;
        }

        public async Task AuthenticateAsync()
        {
            using (var wc = CreateWebClient())
            {
                var response = await wc.UploadValuesTaskAsync("login.php", new System.Collections.Specialized.NameValueCollection
                {
                    { "username", nameof(PDBot) },
                    { "password", Settings.Passkey },
                });
            }
        }

        public virtual Task<string> GetVerificationCodeAsync(string playerName)
        {
            return GetInfobotResponseAsync(playerName, "verify");
        }

        public virtual async Task<string> ResetPasswordAsync(string playerName)
        {
            var reset = await GetInfobotResponseAsync(playerName, "reset");
            return reset.Contains("verification code") ? "This feature is unavailable right now.  Please contact a site admin" : reset;
        }

        public virtual async Task<string> GetInfobotResponseAsync(string playerName, string mode) {
            var path = $"admin/infobot.php?passkey={Settings.Passkey}&username={playerName}&action={mode}";
            using (var webClient = CreateWebClient())
            {
                var resp = await webClient.DownloadStringTaskAsync(path).ConfigureAwait(false);
                return XDocument.Parse(resp).Root.Value;
            }
        }

        public abstract Task<Event[]> GetActiveEventsAsync();
        public abstract Task<Round> GetCurrentPairings(string eventName);
        public abstract Task<Round> GetCurrentPairings(Event tournament);
        public abstract Task<Standing[]> GetCurrentStandingsAsync(Event tournament);
        public abstract Task<Event> GetEvent(string name);

        public virtual async Task<Deck> GetDeckAsync(int deckID)
        {
            using (var wc = CreateWebClient())
            {
                return JsonConvert.DeserializeObject<Deck>(await wc.DownloadStringTaskAsync($"ajax.php?action=deckinfo&deck={deckID}").ConfigureAwait(false));
            }
        }
    }
}
