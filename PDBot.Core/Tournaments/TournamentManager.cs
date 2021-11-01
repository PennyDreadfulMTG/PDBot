using Gatherling;
using Gatherling.Models;
using PDBot.API;
using PDBot.Core.Interfaces;
using PDBot.Discord;
using PDBot.Interfaces;
using Sentry;
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

            foreach (var ae in events)
            {
                try
                {
                    lock (_activeRounds)
                    {
                        _activeEvents[ae.Name] = ae;
                        if (!_activeRounds.ContainsKey(ae.Name))
                        {
                            _activeRounds[ae.Name] = new Round();
                        }
                    }

                    Round round;
                    try
                    {
                        round = await ae.GetCurrentPairingsAsync().ConfigureAwait(false);
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

                    var post = false;
                    lock (_activeRounds)
                    {
                        if (round.RoundNum > _activeRounds[ae.Name].RoundNum)
                        {
                            _activeRounds[ae.Name] = round;
                            post = true;
                        }
                        else if (ae.Series.Contains("Penny Dreadful"))
                        {
                            post = true;
                        }
                    }
                    ulong? ChanId = DiscordFunctions.GetDiscordChannel(ae);
                    bool overlap = events.Count(e => DiscordFunctions.GetDiscordChannel(ae) == ChanId) > 1;

                    await PostPairingsAsync(ae, round, post, ChanId, overlap);
                    lock (_activeRounds)
                    {
                        if (round.RoundNum == _activeRounds[ae.Name].RoundNum)
                        {
                            _activeRounds[ae.Name] = round;
                        }
                    }
                }
                catch (Exception c)
                {
                    SentrySdk.ConfigureScope(scope =>
                    {
                        scope.SetTag("Tournament", ae.Name);
                        SentrySdk.CaptureException(c);
                    });
                }
            }

            foreach (var cachedEvent in _activeEvents.ToArray())
            {
                if (!events.Contains(cachedEvent.Value))
                {
                    _activeEvents.Remove(cachedEvent.Key);
                }
            }
        }

        private async Task PostPairingsAsync(Event eventModel, Round round, bool toMTGO, ulong? chanId, bool overlap)
        {
            if (!Features.AnnouncePairings)
                return;

            var room = eventModel.Channel;

            if (!chanId.HasValue && (string.IsNullOrWhiteSpace(room) || string.IsNullOrWhiteSpace(room.Trim('#'))))
            {
                Console.WriteLine($"No MTGO room defined for {eventModel}.");
                return;
            }
            bool isPD = eventModel.Series.Contains("Penny Dreadful") && Features.ConnectToDiscord;

            var prebuilder = new StringBuilder();
            if (round.RoundNum == 1 && !round.IsFinals)
            {
                await RoundOneAnnouncements(eventModel, round, isPD, prebuilder, chanId);
            }
            var builder = new StringBuilder();
            var leagueMode = false;

            builder.AppendLine($"[sEventTicket] {eventModel.Name}");

            if (round.IsFinals && round.Matches.Count == 1)
                builder.Append($"[sD] Pairings for Finals:\n");
            else if (round.IsFinals)
                builder.Append($"[sD] Pairings for Top {round.Matches.Count * 2}:\n");
            else if (eventModel.Main.Mode == EventStructure.League)
                leagueMode = true;
            else
                builder.Append($"[sD] Pairings for Round {round.RoundNum}:\n");
            var misses = 0;
            if (leagueMode)
            {
                await PostLeagueInfoAsync(eventModel, round, builder, chanId);
            }
            else
            {
                misses = await PrintPairings(round, chanId, builder);
            }

            if (!round.IsFinals && (isPD || misses == 0) && eventModel.Main.Mode != EventStructure.League)
            {
                var minutes = FreeWinTime(eventModel.Name, round.RoundNum);
                builder.AppendLine($"[sB] No-Show win time: XX:{minutes:D2}");
            }
            builder.Append("[sD] Good luck, everyone!");

            string doorPrize = null;
            if (chanId.HasValue)
            {
                if (isPD && eventModel.Rounds.ContainsKey(round.RoundNum - 1))
                {
                    var prev = eventModel.Rounds[round.RoundNum - 1];
                    if (round.IsFinals && !prev.IsFinals && round.Players.Count() == 8 && !eventModel.Name.Contains("500"))
                    {
                        var top8players = round.Players.ToArray();
                        var eligible = prev.Players.Where(p => !top8players.Contains(p)).ToArray();
                        var winner = await DiscordFunctions.MentionOrElseNameAsync(eligible[new Random().Next(eligible.Count())]);
                        doorPrize = $"[sEventTicket] And the Door Prize goes to...\n [sEventTicket] {winner} [sEventTicket]";
                    }
                }

                await DiscordFunctions.PostTournamentPairingsAsync(chanId.Value, builder.ToString(), doorPrize, prebuilder.ToString());
            }
            else if (misses < 3 && toMTGO)
            {

                if (prebuilder.Length > 0)
                    Chat.SendPM(room, prebuilder.ToString());
                var sent = Chat.SendPM(room, builder.ToString());
                if (!string.IsNullOrEmpty(doorPrize))
                {
                    Chat.SendPM(room, doorPrize);
                }
                if (!sent)
                {
                    Chat.Join(room);
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    await PostPairingsAsync(eventModel, round, toMTGO, chanId, overlap);
                }
            }
            // If misses >= 3, we have clearly just rebooted.  Don't send anything.
        }

        private static async Task PostLeagueInfoAsync(Event eventModel, Round round, StringBuilder builder, ulong? chanId)
        {
            if (!chanId.HasValue)
                return;
            var guild = DiscordService.FindChannel(chanId.Value)?.Guild;
            if (eventModel.Main.Rounds == 1)
                builder.Append($"[sD] {eventModel.Name} is active.\n");
            else
                builder.Append($"[sD] League Round {round.RoundNum} is active.\n");
            builder.AppendLine("The following players still have matches remaining:");
            foreach (var player in eventModel.Players.Values)
            {
                int count = round.Matches.Count(m => m.PlayerA == player || m.PlayerB == player);
                if (count != 5)
                {
                    var name = await DiscordFunctions.MentionOrElseNameAsync(player, guild);
                    string line = $"{name}: {5 - count}";
                    builder.AppendLine(line);
                }
            }
        }

        private static async Task<int> PrintPairings(Round round, ulong? ChanId, StringBuilder builder)
        {
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
                if (ChanId.HasValue)
                {
                    pairing.CalculateRes();
                    var guild = DiscordService.FindChannel(ChanId.Value)?.Guild;

                    var A = await DiscordFunctions.MentionOrElseNameAsync(pairing.PlayerA, guild);
                    var B = await DiscordFunctions.MentionOrElseNameAsync(pairing.PlayerB, guild);
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

            return misses;
        }

        private static async Task RoundOneAnnouncements(Event eventModel, Round round, bool isPD, StringBuilder builder, ulong? chanId)
        {
            if (!Features.PublishResults)
            {
                builder.AppendLine("[sF] Due to the spectator switcheroo bug, PDBot cannot trust the results it sees on screen.");
                builder.AppendLine("[sF] PDBot will not be reporting match results to the channel until this bug is fixed.");
                builder.AppendLine("[sF] If you spectate any other player's matches in the tournament," +
                                    " please keep in mind that player names could be attached to the wrong players.");
            }
            builder.Append($"Welcome to {eventModel.Name}. We have {eventModel.Players.Count()} players. We will play {eventModel.Main.Rounds} rounds of {eventModel.Main.ModeRaw}");
            if (eventModel.Finals.Rounds == 0)
                builder.Append(".  ");
            else if (eventModel.Finals.Mode == EventStructure.SingleElimination)
                builder.Append($" followed by cut to top {Math.Pow(2, eventModel.Finals.Rounds)}.  ");
            else
                builder.Append($" followed by {eventModel.Finals.Rounds} rounds of {eventModel.Finals.ModeRaw}.  ");
            if (isPD)
            {

                var tournamentInfo = (await API.DecksiteApi.GetTournaments()).FirstOrDefault(t => t.Name == eventModel.Series);
                if (tournamentInfo?.SponsorName != null)
                    builder.Append($"There are prizes from {tournamentInfo?.SponsorName} for the Top 8 finishes and a door prize for one other randomly-selected player completing the Swiss rounds. "); ;
                if (tournamentInfo?.SponsorName == "Cardhoarder")
                    builder.Append("Prizes will be credited to your Cardhoarder account automatically some time at the end of this week.");
                builder.AppendLine().Append("Please make your games in Constructed, Specialty, Freeform Tournament Practice with 'Penny Dreadful' and your opponent's name in the comments with watchers allowed."
                    + $" If your opponent doesn't show up please message them directly on Magic Online and Discord and if they are not there at :{FreeWinTime(eventModel.Name, round.RoundNum):D2} contact the host for your free 2-0 win."
                    + "\nGood luck everyone!\n");
            }
        }

        static Dictionary<string, int> freeWinTime = new Dictionary<string, int>();
        private static int FreeWinTime(string name, int roundNum)
        {
            var key = $"{name}:{roundNum}";
            if (freeWinTime.ContainsKey(key))
                return freeWinTime[key];

            var time = (DateTime.UtcNow.Minute + 11) % 60;
            freeWinTime[key] = time;
            return time;
        }
    }
}
