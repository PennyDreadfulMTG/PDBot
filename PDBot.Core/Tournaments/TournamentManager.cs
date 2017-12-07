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
        Dictionary<string, Gatherling.Round> ActiveEvents { get; set; } = new Dictionary<string, Gatherling.Round>();

        private IChatDispatcher chatDispatcher;
        public IChatDispatcher Chat { get { if (chatDispatcher == null) chatDispatcher = Resolver.Helpers.GetChatDispatcher(); return chatDispatcher; } }

        public async Task EveryMinute()
        {
            var events = await Gatherling.GatherlingDotCom.GetActiveEventsAsync();
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
                builder.Append($"[sD] Pairings for round {round.RoundNum}:\n");
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
                        builder.Append("[sT] ");
                    }
                    builder.Append(pairing.ToString());
                    builder.Append("\n");
                }
                Chat.SendPM(room, builder.ToString().Trim());
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
                default:
                    break;
            }
            return null;
        }
    }
}
