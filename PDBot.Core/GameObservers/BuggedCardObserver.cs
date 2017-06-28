using PDBot.Core.Data;
using PDBot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.GameObservers
{
    class BuggedCardObserver : IGameObserver
    {
        private List<string> warnings = new List<string>();

        public bool PreventReboot => false;

        public IGameObserver GetInstanceForMatch(IMatch match)
        {
            return new BuggedCardObserver();
        }

        public string HandleLine(GameLogLine gameLogLine)
        {
            foreach (var name in gameLogLine.Cards)
            {
                if (warnings.Contains(name))
                    continue;
                if (API.BuggedCards.IsCardBugged(name) is API.BuggedCards.Bug bug)
                {
                    warnings.Add(name);
                    return $"[sU]{name}[sU] has a {bug.Classification} bug.  {bug.Description}";
                }
            }
            return null;
        }

        public bool IsApplicable(string comment, MagicFormat format, Room room)
        {
            // This one doesn't care about Legality.
            return true;
        }

        public void ProcessWinner(string winner, int gameID)
        {

        }
    }
}
