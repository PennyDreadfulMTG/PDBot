using PDBot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDBot.Core.Data;

namespace PDBot.Core.GameObservers
{
    /// <summary>
    /// This is not useful or functional in any particular way.
    /// We do not actually provide any Frontier functionality.
    /// I'm just curious to know if games even fire.
    /// </summary>
    class FrontierObserver : IGameObserver
    {
        public bool PreventReboot => false;

        public Task<IGameObserver> GetInstanceForMatchAsync(IMatch match)
        {
            if (ShouldJoin(match))
            {
                return Task.FromResult<IGameObserver>(new FrontierObserver());
            }
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
            return match.Format == MagicFormat.Frontier;
        }
    }
}
