using PDBot.Core.Interfaces;
using PDBot.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Commands
{
    class SuppressHype : ICommand
    {
        public string[] Handle { get; } = new string[] { "!suppresshype" };

        public bool AcceptsGameChat => false;

        public bool AcceptsPM => true;

        public Task<string> RunAsync(string player, IMatch game, string[] args)
        {
            File.WriteAllText("suppress_hype.txt", "true");
            return Task.FromResult("Suppressed");
        }
    }
}
