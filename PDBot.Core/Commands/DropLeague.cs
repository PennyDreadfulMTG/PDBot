using PDBot.API;
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

        public async Task<string> Run(string user, IMatch game, string[] args)
        {
            League.Deck run;
            try
            {
                run = await League.GetRun(user);
            }
            catch (WebException c)
            {
                return "Error contacting PDM website.";
            }
            if (run == null)
                return "You do not have an active league deck.";
            var res = run.Retire();
            if (res)
                return $"Your deck {run.Name} has been retired from the {run.CompetitionName}";
            else
                return $"Unable to retire your deck.  Please message Katelyn on discord.";
        }
    }
}
