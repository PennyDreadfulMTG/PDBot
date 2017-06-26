using PDBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDBot.Core.GameObservers;

namespace PDBot.Core.Commands
{
    class LeagueCommand : ICommand
    {
        public string[] Handle { get; } = new string[] { "!league" };

        public bool AcceptsGameChat => true;

        public bool AcceptsPM => true;

        public async Task<string> Run(string player, IMatch game, string[] args)
        {
            var LeagueObserver = game?.Observers?.SingleOrDefault(o => o is GameObservers.LeagueObserver) as GameObservers.LeagueObserver;

            if (LeagueObserver != null && LeagueObserver.HostRun != null && LeagueObserver.LeagueRunOpp != null)
            {
                return $"You can @[Report] your results on the website.";
            }
            else if (game != null)
            {
                return $"This isn't a @[League] match.";
            }
            else
            {
                return "Find out more about the Penny Dreadful @[League].";
            }
        }
    }
}
