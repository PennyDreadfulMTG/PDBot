using PDBot.Core.Data;
using PDBot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.GameObservers
{
    internal class SevenPointHighlander : IGameObserver
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
            return match.Format == MagicFormat.SevenPointHighlander;
        }

    }
}
