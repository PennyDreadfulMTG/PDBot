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
            if (GatherlingClient.PasskeyProvider == null)
                GatherlingClient.PasskeyProvider = new InfoBotSettings();
        }

        private Dictionary<string, Event> _activeEvents { get; } = new Dictionary<string, Event>();
        private Dictionary<string, Round> _activeRounds { get; } = new Dictionary<string, Round>();

        Dictionary<Event, Round> ITournamentManager.ActiveEvents
        {
            get
            {
                var ret = new Dictionary<Event, Round>();
                foreach (var key in _activeEvents.Keys)
                {
                    ret.Add(_activeEvents[key], _activeRounds[key]);
                }
                return ret;
            }
        }

        public IChatDispatcher Chat { get { if (chatDispatcher == null) chatDispatcher = Resolver.Helpers.GetChatDispatcher(); return chatDispatcher; } }

        public List<IMatch> ActiveMatches { get; } = new List<IMatch>();

        public Task EveryHourAsync()
        {
            return Task.FromResult(false);
        }

        public async Task EveryMinuteAsync()
        {
            foreach (var m in ActiveMatches.ToArray())
            {
                if (m.Completed)
                    ActiveMatches.Remove(m);
            }
            var events = await GatherlingClient.GatherlingDotCom.GetActiveEventsAsync();
            events = events.Union(await GatherlingClient.One.GetActiveEventsAsync()).ToArray();

            foreach (var ae in events)
            {
                _activeEvents[ae.Name] = ae;
                if (!_activeRounds.ContainsKey(ae.Name))
                {
                    _activeRounds[ae.Name] = new Round();
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
                if (round.RoundNum > _activeRounds[ae.Name].RoundNum)
                {
                    await PostPairingsAsync(ae, round);
                }
                _activeRounds[ae.Name] = round;
            }

            foreach (var cachedEvent in _activeEvents.ToArray())
            {
                if (!events.Contains(cachedEvent.Value))
                {
                    _activeEvents.Remove(cachedEvent.Key);
                }
            }
        }

        private async Task PostPairingsAsync(Event eventModel, Round round)
        {
            if (!Features.AnnouncePairings)
                return;

            var room = eventModel.Channel;
            if (string.IsNullOrWhiteSpace(room) || string.IsNullOrWhiteSpace(room.Trim('#')))
            {
                Console.WriteLine($"No MTGO room defined for {eventModel}.");
                return;
            }
            else
            {
                var builder = new StringBuilder();
                if (round.RoundNum == 1 &&  !round.IsFinals && !Features.PublishResults)
                {
                    builder.AppendLine("[sF] Due to the spectator switcheroo bug, PDBot cannot trust the results it sees on screen.");
                    builder.AppendLine("[sF] PDBot will not be reporting match results to the channel until this bug is fixed.");
                    builder.AppendLine("[sF] If you spectate any other player's matches in the tournament," +
                                       " please keep in mind that player names could be attached to the wrong players.");
                }

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
                    if (eventModel.Series.Contains("Penny Dreadful"))
                    {
                        pairing.CalculateRes();
                        var A = await DiscordFunctions.MentionOrElseNameAsync(pairing.A);
                        var B = await DiscordFunctions.MentionOrElseNameAsync(pairing.B);
                        if (pairing.Res == "BYE")
                            builder.Append($"{A} has the BYE!");
                        else
                        {
                            builder.Append($"{A} {pairing.Res} {B}");
                        }
                    }
                    else
                    {
                        builder.Append(pairing.ToString());
                    }
                    builder.Append("\n");
                }
                if (misses == 0 && !round.IsFinals)
                {
                    var minutes = (DateTime.UtcNow.Minute + 11) % 60;
                    builder.AppendLine($"[sB] No-Show win time: XX:{minutes.ToString("D2")}");
                }
                builder.Append("[sD] Good luck, everyone!");

                string doorPrize = null;
                if (eventModel.Series.Contains("Penny Dreadful"))
                {
                    if (eventModel.Rounds.ContainsKey(round.RoundNum - 1))
                    {
                        var prev = eventModel.Rounds[round.RoundNum - 1];
                        if (round.IsFinals && !prev.IsFinals && round.Players.Count() == 8)
                        {
                            var top8players = round.Players.ToArray();
                            var eligible = prev.Players.Where(p => !top8players.Contains(p)).ToArray();
                            var winner = eligible[new Random().Next(eligible.Count())];
                            doorPrize = $"[sEventTicket] And the Door Prize goes to...\n [sEventTicket] {winner} [sEventTicket]";
                        }
                    }
                }

                if (misses < 3)
                {
                    var sent = Chat.SendPM(room, builder.ToString());
                    if (!string.IsNullOrEmpty(doorPrize))
                    {
                        Chat.SendPM(room, doorPrize);
                    }
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
