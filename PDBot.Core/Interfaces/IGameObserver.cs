using PDBot.Core.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PDBot.Core.Interfaces
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
        bool ShouldJoin(IMatch match);
        Task<IGameObserver> GetInstanceForMatchAsync(IMatch match);
        string HandleLine(GameLogLine gameLogLine);
        void ProcessWinner(string winner, int gameID);

        bool PreventReboot { get; }
    }

    public interface ILeagueObserver
    {
        bool IsLeagueGame { get; }
    }
}
