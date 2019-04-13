using Gatherling;
using NUnit.Framework;
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
        [OneTimeSetUp]
        public void Setup()
        {
            GatherlingClient.PasskeyProvider = new TournamentManager.InfoBotSettings();
        }

        [TestCase]
        public void TestGatherlingDecks()
        {
            var deck = GatherlingClient.PennyDreadful.GetDeckAsync(10564).GetAwaiter().GetResult();
            Assert.AreEqual(true, deck.Found);
            Assert.AreEqual(10564, deck.Id);
            Assert.AreEqual(" Unclassified", deck.Name);
        }

        [TestCase]
        public void TestVerification()
        {
            var code = GatherlingClient.PennyDreadful.GetVerificationCodeAsync(nameof(PDBot)).GetAwaiter().GetResult();
            Assert.IsNotNull(code);
        }

        [Theory]
        public void GetActiveEvents()
        {
            var events = new Gatherling.Models.Event[0];
            if (GatherlingClient.GatherlingDotCom.ApiVersion > 0)
                events = GatherlingClient.GatherlingDotCom.GetActiveEventsAsync().GetAwaiter().GetResult();
            if (events.Length == 0)
                events = GatherlingClient.One.GetActiveEventsAsync().GetAwaiter().GetResult();

            if (events.Length == 0)
                events = GatherlingClient.PennyDreadful.GetActiveEventsAsync().GetAwaiter().GetResult();
            Assume.That(events.Length > 0);
            var first = events.First();
            var pairings = first.GetCurrentPairingsAsync().GetAwaiter().GetResult();
            Assume.That(pairings.Matches.Any());
            Assume.That(first.Channel != null);
        }
    }
}
