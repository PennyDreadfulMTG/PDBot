using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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
            Assume.That(BuggedCards.Bugs.Any());
            var bug = BuggedCards.Bugs.FirstOrDefault();
            ClassicAssert.NotNull(bug);
            ClassicAssert.NotNull(bug.CardName);
            ClassicAssert.NotNull(bug.Classification);
            ClassicAssert.NotNull(bug.Description);
            ClassicAssert.NotNull(bug.LastConfirmed);
        }
    }
}
