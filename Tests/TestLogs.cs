using NUnit.Framework;
using PDBot.Core;
using PDBot.Core.Data;
using PDBot.Core.GameObservers;
using PDBot.Core.Interfaces;
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
            var match = new MockMatch();
            CountCards(line, cards, tokens, match);
        }

        private static void CountCards(string line, int cards, int tokens, IMatch match)
        {
            var parser = new GameLogLine(line, match);
            Assert.AreEqual(cards, parser.Cards.Count());
            Assert.AreEqual(tokens, parser.Tokens.Count());
        }

        [Test]
        public void TestPDIllegal()
        {
            var match = new MockMatch();
            var checker = match.Observers.OfType<PennyDreadfulLegality>().Single();
            Assert.IsNotNull(checker.HandleLine(new GameLogLine("[Black Lotus] is never going to be 0.01 TIX.", match)));
            Assert.IsNull(checker.HandleLine(new GameLogLine("[Black Lotus] is never going to be 0.01 TIX.", match)));
        }
        }

        [Test]
        public void TestHeirloomLegality()
        {
            var match = new MockMatch("Heirloom", null, MagicFormat.Heirloom);
            var checker = match.Observers.OfType<HeirloomLegality>().Single();
            Assert.IsNotNull(checker.HandleLine(new GameLogLine("[Dark Ritual] is banned.", match)));
            Assert.IsNull(checker.HandleLine(new GameLogLine("[Part the Waterveil] is good.", match)));
            Assert.IsNull(checker.HandleLine(new GameLogLine("[Ajani Goldmane][Ajani Steadfast][Ajani Unyielding][Ajani, Caller of the Pride], and [Ajani, Mentor of Heroes] are all legal.", match)));
        }

        [Test]
        public void TestSparkSpitter()
        {
            // This test checks that creatures that are created by the Sparkspitter/Llanowar Mentor cycle are cards until proven to be tokens.
            var match = new MockMatch();
            CountCards("TheFancyMusterd is being attacked by [Spark Elemental] and [Fusion Elemental].", 2, 0, match);
            CountCards("WookieeGT activates an ability of[Sparkspitter](Create a 3 / 1 red Elemental creature token named Spark Elemental.It has trample, haste, and 'At t...).", 
                1, 0, match);
            CountCards("WookieeGT's [Sparkspitter] creates a Spark Elemental.", 1, 0, match);
            CountCards("TheFancyMusterd is being attacked by [Spark Elemental] and [Fusion Elemental].", 1, 1, match);

            
        }
    }
}
