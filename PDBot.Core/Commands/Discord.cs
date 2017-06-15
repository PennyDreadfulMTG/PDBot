using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Commands
{
    public class Discord : ICommand
    {
        public string[] Handle => new string[] { "!discord" };

        public bool AcceptsGameChat => true;

        public bool AcceptsPM => true;

        public async Task<string> Run(string user, string[] args)
        {
            return $"Join our Discord community! https://discord.gg/RxhTEEP";
        }
    }
}
