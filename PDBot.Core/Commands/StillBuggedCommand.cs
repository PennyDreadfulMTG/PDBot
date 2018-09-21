using PDBot.Core.Interfaces;
using PDBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.Commands
{
    class StillBuggedCommand : ICommand
    {
        public string[] Handle => new string[] { "!stillbugged" };

        public bool AcceptsGameChat => true;

        public bool AcceptsPM => false;

        public async Task<string> RunAsync(string player, IMatch game, string[] args)
        {
            var (success, message) = await API.BuggedCards.UpdateBuggedAsync(string.Join(" ", args), player, game.MatchID, false).ConfigureAwait(false);
            return message;
        }
    }

    class NotBuggedCommand : ICommand
    {
        public string[] Handle => new string[] { "!notbugged" };

        public bool AcceptsGameChat => true;

        public bool AcceptsPM => false;

        public async Task<string> RunAsync(string player, IMatch game, string[] args)
        {
            var (success, message) = await API.BuggedCards.UpdateBuggedAsync(string.Join(" ", args), player, game.MatchID, true).ConfigureAwait(false);
            return message;
        }
    }
}
