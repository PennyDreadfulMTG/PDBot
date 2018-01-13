using PDBot.API;
using PDBot.Core.Interfaces;
using PDBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.Tournaments
{
    class TournamentManager : ICronObject
    {
        public Dictionary<Gatherling.Event, Gatherling.Round> ActiveEvents { get; } = new Dictionary<Gatherling.Event, Gatherling.Round>();

        private IChatDispatcher chatDispatcher;
        public IChatDispatcher Chat { get { if (chatDispatcher == null) chatDispatcher = Resolver.Helpers.GetChatDispatcher(); return chatDispatcher; } }

        public async Task EveryMinute()
        {
            var events = await Gatherling.GatherlingDotCom.GetActiveEventsAsync();
            try
            {
                //events = events.Union(await Gatherling.PennyDreadful.GetActiveEventsAsync()).ToArray();
            }
#pragma warning disable CC0004 // Catch block cannot be empty
            catch (WebException)
            {

            }
#pragma warning restore CC0004 // Catch block cannot be empty
            foreach (var ae in events)
            {
                if (!ActiveEvents.ContainsKey(ae))
                {
                    ActiveEvents.Add(ae, new Gatherling.Round());
                }
                Gatherling.Round round;
                try
                {
                    round = await ae.GetCurrentPairings().ConfigureAwait(false);
                }
                catch (WebException c)
                {
                    throw new WebException($"Error retrieving {ae} round.", c);
                }
                if (round.RoundNum > ActiveEvents[ae].RoundNum)
                {
                    ActiveEvents[ae] = round;
                    PostPairings(ae, round);
                }
            }
        }

        private void PostPairings(Gatherling.Event eventModel, Gatherling.Round round)
        {
            var room = eventModel.Channel;
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
    }
}
