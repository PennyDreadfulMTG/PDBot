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
        public async Task TestGatherlingDecks()
        {
            var deck = await GatherlingClient.GatherlingDotCom.GetDeckAsync(87052);
            Assert.AreEqual(true, deck.Found);
            Assert.AreEqual(87052, deck.Id);
            Assert.AreEqual("PD Drake", deck.Name);
        }

        [TestCase]
        public void TestVerification()
        {
            var code = GatherlingClient.PennyDreadful.GetVerificationCodeAsync(nameof(PDBot)).GetAwaiter().GetResult();
            Assert.IsNotNull(code);
        }

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
            Assert.That(pairings.Matches.Any());
            Assert.That(first.Channel != null);
        }

        [Test]
        public async Task ParseStandings()
        {
            var @event = await GatherlingClient.GatherlingDotCom.GetEvent("Penny Dreadful Thursdays 12.01");
            Assert.NotNull(@event.Standings);
        }
    }
}
