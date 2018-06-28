using PDBot.Core.API;
using PDBot.Core.Interfaces;
using PDBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.Commands
{
    class LeagueRecord : ICommand
    {
        public string[] Handle => new string[] { "!league", "!active", "!run", "!record" };

        public bool AcceptsGameChat => false;

        public bool AcceptsPM => true;

        public async Task<string> RunAsync(string player, IMatch game, string[] args)
        {
            DecksiteApi.Deck run;
            try
            {
                run = await DecksiteApi.GetRunAsync(player);
            }
            catch (WebException)
            {
                return "Error contacting PDM website.";
            }
            if (run == null)
            {
                return $"You do not have an active deck in {DecksiteApi.CurrentLeagueName()}.";
            }
            return $"Your deck '{run.Name}' is currently {run.Wins}-{run.Losses}";
        }
    }
}
