using PDBot.Core.API;
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

        public Task<IGameObserver> GetInstanceForMatchAsync(IMatch match)
        {
            if (match.Format != MagicFormat.PennyDreadful && match.Format != MagicFormat.PennyDreadfulCommander)
                return Task.FromResult<IGameObserver>(null);

            if (Tourney.GetEvent(match) != null)
                return Task.FromResult<IGameObserver>(null); // Tournament Matches aren't League Matches.

            var obs =  new LeagueObserver(match);
            obs.CheckForLeagueAsync().GetAwaiter();
            return Task.FromResult<IGameObserver>(obs);
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
                    match.Log("[League] Invalid Match");
                    return $"[sD][sR] {name} was not on a submitted league decklist. This is not a league match.";
                }
            }
            return null;
        }

        private async Task<bool> CheckForLeagueAsync()
        {
            if (match.Players.Length != 2)
                return false;
            await Task.Delay(TimeSpan.FromSeconds(2)); // Sometimes PDBot gets into a game before one of the players.  If this happens, they miss the message.
            var desc = match.Comments.ToLower();
            var loud = desc.Contains("league");
            try
            {
                HostRun = await DecksiteApi.GetRunAsync(match.Players[0]);
            }
            catch (Exception)
            {
                match.Log($"[League] Unable to reach PDM");
                if (loud)
                {
                    match.SendChat($"[sD][sR] Error contacting pennydreadfulmagic.com, Please @[Report] manually!");
                }
                return false;
            }
            if (HostRun == null)
            {
                match.Log($"[League] Host doesn't have active run");
                if (loud)
                {
                    match.SendChat($"[sD][sR] This is not a valid @[League] pairing!");
                    match.SendChat($"[sD][sR] {match.Players[0]}, you do not have an active run.");
                }

                return false;
            }

            var opp = match.Players[1];
            try
            {
                LeagueRunOpp = await DecksiteApi.GetRunAsync(opp);
            }
            catch (Exception)
            {
                match.Log($"[League] Unable to reach PDM");
                if (loud)
                {
                    match.SendChat($"[sD][sR] Error contacting pennydreadfulmagic.com, Please @[Report] manually!");
                }
                return false;
            }

            if (HostRun.CanPlay.Contains(opp, StringComparer.InvariantCultureIgnoreCase))
            {
                if (loud)
                    match.SendChat($"[sD] Good luck in your @[League] match!");
                else
                    match.SendChat($"[sD] If this is a league game, don't forget to @[Report]!");
                match.Log($"[League] {HostRun} ({HostRun.Id}) vs {LeagueRunOpp} ({LeagueRunOpp.Id})");

                if (File.Exists(Path.Combine("Updates", "urgent.txt")))
                {
                    match.SendChat("[sD] PDBot will be going down for scheduled maintenance.  Please @[Report] this league match manually.");
                    HostRun = null;
                }
                else if (!Features.PublishResults)
                {
                    match.SendChat("[sD] Due to a Magic Online bug, PDBot is unable to tell which player is which.  Please @[Report] this league match manually.");
                    HostRun = null;
                }
                return true;

            }
            else
            {
                if (loud)
                    match.SendChat($"[sD][sR] This is not a valid @[League] pairing!");
                if (HostRun == null)
                {
                    if (loud)
                        match.SendChat($"[sD][sR] {match.Players[0]}, you do not have an active run.");
                match.Log($"[League] {match.Players[0]} doesn't have active run");
                }
                else if (LeagueRunOpp == null)
                {
                    if (loud)
                        match.SendChat($"[sD][sR] {opp}, you do not have an active run.");
                    match.Log($"[League] {opp} doesn't have active run");
                }
                else
                {
                    if (loud)
                        match.SendChat($"[sD][sR] You have both already played each other with these decks.");
                    match.Log($"[League] Duplicate Pairing: {HostRun} ({HostRun.Id}) vs {LeagueRunOpp} ({LeagueRunOpp.Id})");
                }

                HostRun = null;
                LeagueRunOpp = null;
                return false;
            }
        }

        public async void ProcessWinner(string winner, int gameID)
        {
            match.Winners.GetRecordData(out var first, out var record);
            if (first.Wins == 2 && HostRun != null && LeagueRunOpp != null)
            {
                var WinningRun = HostRun.Person.Equals(winner, StringComparison.InvariantCultureIgnoreCase) ? HostRun : LeagueRunOpp;
                var LosingRun = (new DecksiteApi.Deck[] { HostRun, LeagueRunOpp }).Single(d => d != WinningRun);
                if (Features.PublishResults && await DecksiteApi.UploadResultsAsync(WinningRun, LosingRun, record, match.MatchID))
                {
                    await DiscordService.SendToLeagueAsync($":trophy: {WinningRun.Person} {record} {LosingRun.Person}");
                }
                else
                {
                    await DiscordService.SendToLeagueAsync($":trophy: {WinningRun.Person} {record} {LosingRun.Person} (Please verify and report manually)");
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
