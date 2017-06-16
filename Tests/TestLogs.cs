using NUnit.Framework;
using PDBot.Core.GameObservers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    class TestLogs
    {
        [Test]
        public void TestLogHandler()
        {
            var line = "skinnersweet1880 is being attacked by [Vampire] token, [Vampire] token, [Vampire Nighthawk], [Vampire Lacerator], and [Lord of Lineage].";
            var parser = new GameLogLine(line);
            Assert.AreEqual(3, parser.Cards.Count());
            Assert.AreEqual(2, parser.Tokens.Count());

        }
    }
}
