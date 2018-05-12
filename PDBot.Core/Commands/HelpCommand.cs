using PDBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDBot.Core.Interfaces;

namespace PDBot.Core.Commands
{
    class HelpCommand : ICommand
    {
        public string[] Handle => new string[] { "!help" };

        public bool AcceptsGameChat => false;

        public bool AcceptsPM => true;

        public Task<string> RunAsync(string player, IMatch game, string[] args)
        {
            const string HelpText = "If you have any questions about PDBot or its operations, please ask in Discord, or PM silasary if she is online. \n\n"+
                "You can find out more at https://pennydreadfulmagic.com/about \n" +
                "or https://pennydreadfulmagic.com/discord";
            return Task.FromResult(HelpText);
        }
    }
}
