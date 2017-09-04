using PDBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDBot.Core.GameObservers;
using PDBot.Interfaces;
using PDBot.Core.Interfaces;

namespace PDBot.Core.Commands
{
    class LeagueCommand : ICommand
    {
        public string[] Handle { get; } = new string[] { "!league" };

        public bool AcceptsGameChat => true;

        public bool AcceptsPM => true;

        public Task<string> RunAsync(string player, IMatch game, string[] args)
        {

            if (game?.Observers?.SingleOrDefault(o => o is GameObservers.LeagueObserver) is GameObservers.LeagueObserver LeagueObserver && LeagueObserver.HostRun != null && LeagueObserver.LeagueRunOpp != null)
            {
                return Task.FromResult($"You can @[Report] your results on the website.");
            }
            else if (game != null)
            {
                return Task.FromResult($"This isn't a @[League] match.");
            }
            else
            {
                return Task.FromResult("Find out more about the Penny Dreadful @[League].");
            }
        }
    }
}
