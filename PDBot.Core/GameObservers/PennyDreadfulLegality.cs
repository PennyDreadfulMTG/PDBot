using Newtonsoft.Json.Linq;
using PDBot.Core.Data;
using PDBot.Core.Interfaces;
using PDBot.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.GameObservers
{
    public class PennyDreadfulLegality : BaseLegalityChecker, IGameObserver
    {
        public override string FormatName => "Penny Dreadful";

        public override string MoreInfo => "pdmtgo.com or reddit.com/r/PennyDreadfulMTG";

        protected override string LegalListUrl => "http://pdmtgo.com/legal_cards.txt";

        public override Task<IGameObserver> GetInstanceForMatchAsync(IMatch match)
        {
            if (ShouldJoin(match))
            {
                return Task.FromResult<IGameObserver>(new PennyDreadfulLegality());
            }
            return Task.FromResult<IGameObserver>(null);
        }

        public override bool ShouldJoin(IMatch match)
        {
            return match.Format == MagicFormat.PennyDreadful || match.Format == MagicFormat.PennyDreadfulCommander;
        }
    }
}
