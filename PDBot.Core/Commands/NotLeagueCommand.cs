using PDBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDBot.Core.GameObservers;
using PDBot.Core.Interfaces;
using PDBot.Interfaces;

namespace PDBot.Core.Commands
{
    class NotLeagueCommand : ICommand
    {
        public string[] Handle { get; } = new string[] { "!notleague" };

        public bool AcceptsGameChat => true;

        public bool AcceptsPM => false;

        public async Task<string> RunAsync(string player, IMatch game, string[] args)
        {
            if (game?.Observers?.SingleOrDefault(o => o is GameObservers.LeagueObserver) is GameObservers.LeagueObserver LeagueObserver && LeagueObserver.HostRun != null && LeagueObserver.LeagueRunOpp != null)
            {
                LeagueObserver.HostRun = null;
                return $"[sD]Ok, I won't treat this as a league match.\nIf you change your mind, please @[Report] manually.";
            }
            else
            {
                return $"[sD]This isn't a @[League] match.";
            }
        }
    }
}
