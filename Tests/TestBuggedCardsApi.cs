using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit;
using NUnit.Framework;
using PDBot.API;

namespace Tests
{
    class TestBuggedCardsApi
    {
        [Test]
        public void TestBuggedCards()
        {
            BuggedCards.CheckForNewList();
            Assert.Positive(BuggedCards.Bugs.Count);
        }
    }
}
