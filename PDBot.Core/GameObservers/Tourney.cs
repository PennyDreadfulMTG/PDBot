using PDBot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDBot.Core.Data;

namespace PDBot.Core.GameObservers
{
    class Tourney : IGameObserver
    {
        private IMatch match;

        public Tourney()
        {

        }

        public Tourney(IMatch match)
        {
            this.match = match;
        }

        public bool PreventReboot => true;

        public async Task<IGameObserver> GetInstanceForMatchAsync(IMatch match)
        {
            if (ShouldJoin(match))
            {
                return new Tourney(match);
            }
            return null;
        }

        public string HandleLine(GameLogLine gameLogLine)
        {
            return null;
        }

        public void ProcessWinner(string winner, int gameID)
        {
            var channel = GetChannel();
            if (channel == null)
                return;
            match.Winners.GetRecordData(out var first, out var record);
            if (first.Wins == 2)
            {
                var loser = match.Players.FirstOrDefault(d => d != winner);
                Resolver.Helpers.GetChatDispatcher().SendPM(channel, $"[sD] {winner} {record} {loser}");
            }
        }

        private string GetChannel()
        {
            string channel = null;
            if (match.Format == MagicFormat.Heirloom)
            {
                channel = "#heirloom";
            }
            else if (match.Format == MagicFormat.PennyDreadful)
            {
                TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
                if (now.DayOfWeek == DayOfWeek.Sunday)
                    channel = "#PDS";
                else if (now.DayOfWeek == DayOfWeek.Monday)
                    channel = "#PDM";
                else if (now.DayOfWeek == DayOfWeek.Thursday)
                    channel = "#PDT";
            }
            return channel;
        }

        public bool ShouldJoin(IMatch match)
        {
            if (match.GameRoom != Room.GettingSerious)
                return false;
            return match.Format == MagicFormat.PennyDreadful || match.Format == MagicFormat.Heirloom;
        }
    }
}
