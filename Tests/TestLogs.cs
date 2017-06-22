using NUnit.Framework;
using PDBot.Core;
using PDBot.Core.GameObservers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Mocks;

namespace Tests
{
    class TestLogs
    {
        [TestCase("skinnersweet1880 is being attacked by [Vampire] token, [Vampire] token, [Vampire Nighthawk], [Vampire Lacerator], and [Lord of Lineage].", 3, 2)]
        [TestCase("[Eldrazi Scion] token blocks [Lightning Berserker].", 1, 1)]
        [TestCase("[Elvish Visionary] blocks [Tuktuk the Returned].", 1, 1)]
        [TestCase("Blockers for [Vampire Nighthawk] are ordered as follows: [Glint-Nest Crane], [Faerie Mechanist]", 3, 0)]
        public void TestLogHandler(string line, int cards, int tokens)
        {
            var parser = new GameLogLine(line);
            Assert.AreEqual(cards, parser.Cards.Count());
            Assert.AreEqual(tokens, parser.Tokens.Count());
        }

        [Test]
        public void TestPDIllegal()
        {
            var checker = Resolver.GetInstances<IGameObserver>().Single(o => o.GetType().Name == "PennyDreadfulLegality").GetInstanceForMatch(new MockMatch());
            Assert.IsNotNull(checker.HandleLine(new GameLogLine("[Black Lotus] is never going to be 0.01 TIX.")));
            Assert.IsNull(checker.HandleLine(new GameLogLine("[Black Lotus] is never going to be 0.01 TIX.")));
        }
    }
}
