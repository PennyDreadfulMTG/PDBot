using Gatherling.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PDBot.API.GatherlingExtensions;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Gatherling
{
    public partial class GatherlingClient : IGatherlingApi
    {
        private static IPasskeyProvider passkeyProvider;
        public static IPasskeyProvider PasskeyProvider {
            get => passkeyProvider ?? new DefaultPasskeyProvider();
            set
            {
                if (value == passkeyProvider)
                    return;
                passkeyProvider = value;
                GatherlingDotCom.Settings.Update(passkeyProvider);
                PennyDreadful.Settings.Update(passkeyProvider);
                //One.Settings.Update(passkeyProvider);
                Localhost.Settings.Update(passkeyProvider);
            }
        }

        public static GatherlingClient GatherlingDotCom { get; } = new GatherlingClient("https://gatherling.com/");
        public static GatherlingClient PennyDreadful { get; } = new GatherlingClient("https://gatherling.pennydreadfulmagic.com/");
        //public static GatherlingClient One { get; } = new GatherlingClient("https://gatherling.one/");
        public static GatherlingClient Localhost { get; } = new GatherlingClient("http://127.0.0.1/gatherling/", "xxxx");

        private IGatherlingApi api;
        private IGatherlingApi VersionedApi
        {
            get
            {
                if (api == null)
                {
                    switch (ApiVersion)
                    {
                        case -1:
                            throw new InvalidOperationException("Gatherling is not contactable");
                        case 1:
                            api = new VersionedApis.V1(Settings, cookies);
                            break;
                        case 2:
                        default:
                            api = new VersionedApis.V2(Settings, cookies);
                            break;
                    }
                }
                return api;
            }
        }

        public ServerSettings Settings { get; }

        private GatherlingClient(string Hostname, string passkey = null)
        {
            cookies = new CookieContainer();
            Settings = new ServerSettings
            {
                Host = Hostname,
                Passkey = passkey
            };
            passkey = PasskeyProvider.GetServer(Hostname).Passkey;
            if (string.IsNullOrEmpty(Settings.Passkey) && !string.IsNullOrEmpty(passkey))
            {
                Settings.Passkey = passkey;
            }
        }

        private readonly CookieContainer cookies;

        private WebClient CreateWebClient()
        {
            var webClient = new CookieAwareWebClient(cookies)
            {
                BaseAddress = Settings.Host,
            };
            webClient.Headers[HttpRequestHeader.UserAgent] = "infobot/PDBot";
            return webClient;
        }

        public Task AuthenticateAsync()
        {
            return VersionedApi.AuthenticateAsync();
        }

        public Task<Event[]> GetActiveEventsAsync()
        {
            return VersionedApi.GetActiveEventsAsync();
        }

        public Task<Event> GetEvent(string name)
        {
            return VersionedApi.GetEvent(name);
        }

        [Obsolete]
        public Task<Round> GetCurrentPairings(string eventName)
        {
            return VersionedApi.GetCurrentPairings(eventName);
        }

        public Task<Deck> GetDeckAsync(int deckID)
        {
            return VersionedApi.GetDeckAsync(deckID);
        }

        public Task<string> GetVerificationCodeAsync(string playerName)
        {
            return VersionedApi.GetVerificationCodeAsync(playerName);
        }

        public Task<Round> GetCurrentPairings(Event tournament)
        {
            return VersionedApi.GetCurrentPairings(tournament);
        }

        public Task<string> ResetPasswordAsync(string playerName)
        {
            return VersionedApi.ResetPasswordAsync(playerName);
        }

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
                        else if (c.Status == WebExceptionStatus.ConnectFailure)
                        {
                            _apiVersion = -1;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    catch (SocketException)
                    {
                        _apiVersion = -1;
                    }
                }
                return _apiVersion;
            }
        }






    }
}
