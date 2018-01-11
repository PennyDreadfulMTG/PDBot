using PDBot.API;
using PDBot.Core.Interfaces;
using PDBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.Tournaments
{
    class TournamentManager : ICronObject
    {
        public Dictionary<string, Gatherling.Round> ActiveEvents { get; } = new Dictionary<string, Gatherling.Round>();

        private IChatDispatcher chatDispatcher;
        public IChatDispatcher Chat { get { if (chatDispatcher == null) chatDispatcher = Resolver.Helpers.GetChatDispatcher(); return chatDispatcher; } }

        public async Task EveryMinute()
        {
            var events = await Gatherling.GatherlingDotCom.GetActiveEventsAsync();
            events = events.Union(await Gatherling.PennyDreadful.GetActiveEventsAsync()).ToArray();
            foreach (var ae in events)
            {
                if (!ActiveEvents.ContainsKey(ae))
                {
                    ActiveEvents.Add(ae, new Gatherling.Round());
                }
                var round = await Gatherling.GatherlingDotCom.GetCurrentPairings(ae);
                if (round.RoundNum > ActiveEvents[ae].RoundNum)
                {
                    ActiveEvents[ae] = round;
                    PostPairings(ae, round);
                }
            }
        }

        private void PostPairings(string eventName, Gatherling.Round round)
        {
            var series = eventName.Trim(' ', '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
            string room;
            room = RoomForSeries(series);
            if (room != null)
            {
                var builder = new StringBuilder();
                builder.Append($"[sD] Pairings for Round {round.RoundNum}:\n");
                var misses = 0;
                foreach (var pairing in round.Matches)
                {
                    if (pairing.Res == "BYE")
                    {
                        builder.Append("[sG] ");
                    }
                    else if (pairing.Res == "vs.")
                    {
                        builder.Append("[sR] ");
                    }
                    else
                    {
                        misses += 1;
                        builder.Append("[sT] ");
                    }
                    builder.Append(pairing.ToString());
                    builder.Append("\n");
                }
                if (misses == 0)
                {
                    int minutes = (DateTime.UtcNow.Minute + 10) % 60;
                    builder.Append($"[sPig] Free win time: XX:{minutes.ToString("D2")}!");
                }
                if (misses < 3)
                {
                    var sent = Chat.SendPM(room, builder.ToString());
                    if (!sent)
                    {
                        Chat.Join(room);
                        Chat.SendPM(room, builder.ToString());
                    }
                }
                // If misses >= 3, we have clearly just rebooted.  Don't send anything.
            }
        }

        private static string RoomForSeries(string series)
        {
            switch (series)
            {
                case "Penny Dreadful Thursdays":
                    return "#PDT";
                case "Penny Dreadful Saturdays":
                case "Penny Dreadful Sundays":
                    return "#PDS";
                case "Penny Dreadful Mondays":
                    return "#PDM";
                case "Classic Heirloom":
                    return "#heirloom";
                case "Community Legacy League":
                    return "#CLL";
                case "PauperPower":
                    return "#pauperpower";
                case "Modern Times":
                    return "#modern";
                case "Pauper Classic Tuesdays":
                    return "#pct";
                case "Vintage MTGO Swiss":
                    return "#vintageswiss";
                default:
                    break;
            }
            if (series.StartsWith("CLL Quarterly") || series.StartsWith("Community Legacy League"))
                return "#CLL";

            return null;
        }
    }
}
