using NUnit.Framework;
using NUnit.Framework.Legacy;
using PDBot.Core;
using PDBot.Core.Data;
using PDBot.Core.GameObservers;
using PDBot.Core.Interfaces;
using PDBot.Data;
using System.Linq;
using Tests.Mocks;

namespace Tests
{
    class TestLogs
    {
        [TestCase("skinnersweet1880 is being attacked by [Vampire] token, [Vampire] token, [Vampire Nighthawk], [Vampire Lacerator], and [Lord of Lineage].", 3, 2)]
        [TestCase("[Eldrazi Scion] token blocks [Lightning Berserker].", 1, 1)]
        [TestCase("[Elvish Visionary] blocks [Tuktuk the Returned].", 1, 1)]
        [TestCase("Blockers for [Vampire Nighthawk] are ordered as follows: [Glint-Nest Crane], [Faerie Mechanist]", 3, 0)]
        [TestCase("jmblinn13 is being attacked by [Spirit Token].", 0, 1)]
        [TestCase("Tygrak is being attacked by [Legitimate Businessperson].", 0, 1)]
        [TestCase("Username activates an ability of [Everflame, Heroes' Legacy] targeting [Tameshi, Reality Architect].", 1, 1)]
        public void TestLogHandler(string line, int cards, int tokens)
        {
            var match = new MockMatch();
            CountCards(line, cards, tokens, match);
        }

        private static void CountCards(string line, int cards, int tokens, IMatch match)
        {
            var parser = new GameLogLine(line, match);
            ClassicAssert.AreEqual(cards, parser.Cards.Count());
            ClassicAssert.AreEqual(tokens, parser.Tokens.Count());
        }

        [Test]
        public void TestPDIllegal()
        {
            var match = new MockMatch();
            var checker = match.Observers.OfType<PennyDreadfulLegality>().Single();
            ClassicAssert.IsNotNull(checker.HandleLine(new GameLogLine("[Black Lotus] is never going to be 0.01 TIX.", match)));
            ClassicAssert.IsNull(checker.HandleLine(new GameLogLine("[Black Lotus] is never going to be 0.01 TIX.", match)));
        }

        [Test]
        public void TestAccents()
        {
            ClassicAssert.AreEqual("Dandân", new CardName("Dandân").FullName);
            ClassicAssert.AreEqual("Junún Efreet", new CardName("Junún Efreet"));
            ClassicAssert.AreEqual("Márton Stromgald", new CardName("Márton Stromgald").FullName);
            ClassicAssert.AreEqual("Dandân", new CardName("DandÃ¢n").FullName);
            ClassicAssert.AreEqual("Lim-Dûl the Necromancer", new CardName("Lim-DÃ»l the Necromancer").FullName);
            ClassicAssert.AreEqual("Barad-dûr", new CardName("Barad-dÃ»r").FullName);
            ClassicAssert.AreEqual("Ifh-Bíff Efreet", new CardName("Ifh-BÃ­ff Efreet").FullName);
        }

        [Test]
        public void TestLegalCardsForAccents()
        {
            var legality = new PennyDreadfulLegality();
            Assume.That(legality.IsCardLegal("Island"));
            foreach (var name in legality.LegalCards)
            {
                var n = CardName.FixAccents(name);
                ClassicAssert.IsFalse(n.Contains("Ã"), $"{name} contains an Ã.");
            }
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
