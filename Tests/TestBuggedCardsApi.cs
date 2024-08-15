using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit;
using NUnit.Framework;
using PDBot.API;
using PDBot.Core.API;

namespace Tests
{
    class TestBuggedCardsApi
    {
        [Test]
        public void TestBuggedCards()
        {
            BuggedCards.CheckForNewList();
            var bug = BuggedCards.Bugs.FirstOrDefault();
            Assert.NotNull(bug);
            Assert.NotNull(bug.CardName);
            Assert.NotNull(bug.Classification);
            Assert.NotNull(bug.Description);
            Assert.NotNull(bug.LastConfirmed);
        }
    }
}
