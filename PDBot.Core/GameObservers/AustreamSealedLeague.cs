using PDBot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDBot.Core.Data;
using PDBot.Discord;

namespace PDBot.Core.GameObservers
{
    // This is an observer that I'm using for the MTG Austream sealed showdown.
    // It's also a nice simple class that I can use as an example.
    class AustreamSealedLeague : IGameObserver
    {
        /// <summary>
        /// This is a list of the players we care about.
        /// Due to a number of reasons, I'm just hardcoding it.
        /// </summary>
        string[] AustreamMembers = 
            {
            "voitstarr",        // CheshirePlaysGames
            "LadyDanger",       // xLadyDangerx
            "Draftaholics",     // DraftaholicsAnonymous
            "silasary",         // silasary
            "gem-of-magic",     // gemofmagic
            "mull-to-3",        // mullto3
            "meglin",           // CardScience
            "CoopDeGrace",      // WholeBoxAndDice
        };

        private IMatch match;

        /// <summary>
        /// We need an empty constructor.  This will be used when generating the prototype instance
        /// </summary>
        public AustreamSealedLeague()
        {

        }

        /// <summary>
        /// We also need a constructor that takes a match.
        /// We use this below.
        /// </summary>
        /// <param name="match"></param>
        public AustreamSealedLeague(IMatch match)
        {
            this.match = match;
        }

        /// <summary>
        /// While this game is ongoing, don't allow the bot to reboot for upgrades.
        /// </summary>
        public bool PreventReboot => true;

        /// <summary>
        /// When a match is joined (Regardless of our response to ShouldJoin), this method will be called.
        /// We either return an instanced observer, or null, depending on whether we actually need to watch.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public async Task<IGameObserver> GetInstanceForMatchAsync(IMatch match)
        {
            // In this case, we use the same logic as we did for joining.  Very simple.
            if (ShouldJoin(match))
            {
                return new AustreamSealedLeague(match);
            }
            return null;
        }

        /// <summary>
        /// This is triggered every time a line is sent through the game log.
        /// If we have something to say, return a string.
        /// </summary>
        /// <param name="gameLogLine"></param>
        /// <returns></returns>
        public string HandleLine(GameLogLine gameLogLine)
        {
            // Don't care.
            return null;
        }

        /// <summary>
        /// Triggers every time a game is won.
        /// </summary>
        /// <param name="winner"></param>
        /// <param name="gameID"></param>
        public void ProcessWinner(string winner, int gameID)
        {
            // Wait until someone has won two games, then report the results to Discord.
            match.Winners.GetRecordData(out var first, out var record);
            if (first.Wins == 2)
            {
                var loser = match.Players.FirstOrDefault(d => d != winner);
                DiscordService.SendToArbiraryChannel($":trophy: {winner} {record} {loser}", "MTG Austream", 291179039291473920);
            }
        }

        /// <summary>
        /// Check whether this observer cares enough to trigger joining a game.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public bool ShouldJoin(IMatch match)
        {
            if (match.Format != MagicFormat.Freeform)
            {
                return false;
            }
            else
            {
                // If all players are part of Austream
                var r = match.Players.All(p => AustreamMembers.Contains(p, StringComparer.CurrentCultureIgnoreCase));
                if (r)
                    return true;
                //var not = match.Players.Where(p => !AustreamMembers.Contains(p, StringComparer.CurrentCultureIgnoreCase));
                //foreach (var person in not)
                //{
                //    Console.WriteLine($"{person} is not in Austream.");
                //}
                return false;
            }
        }
    }
}
