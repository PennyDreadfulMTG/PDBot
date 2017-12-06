using PDBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDBot.Core.Interfaces;
using System.Net;
using System.Xml.Linq;
using PDBot.API;

namespace PDBot.Commands
{
    class VerifyGatherling : ICommand
    {
        public string[] Handle => new string[] { "!verify" };

        public bool AcceptsGameChat => false;

        public bool AcceptsPM => true;

        public Task<string> RunAsync(string player, IMatch game, string[] args)
        {
            return Gatherling.PennyDreadful.GetVerificationCodeAsync(player);
        }
    }
}
