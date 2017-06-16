using System;
using System.Collections.Generic;

namespace PDBot.Core.GameObservers
{
    public enum Room
    {
        Invalid = 0,
        JustStartingOut = 1, // We don't actually look in this room right now.  Let me know if you want it.
        JustForFun = 2,
        GettingSerious = 3,
        TournamentPractice = 4 // Same goes for this one.
    };

    public interface IGameObserver
    {
        bool IsApplicable(string comment, MagicFormat format, Room room);
        IGameObserver GetInstanceForMatch(IMatch match);
        string HandleLine(GameLogLine gameLogLine);
        void ProcessWinner(string winner, int gameID);
    }
}
