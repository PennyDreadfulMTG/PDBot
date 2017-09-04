using PDBot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDBot.Core.Data;

namespace PDBot.Core.GameObservers
{
    class StalkerMode : IGameObserver
    {
        static readonly string[] FavoritePlayers = new string[] { "silasary", "hexalite", "j_meka", "brainlesss96" };

        public bool PreventReboot => false;

        public Task<IGameObserver> GetInstanceForMatchAsync(IMatch match)
        {
            return Task.FromResult<IGameObserver>(null);
        }

        public string HandleLine(GameLogLine gameLogLine)
        {
            return null;
        }

        public void ProcessWinner(string winner, int gameID)
        {

        }

        public bool ShouldJoin(IMatch match)
        {
            return match.Players.Intersect(FavoritePlayers, StringComparer.CurrentCultureIgnoreCase).Any();
        }
    }
}
