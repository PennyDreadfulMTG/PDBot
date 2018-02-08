using System.Collections.Generic;
using Gatherling.Models;

namespace PDBot.Core.Tournaments
{
    public interface ITournamentManager
    {
        Dictionary<Event, Round> ActiveEvents { get; }
    }
}