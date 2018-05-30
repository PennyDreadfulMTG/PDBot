using System.Collections.Generic;
using Gatherling.Models;

namespace PDBot.Core.Interfaces
{
    public interface ITournamentManager
    {
        Dictionary<Event, Round> ActiveEvents { get; }

        List<IMatch> ActiveMatches { get; }
    }
}
