using NUnit.Framework;
using PDBot.Core.GameObservers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
