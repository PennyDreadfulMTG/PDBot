using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDBot.Core.GameObservers;
using PDBot.Core.Interfaces;
using PDBot.Interfaces;

namespace PDBot.Commands
{
    public class Discord : ICommand
    {
        public string[] Handle => new string[] { "!discord" };

        public bool AcceptsGameChat => false;

        public bool AcceptsPM => false;

        public async Task<string> RunAsync(string user, IMatch game, string[] args)
        {
            return $"Join our Discord community! https://discord.gg/RxhTEEP";
        }
    }
}
