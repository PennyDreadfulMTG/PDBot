using NUnit.Framework;
using PDBot.Core.GameObservers;

namespace Tests
{
    class TestTransforms
    {
        [Test]
        public void TestRearFaces()
        {
            Assert.IsFalse(BaseLegalityChecker.IsRearFace("Delver of Secrets"));
            Assert.IsTrue(BaseLegalityChecker.IsRearFace("Insectile Aberration"));
        }

        [Test]
        public void TestMelds()
        {
            Assert.IsFalse(BaseLegalityChecker.IsRearFace("Graf Rats"));
            Assert.IsTrue(BaseLegalityChecker.IsRearFace("Chittering Host"));
        }

        [Test]
        public void TestAdventure()
        {
            Assert.IsFalse(BaseLegalityChecker.IsRearFace("Smitten Swordmaster"));
            Assert.IsTrue(BaseLegalityChecker.IsRearFace("Curry Favor"));
        }
        [Test]
        public void TestMDFC()
        {
            Assert.IsTrue(BaseLegalityChecker.IsRearFace("Akoum Teeth"));
            Assert.IsTrue(BaseLegalityChecker.IsRearFace("Flamethrower Sonata"));
            Assert.IsTrue(BaseLegalityChecker.IsRearFace("The Ringhart Crest"));
        }

        [Test]
        public void TestFlip()
        {
            Assert.IsTrue(BaseLegalityChecker.IsRearFace("Erayo's Essence"));
            Assert.IsTrue(BaseLegalityChecker.IsRearFace("Erayo, Soratami Ascendant's Essence"));
        }

        [Test]
        public void TestDungeon()
        {
            Assert.IsTrue(BaseLegalityChecker.IsRearFace("Tomb of Annihilation"));
        }
    }
}
