using PDBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDBot.Core.GameObservers;
using PDBot.Interfaces;
using PDBot.Core.Interfaces;
using PDBot.Core.API;
using System.Net;

namespace PDBot.Core.Commands
{
    class LeagueCommand : ICommand
    {
        public string[] Handle { get; } = new string[] { "!league", "!active", "!run" };

        public bool AcceptsGameChat => true;

        public bool AcceptsPM => true;

        public async Task<string> RunAsync(string player, IMatch game, string[] args)
        {

            if (game?.Observers?.SingleOrDefault(o => o is GameObservers.LeagueObserver) is GameObservers.LeagueObserver LeagueObserver && LeagueObserver.HostRun != null && LeagueObserver.LeagueRunOpp != null)
            {
                return $"This is a valid League match.";
            }
            else if (game != null)
            {
                return $"This isn't a @[League] match.";
            }
            DecksiteApi.Deck run;
            try
            {
                run = await DecksiteApi.GetRunAsync(player);
            }
            catch (WebException)
            {
                return "Error contacting PDM website.";
            }
            if (run == null)
            {
                return $"You do not have an active deck in {DecksiteApi.CurrentLeagueName()}.";
            }
            return $"Your deck '{run.Name}' is currently {run.Wins}-{run.Losses}";
        }
    }
}
