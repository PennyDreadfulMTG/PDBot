using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PDBot.Commands
{
    public class ExplainPD : ICommand
    {
        public string[] Handle => new string[] { "!pd" };

        public bool AcceptsGameChat => true;

        public bool AcceptsPM => false;

        public async Task<string> Run(string user, string[] args)
        {
            return $"[sU]Penny Dreadful is a Player-run format for MTGO where only cards that cost 0.01 tix online are legal.\n" +
                $"For more information about Penny Dreadful, see pdmtgo.com or reddit.com/r/PennyDreadfulMTG";
        }
    }
}
