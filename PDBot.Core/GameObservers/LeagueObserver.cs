using PDBot.API;
using PDBot.Core.Data;
using PDBot.Core.Interfaces;
using PDBot.Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.GameObservers
{
    class LeagueObserver : IGameObserver, ILeagueObserver
    {
        private readonly IMatch match;

        public DecksiteApi.Deck HostRun { get; internal set; }
        public DecksiteApi.Deck LeagueRunOpp { get; internal set; }

        public bool PreventReboot => IsLeagueGame;

        public bool IsLeagueGame => HostRun != null && LeagueRunOpp != null;

        public LeagueObserver()
        {

        }

        public LeagueObserver(IMatch match)
        {
            this.match = match;
        }

        public async Task<IGameObserver> GetInstanceForMatchAsync(IMatch match)
        {
            if (match.Format != MagicFormat.PennyDreadful && match.Format != MagicFormat.PennyDreadfulCommander)
                return null;

            var obs =  new LeagueObserver(match);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            obs.CheckForLeague();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return obs;
        }

        public string HandleLine(GameLogLine gameLogLine)
        {
            foreach (var name in gameLogLine.Cards)
            {
                if (HostRun == null || LeagueRunOpp == null)
                    return null;

                var InList = HostRun.ContainsCard(name) || LeagueRunOpp.ContainsCard(name) || BaseLegalityChecker.IsRearFace(name);
                if (!InList)
                {
                    HostRun = LeagueRunOpp = null;
                    return $"[sD][sR]{name} was not on a submitted decklist. This is not a league match.";
                }
            }
            return null;
        }

        private async Task<bool> CheckForLeague()
        {
            Console.WriteLine("Checking for League");
            if (match.Players.Length != 2)
                return false;
            var desc = match.Comments.ToLower();
            var loud = desc.Contains("league");
            try
            {
                HostRun = await DecksiteApi.GetRun(match.Players[0]);
            }
            catch (Exception)
            {
                if (loud)
                {
                    match.SendChat($"[sD][sR]Error contacting pennydreadfulmagic.com, Please @[Report] manually!");
                }
                return false;
            }
            if (HostRun == null)
            {
                if (loud)
                {
                    match.SendChat($"[sD][sR]This is not a valid @[League] pairing!");
                    match.SendChat($"[sD][sR]{match.Players[0]}, you do not have an active run.");
                }

                return false;
            }

            var opp = match.Players[1];
            try
            {
                LeagueRunOpp = await DecksiteApi.GetRun(opp);
            }
            catch (Exception)
            {
                if (loud)
                {
                    match.SendChat($"[sD][sR]Error contacting pennydreadfulmagic.com, Please @[Report] manually!");
                }
                return false;
            }

            if (HostRun.CanPlay.Contains(opp, StringComparer.InvariantCultureIgnoreCase))
            {
                if (File.Exists(Path.Combine("Updates", "urgent.txt")))
                {
                    match.SendChat("[sD]PDBot will be going down for scheduled maintenance.  Please @[Report] this league match manually.");
                }

                if (loud)
                    match.SendChat($"[sD]Good luck in your @[League] match!");
                else if (match.GameRoom == Room.GettingSerious)
                    match.SendChat($"[sD]If this is a league game, don't forget to @[Report]!\nIf you do not want this match to be auto-reported, type !notleague");
                else
                    match.SendChat($"[sD]If this is a league game, don't forget to @[Report]!");
                return true;

            }
            else if (loud)
            {
                match.SendChat($"[sD][sR]This is not a valid @[League] pairing!");
                if (HostRun == null)
                    match.SendChat($"[sD][sR]{match.Players[0]}, you do not have an active run.");
                else if (LeagueRunOpp == null)
                    match.SendChat($"[sD][sR]{opp}, you do not have an active run.");
                else
                    match.SendChat($"[sD][sR]You have both already played each other with these decks.");
                HostRun = null;
                LeagueRunOpp = null;
                return false;
            }
            else
                return false;
        }

        public void ProcessWinner(string winner, int gameID)
        {
            match.Winners.GetRecordData(out var first, out var record);
            if (first.Wins == 2)
            {
                if (HostRun != null && LeagueRunOpp != null)
                {
                    var WinningRun = HostRun.Person.Equals(winner, StringComparison.InvariantCultureIgnoreCase) ? HostRun : LeagueRunOpp;
                    var LosingRun = (new DecksiteApi.Deck[] { HostRun, LeagueRunOpp }).Single(d => d != WinningRun);
                    if (Features.PublishResults)
                    {
                        DecksiteApi.UploadResults(WinningRun, LosingRun, record);
                    }

                    DiscordService.SendToLeagueAsync($":trophy: {WinningRun.Person} {record} {LosingRun.Person}");
                }
            }
        }

        public bool ShouldJoin(IMatch match)
        {
            // Don't attempt to join here.  That'll come from PennyDreadfulLegality
            return false;
        }
    }
}
