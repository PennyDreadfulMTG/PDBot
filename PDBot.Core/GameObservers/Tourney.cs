using PDBot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDBot.Core.Data;
using PDBot.Core.Tournaments;

namespace PDBot.Core.GameObservers
{
    class Tourney : IGameObserver
    {
        private readonly IMatch match;
        private static ITournamentManager tournamentManager;

        public Tourney()
        {

        }

        public Tourney(IMatch match)
        {
            this.match = match;
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                var ae = GetEvent(match);
                if (ae != null)
                {
                    var round = await ae.GetCurrentPairingsAsync();
                    match.Log($"[Gatherling] Event={ae.Name}");
                    match.Log($"[Gatherling] Round={round.RoundNum}");
                }

            });
        }

        public bool PreventReboot => true;

        public static ITournamentManager TournamentManager => tournamentManager ?? (tournamentManager = Resolver.Helpers.GetTournamentManager());

        public Task<IGameObserver> GetInstanceForMatchAsync(IMatch match)
        {
            if (ShouldJoin(match))
            {
                return Task.FromResult<IGameObserver>(new Tourney(match));
            }
            return Task.FromResult<IGameObserver>(null);
        }

        public string HandleLine(GameLogLine gameLogLine)
        {
            return null;
        }

        public void ProcessWinner(string winner, int gameID)
        {
            var channel = GetChannel(match);
            if (channel == null)
                return;
            match.Winners.GetRecordData(out var first, out var record);
            if (first.Wins == 2)
            {
                var loser = match.Players.FirstOrDefault(d => d != winner);
                if (Features.PublishResults)
                {
                    Resolver.Helpers.GetChatDispatcher().SendPM(channel, $"[sD] {winner} {record} {loser} (You still need to report)");
                }
            }
        }

        private static string GetChannel(IMatch match)
        {
            var tournament = GetEvent(match);
            if (tournament != null)
                return tournament.Channel;

            string channel = null;
            if (match.Format == MagicFormat.Heirloom)
            {
                channel = "#heirloom";
            }
            else if (match.Format == MagicFormat.PennyDreadful)
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
                if (now.DayOfWeek == DayOfWeek.Sunday || now.DayOfWeek == DayOfWeek.Saturday)
                    channel = "#PDS";
                else if (now.DayOfWeek == DayOfWeek.Monday)
                    channel = "#PDM";
                else if (now.DayOfWeek == DayOfWeek.Thursday)
                    channel = "#PDT";
            }
            else if (match.Format == MagicFormat.Squire)
                channel = "#squire";
            else if (IsCLL(match))
                channel = "#CLL";
            else if (IsPCT(match))
                channel = "#PCT";
            else if (IsPauperPower(match))
                channel = "#pauperpower";
            else if (IsModernTimes(match))
                channel = "#modern";
            return channel;
        }

        public static Gatherling.Models.Event GetEvent(IMatch match)
        {
            foreach (var tournament in TournamentManager.ActiveEvents)
            {
                var pairing = tournament.Value.Matches.FirstOrDefault(p => match.Players.Contains(p.A) && match.Players.Contains(p.B));
                if (pairing != null)
                {
                    TournamentManager.ActiveMatches.Add(match);
                    if (tournament.Key.Channel != null)
                    {
                        return tournament.Key;
                    }
                }
            }
            return null;
        }

        public bool ShouldJoin(IMatch match)
        {
            if (match.GameRoom != Room.GettingSerious)
                return false;
            if (GetChannel(match) != null)
                return true;
            //Console.WriteLine($"Missed Tourney game:\n\t{match.Format.ToString()}\n\t{match.Comments}");
            return false;
        }

        private static bool IsCLL(IMatch match)
        {
            if (match.Format != MagicFormat.Legacy)
                return false;
            if (match.Comments.ToUpper().Contains("CLL"))
                return true;
            if (match.Comments.ToLower().Contains("community legacy league"))
                return true;
            return false;
        }

        private static bool IsPCT(IMatch match)
        {
            if (match.Format != MagicFormat.Pauper)
                return false;
            if (match.Comments.ToLower().Contains("pct"))
                return true;
            if (match.Comments.ToLower().Contains("pauper classic tuesdays"))
                return true;
            return false;
        }

        private static bool IsPauperPower(IMatch match)
        {
            if (match.Format != MagicFormat.Pauper)
                return false;
            if (match.Comments.ToLower().Contains("pauperpower"))
                return true;
            if (match.Comments.ToLower().Contains("pauper power"))
                return true;
            return false;

        }

        private static bool IsModernTimes(IMatch match)
        {
            if (match.Format != MagicFormat.Modern)
                return false;
            return match.Comments.ToLower().Replace(" ", "").Contains("moderntimes");
        }

    }
}
