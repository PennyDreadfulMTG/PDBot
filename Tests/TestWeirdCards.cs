using NUnit.Framework;
using NUnit.Framework.Legacy;
using PDBot.Core.GameObservers;

namespace Tests
{
    class TestWeirdCards
    {
        [Test]
        public void TestRearFaces()
        {
            ClassicAssert.IsFalse(BaseLegalityChecker.IsRearFace("Delver of Secrets"));
            ClassicAssert.IsTrue(BaseLegalityChecker.IsRearFace("Insectile Aberration"));
        }

        [Test]
        public void TestMelds()
        {
            ClassicAssert.IsFalse(BaseLegalityChecker.IsRearFace("Graf Rats"));
            ClassicAssert.IsTrue(BaseLegalityChecker.IsRearFace("Chittering Host"));
        }

        [Test]
        public void TestAdventure()
        {
            ClassicAssert.IsFalse(BaseLegalityChecker.IsRearFace("Smitten Swordmaster"));
            ClassicAssert.IsTrue(BaseLegalityChecker.IsRearFace("Curry Favor"));
        }
        [Test]
        public void TestMDFC()
        {
            ClassicAssert.IsTrue(BaseLegalityChecker.IsRearFace("Akoum Teeth"));
            ClassicAssert.IsTrue(BaseLegalityChecker.IsRearFace("Flamethrower Sonata"));
            ClassicAssert.IsTrue(BaseLegalityChecker.IsRearFace("The Ringhart Crest"));
        }

        [Test]
        public void TestFlip()
        {
            ClassicAssert.IsTrue(BaseLegalityChecker.IsRearFace("Erayo's Essence"));
            ClassicAssert.IsTrue(BaseLegalityChecker.IsRearFace("Erayo, Soratami Ascendant's Essence"));
        }

        [Test]
        public void TestDungeon()
        {
            ClassicAssert.IsTrue(BaseLegalityChecker.IsRearFace("Tomb of Annihilation"));
        }

        [Test]
        public void TestOmens()
        {
            ClassicAssert.IsFalse(BaseLegalityChecker.IsRearFace("Purging Stormbrood"));
            ClassicAssert.IsTrue(BaseLegalityChecker.IsRearFace("Absorb Essence"));
        }

        [Test]
        public void TestSpiderman()
        {
            var format = new TestLegalityChecker(new string[] {
                "Leyline Weaver",
                "Spider-Man Noir",
            });

            Assert.That(format.IsCardLegal("Leyline Weaver"), "Leyline Weaver (OM1) is legal");
            Assert.That(format.IsCardLegal("Kroble, Envoy of the Bog"), "Kroble, Envoy of the Bog (OM1) is legal");
        }

        [Test]
        public void TestFlavorName()
        {
            var format = new TestLegalityChecker(new string[]
            {
                "Fatal Push"
            });

            Assert.That(format.IsCardLegal("Battle at the Big Bridge"), "Battle at the Big Bridge (Fatal Push) is legal");
        }

        [Test]
        public void TestFlipLegal()
        {
            var format = new TestLegalityChecker(null, "https://pennydreadfulmtg.github.io/NEO_legal_cards.txt");
            Assert.That(format.IsCardLegal("Orochi Eggwatcher"));

            format = new TestLegalityChecker(null, "https://pennydreadfulmtg.github.io/KHM_legal_cards.txt");
            Assert.That(!format.IsCardLegal("Orochi Eggwatcher"));

        }
    }
}
