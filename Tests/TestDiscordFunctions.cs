using NUnit.Framework;
using NUnit.Framework.Legacy;
using PDBot.Core;

namespace Tests
{
    class TestDiscordFunctions
    {
        [Test]
        public void SummarizeTournamentPairingsPreservesNoShowTime()
        {
            var pairings = "<:sEventTicket:1> Penny Dreadful Sundays 42.01\n"
                + "<:sD:2> Pairings for Round 1:\n"
                + "<:sR:3> alice vs. bob\n"
                + "<:sB:4> No-Show win time: XX:40\n"
                + "<:sD:2> Good luck, everyone!";

            var summary = DiscordFunctions.SummarizeTournamentPairings(pairings, "<@&123>");

            ClassicAssert.AreEqual(
                "<:sEventTicket:1> Penny Dreadful Sundays 42.01\n"
                + "<@&123> Check Gatherling for your pairings!\n"
                + "<:sB:4> No-Show win time: XX:40",
                summary);
        }
    }
}
