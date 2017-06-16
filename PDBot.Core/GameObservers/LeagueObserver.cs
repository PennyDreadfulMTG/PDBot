using PDBot.API;
using PDBot.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.GameObservers
{
    class LeagueObserver : IGameObserver
    {
        private IMatch match;

        public League.Deck HostRun { get; private set; }
        public League.Deck LeagueRunOpp { get; private set; }

        public LeagueObserver()
        {

        }

        public LeagueObserver(IMatch match)
        {
            this.match = match;
            CheckForLeague(); // Do this on an async thread.
        }

        public IGameObserver GetInstanceForMatch(IMatch match)
        {
            return new LeagueObserver(match);
        }

        public string HandleLine(GameLogLine gameLogLine)
        {
            foreach (var name in gameLogLine.Cards)
            {
                if (HostRun == null || LeagueRunOpp == null)
                    return null;

                var InList = HostRun.ContainsCard(name) || LeagueRunOpp.ContainsCard(name);
                if (!InList)
                {
                    HostRun = LeagueRunOpp = null;
                    return $"[sD][sR]{name} was not on a submitted decklist. This is not a league match.";
                }
            }
            return null;
        }

        public bool IsApplicable(string comment, MagicFormat format, Room room)
        {
            if (format == MagicFormat.PennyDreadful || format == MagicFormat.PennyDreadfulCommander)
            {
                return true;
            }
            return false;
        }

        private async void CheckForLeague()
        {
            Console.WriteLine("Checking for League");
            if (match.Players.Length != 2)
                return;

            HostRun = await League.GetRun(match.Players[0]);
            var desc = match.Comments.ToLower();
            if (HostRun == null)
            {
                if (desc.Contains("league"))
                {
                    match.SendChat($"[sD][sR]This is not a valid @[League] pairing!");
                    match.SendChat($"[sD][sR]{match.Players[0]}, you do not have an active run.");
                }

                return;
            }

            var opp = match.Players[1];
            LeagueRunOpp = await League.GetRun(opp);
            if (HostRun.CanPlay.Contains(opp, StringComparer.InvariantCultureIgnoreCase))
            {
                if (desc.Contains("league"))
                    match.SendChat($"[sD]Good luck in your @[League] match!");
                else
                    match.SendChat($"[sD]If this is a league game, don't forget to @[Report]!");
            }
            else if (desc.Contains("league"))
            {
                match.SendChat($"[sD][sR]This is not a valid @[League] pairing!");
                if (HostRun == null)
                    match.SendChat($"[sD][sR]{match.Players[0]}, you do not have an active run.");
                else if (LeagueRunOpp == null)
                    match.SendChat($"[sD][sR]{opp}, you do not have an active run.");
                else
                    match.SendChat($"[sD][sR]You have both already played each other with these decks.");
            }
        }

        public void ProcessWinner(string winner, int gameID)
        {
            match.GetRecord(out var first, out var record);
            if (first.Value == 2)
            {
                if (HostRun != null && LeagueRunOpp != null)
                {
                    var WinningRun = HostRun.Person.Equals(winner, StringComparison.InvariantCultureIgnoreCase) ? HostRun : LeagueRunOpp;
                    var LosingRun = (new League.Deck[] { HostRun, LeagueRunOpp }).Single(d => d != WinningRun);
                    League.UploadResults(WinningRun, LosingRun, record);
                    DiscordService.SendToLeagueAsync($":trophy: {WinningRun.Person} {record} {LosingRun.Person}");
                }
            }
        }
    }
}
