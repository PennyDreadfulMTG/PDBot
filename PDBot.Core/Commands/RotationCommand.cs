using PDBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDBot.Core.Interfaces;
using PDBot.API;
using PDBot.Core.API;

namespace PDBot.Core.Commands
{
    class RotationCommand : ICommand
    {
        public string[] Handle => new string[] { "!rotation" };

        public bool AcceptsGameChat => true;

        public bool AcceptsPM => true;

        public async Task<string> RunAsync(string player, IMatch game, string[] args)
        {
            DecksiteApi.Rotation rotation = await DecksiteApi.GetRotationAsync();
            return $"[sU]The next rotation is in {rotation.FriendlyDiff}";
        }
    }
}
