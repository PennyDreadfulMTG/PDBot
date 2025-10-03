using PDBot.Core.GameObservers;
using PDBot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    internal class TestLegalityChecker : BaseLegalityChecker
    {
        private readonly string format_url;

        public TestLegalityChecker(string[] legal_cards, string url = null)
        {
            LegalCards = legal_cards;
            format_url = url;
        }

        public override string FormatName => "Test Format";

        public override string MoreInfo => "";

        protected override string LegalListUrl => format_url;

        public override Task<IGameObserver> GetInstanceForMatchAsync(IMatch match)
        {
            throw new NotImplementedException();
        }

        public override bool ShouldJoin(IMatch match)
        {
            throw new NotImplementedException();
        }
    }
}
