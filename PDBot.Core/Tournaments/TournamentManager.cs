using Gatherling;
using Gatherling.Models;
using PDBot.API;
using PDBot.Core.Interfaces;
using PDBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.Tournaments
{
    public class TournamentManager : ICronObject, ITournamentManager
    {

        private IChatDispatcher chatDispatcher;

        public TournamentManager()
        {
            GatherlingClient.PasskeyProvider = new InfoBotSettings();
        }

        public Dictionary<Event, Round> ActiveEvents { get; } = new Dictionary<Event, Round>();
        public IChatDispatcher Chat { get { if (chatDispatcher == null) chatDispatcher = Resolver.Helpers.GetChatDispatcher(); return chatDispatcher; } }

        public async Task EveryHour()
        {
        }

        public async Task EveryMinute()
        {
            var events = await GatherlingClient.GatherlingDotCom.GetActiveEventsAsync();
            try
            {
                //events = events.Union(await GatherlingClient.PennyDreadful.GetActiveEventsAsync()).ToArray();
            }
#pragma warning disable CC0004 // Catch block cannot be empty
            catch (WebException)
            {

            }
#pragma warning restore CC0004 // Catch block cannot be empty
            foreach (var ae in events)
            {
                if (!ActiveEvents.ContainsKey(ae))
                {
                    ActiveEvents.Add(ae, new Round());
                }
                Round round;
                try
                {
                    round = await ae.GetCurrentPairings().ConfigureAwait(false);
                }
                catch (Exception c)
                {
                    throw new WebException($"Error retrieving {ae} round.", c);
                }
                if (round == null)
                {
                    Console.WriteLine($"No active round for {ae}?");
                    continue;
                }
                if (round.RoundNum > ActiveEvents[ae].RoundNum)
                {
                    ActiveEvents[ae] = round;
                    await PostPairingsAsync(ae, round);
                }
            }
        }

        private async Task PostPairingsAsync(Event eventModel, Round round)
        {
            var room = eventModel.Channel;
            if (room == null)
            {
                Console.WriteLine($"No MTGO room defined for {eventModel}.");
                return;
            }
            else
            {
                var builder = new StringBuilder();
                if (round.IsFinals && round.Matches.Count == 1)
                    builder.Append($"[sD] Pairings for Finals:\n");
                else if (round.IsFinals)
                    builder.Append($"[sD] Pairings for Top {round.Matches.Count * 2}:\n");
                else
                    builder.Append($"[sD] Pairings for Round {round.RoundNum}:\n");
                var misses = 0;
                foreach (var pairing in round.Matches)
                {
                    if (pairing.A == pairing.B)
                    {
                        builder.Append("[sG] ");
                    }
                    else if (pairing.Verification == "verified")
                    {
                        misses += 1;
                        builder.Append("[sT] ");
                    }
                    else if (pairing.Verification == "unverified")
                    {
                        builder.Append("[sR] ");
                    }
                    else if (pairing.Res == "vs.")
                    {
                        builder.Append("[sR] ");
                    }
                    else
                    {
                        misses += 1;
                        builder.Append("[sT] ");
                    }
                    builder.Append(pairing.ToString());
                    builder.Append("\n");
                }
                if (misses == 0 && !round.IsFinals)
                {
                    var minutes = (DateTime.UtcNow.Minute + 11) % 60;
                    builder.AppendLine($"[sB] No-Show win time: XX:{minutes.ToString("D2")}");
                }
                builder.Append("[sD] Good luck, everyone!");
                if (misses < 3)
                {
                    var sent = Chat.SendPM(room, builder.ToString());
                    if (!sent)
                    {
                        Chat.Join(room);
                        await Task.Delay(TimeSpan.FromSeconds(10));
                        await PostPairingsAsync(eventModel, round);
                    }
                }
                // If misses >= 3, we have clearly just rebooted.  Don't send anything.
            }
        }
        public class InfoBotSettings : ApplicationSettingsBase, IPasskeyProvider
        {
            public InfoBotSettings()
            {
                var path = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
                Console.WriteLine($"Config path: {path}");
            }

            [ApplicationScopedSetting]
            public List<ServerSettings> Servers
            {
                get
                {
                    return this[nameof(Servers)] as List<ServerSettings>;
                }
                set
                {
                    this[nameof(Servers)] = value;
                }
            }

            public ServerSettings GetServer(string host)
            {
                if (Servers == null)
                    Servers = new List<ServerSettings>();
                var val = Servers.SingleOrDefault(s => s.Host == host);
                if (val == null)
                {
                    this.Servers.Add(val = new ServerSettings { Host = host, Passkey = "" });
                    Save();
                }
                return val;
            }
        }
    }
}
