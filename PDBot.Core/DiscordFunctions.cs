using PDBot.Core.API;
using PDBot.Core.Interfaces;
using PDBot.Discord;
using PDBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core
{
    public class DiscordFunctions : ICronObject
    {
        Dictionary<string, long?> MtgoToDiscordMapping = new Dictionary<string, long?>();
        private ITournamentManager tournamentManager;

        ITournamentManager TournamentManager => tournamentManager ?? (tournamentManager = Resolver.Helpers.GetTournamentManager());

        public async Task EveryHourAsync()
        {
            await DoPDHRole();
            await WeeklyRecapAsync();
        }

        public Task EveryMinuteAsync()
        {
            return DoTournamentRoleAsync();
        }

        private async static Task WeeklyRecapAsync()
        {
            if (await DiscordService.CheckForPinnedMessageAsync())
                return;
            var stats = await LogsiteApi.GetStatsAsync();

            var PdGames = stats.Formats[MagicFormat.PennyDreadful.ToString()].LastWeek.NumMatches;
            var PdhGames = stats.Formats[MagicFormat.PennyDreadfulCommander.ToString()].LastWeek.NumMatches;

            var prevPdGames = stats.Formats[MagicFormat.PennyDreadful.ToString()].LastLastWeek?.NumMatches ?? 0;
            var prevPdhGames = stats.Formats[MagicFormat.PennyDreadfulCommander.ToString()].LastLastWeek?.NumMatches ?? 0;

            var players = stats.Formats[MagicFormat.PennyDreadful.ToString()].LastWeek.Players.Union(stats.Formats[MagicFormat.PennyDreadfulCommander.ToString()].LastWeek.Players);

            var sb = new StringBuilder();
            sb.Append($"In the last week, I saw {players.Count()} people from the Penny Dreadful community play {PdGames} Penny Dreadful matches");
            if (prevPdGames > 0)
            {
                var percent = ((PdGames - prevPdGames) / (double)prevPdGames);
                string pstr;
                if (percent >= 0)
                    pstr = $"up {percent.ToString("p0")}";
                else
                    pstr = $"down {Math.Abs(percent).ToString("p0")}";
                sb.Append($" ({pstr} from last week)");
            }

            sb.Append($" and {PdhGames} PDH games");
            if (prevPdhGames > 0)
            {
                var percent = ((PdhGames - prevPdhGames) / (double)prevPdhGames);
                string pstr;
                if (percent >= 0)
                    pstr = $"up {percent.ToString("p0")}";
                else
                    pstr = $"down {Math.Abs(percent).ToString("p0")}";
                sb.Append($" ({pstr} from last week)");

            }
            sb.Append(".");
            await DiscordService.SendToGeneralAsync(sb.ToString().Replace(" %", "%"), true);
        }

        private async Task<long?> DiscordIDAsync(string username)
        {
            if (MtgoToDiscordMapping.ContainsKey(username))
                return MtgoToDiscordMapping[username];
            var person = await DecksiteApi.GetPersonAsync(username);
            return MtgoToDiscordMapping[username] = person.discord_id;
        }

        private async Task DoPDHRole()
        {
            var stats = await LogsiteApi.GetStatsAsync();
            var pdh = stats.Formats[MagicFormat.PennyDreadfulCommander.ToString()];
            var players = await GetDiscordIDs(pdh.LastMonth.Players);
            await DiscordService.SyncRoleAsync(207281932214599682, "PDH", players);
        }

        private async Task<long?[]> GetDiscordIDs(IEnumerable<string> playerNames)
        {
            var tasks = playerNames.Select(DiscordIDAsync).ToArray();
            await Task.WhenAll(tasks);
            var players = tasks.Select(t => t.Result).Where(id => id != null).ToArray();
            return players;
        }

        private async Task DoTournamentRoleAsync()
        {
            var playerNames = new List<string>();
            foreach (var tournament in tournamentManager.ActiveEvents)
            {
                if (tournament.Key.Channel.StartsWith("PD", StringComparison.InvariantCultureIgnoreCase))
                {
                    // Only PD Tournaments
                    foreach (var m in tournament.Value.Matches)
                    {
                        playerNames.Add(m.A);
                        playerNames.Add(m.B);
                    }
                }
            }
            var playerIDs = await GetDiscordIDs(playerNames);
            await DiscordService.SyncRoleAsync(207281932214599682, "Tournament Players", playerIDs);
        }
    }
}
