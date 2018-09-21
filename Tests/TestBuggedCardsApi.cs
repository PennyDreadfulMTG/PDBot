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
            Assert.Positive(BuggedCards.Bugs.Count);
            Assert.NotNull(BuggedCards.Bugs.First().CardName);
            Assert.NotNull(BuggedCards.Bugs.First().Classification);
            Assert.NotNull(BuggedCards.Bugs.First().Description);
            Assert.NotNull(BuggedCards.Bugs.First().LastConfirmed);
        }

        [Test]
        public async Task TestVerifications()
        {
            var LivingLore = await BuggedCards.GetVerificationForBugAsync(35);
            var currentBuild = await BuggedCards.GetCurrentBuildAsync();
            Assert.AreEqual(LivingLore.Major, 3);
            Assert.LessOrEqual(LivingLore, currentBuild);
        }
    }
}
