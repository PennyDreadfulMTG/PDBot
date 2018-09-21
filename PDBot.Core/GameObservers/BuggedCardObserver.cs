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
        private readonly List<string> warnings = new List<string>();
        private readonly IMatch match;

        public BuggedCardObserver()
        {

        }
    
        public BuggedCardObserver(IMatch match)
        {
            this.match = match;
        }

        public bool PreventReboot => false;

        public Task<IGameObserver> GetInstanceForMatchAsync(IMatch match)
        {
            return Task.FromResult<IGameObserver>(new BuggedCardObserver(match));
        }

        public string HandleLine(GameLogLine gameLogLine)
        {
            foreach (var name in gameLogLine.Cards)
            {
                if (warnings.Contains(name))
                    continue;
                if (API.BuggedCards.IsCardBugged(name) is API.BuggedCards.Bug bug)
                {
                    if (bug.Multiplayer && match.Players.Length < 3)
                        continue;
                    warnings.Add(name);
                    string a_or_an(string word) // Yes, it's bad.
                    {
                        return word[0] == 'a' ? "an" : "a";
                    }
                    var a = a_or_an(bug.Classification);
                    var v = new StringBuilder($"[sU]{name}[sU] has {a} {bug.Classification} bug.\n");
                    v.AppendLine(bug.Description.Replace("[", "").Replace("]", ""));
                    if (bug.HelpWanted)
                    {
                        v.AppendLine("Our data about this bug is out of date.  Please let us know if this card is still bugged (or if it's been fixed).");
                        v.AppendLine($"You can do so by PM'ing this bot, or by typing `!stillbugged {name}` or `!notbugged {name}`");
                    }
                    return v.ToString().Trim();
                }
            }
            return null;
        }

        public void ProcessWinner(string winner, int gameID)
        {

        }

        public bool ShouldJoin(IMatch match)
        {
            // Bugged cards is never enough of a reason to join a match.  It's just an added bonus
            return false;
        }
    }
}
