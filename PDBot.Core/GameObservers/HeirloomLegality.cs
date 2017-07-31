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
    public class HeirloomLegality : BaseLegalityChecker, IGameObserver
    {
        protected override string LegalListUrl => "https://docs.google.com/spreadsheets/d/1HxH-C6ZCY3nXbYN60xerY7GENb0jZ_49_5cvXowElhU/export?format=csv&id=1HxH-C6ZCY3nXbYN60xerY7GENb0jZ_49_5cvXowElhU&gid=0";

        public override string FormatName => "Heirloom";

        public override string MoreInfo => null;

        public override bool ShouldJoin(IMatch match)
        {
            return match.Format == MagicFormat.Heirloom;
        }

        public override async Task<IGameObserver> GetInstanceForMatchAsync(IMatch match)
        {
            if (ShouldJoin(match))
            {
                return new HeirloomLegality();
            }
            return null;
        }
    }
}
