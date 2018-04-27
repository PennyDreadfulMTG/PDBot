using PDBot.API;
using PDBot.Core.API;
using PDBot.Core.GameObservers;
using PDBot.Core.Interfaces;
using PDBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Commands
{
    public class DropLeague : ICommand
    {

        public string[] Handle { get; } = new string[] { "!drop", "!retire" };

        public bool AcceptsGameChat => true;

        public bool AcceptsPM => true;

        public async Task<string> RunAsync(string user, IMatch game, string[] args)
        {
            DecksiteApi.Deck run;
            try
            {
                run = await DecksiteApi.GetRunAsync(user);
            }
            catch (WebException)
            {
                return "Error contacting PDM website.";
            }
            if (run == null)
                return $"You do not have an active deck in {DecksiteApi.CurrentLeagueName()}.";
            var res = await run.Retire();
            if (res)
                return $"Your deck {run.Name} has been retired from the {run.CompetitionName}";
            else
                return $"Unable to retire your deck.  Please message Katelyn on discord.";
        }
    }
}
