using Gatherling;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using PDBot.API;
using PDBot.Core.Tournaments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    class TestGatherlingAPI
    {
        [TestCase]
        public async Task TestGatherlingDecks()
        {
            var deck = await GatherlingClient.GatherlingDotCom.GetDeckAsync(87052);
            ClassicAssert.AreEqual(true, deck.Found);
            ClassicAssert.AreEqual(87052, deck.Id);
            ClassicAssert.AreEqual("PD Drake", deck.Name);
        }

        //[TestCase]
        //public void TestVerification()
        //{
        //    var code = GatherlingClient.PennyDreadful.GetVerificationCodeAsync(nameof(PDBot)).GetAwaiter().GetResult();
        //    ClassicAssert.IsNotNull(code);
        //}

        [Theory]
        public async Task GetActiveEvents()
        {
            var events = new Gatherling.Models.Event[0];
            if (GatherlingClient.GatherlingDotCom.ApiVersion > 0)
                events = await GatherlingClient.GatherlingDotCom.GetActiveEventsAsync();

            if (events.Length == 0)
                events = await GatherlingClient.PennyDreadful.GetActiveEventsAsync();
            Assume.That(events.Length > 0);
            var first = events.First();
            var pairings = await first.GetCurrentPairingsAsync();
            ClassicAssert.That(pairings.Matches.Any());
            ClassicAssert.That(first.Channel != null);
        }

        [Test]
        public async Task ParseStandings()
        {
            var @event = await GatherlingClient.GatherlingDotCom.GetEvent("Penny Dreadful Thursdays 12.01");
            ClassicAssert.NotNull(@event.Standings);
        }
    }
}
