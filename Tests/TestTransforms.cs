using NUnit.Framework;
using NUnit.Framework.Legacy;
using PDBot.Core.GameObservers;

namespace Tests
{
    class TestTransforms
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
    }
}
