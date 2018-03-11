using PDBot.Core.API;
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

        public async Task EveryHourAsync()
        {
            await DoPDHRole();
            await WeeklyRecapAsync();
        }

        private async Task WeeklyRecapAsync()
        {
            if (await DiscordService.CheckForPinnedMessageAsync())
                return;
            var stats = await LogsiteApi.GetStatsAsync();

            var PdGames = stats.Formats[MagicFormat.PennyDreadful.ToString()].LastWeek.NumMatches;
            var PdhGames = stats.Formats[MagicFormat.PennyDreadfulCommander.ToString()].LastWeek.NumMatches;

            var prevPdGames = stats.Formats[MagicFormat.PennyDreadful.ToString()].LastLastWeek?.NumMatches ?? 0;
            var prevPdhGames = stats.Formats[MagicFormat.PennyDreadfulCommander.ToString()].LastLastWeek?.NumMatches?? 0;

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

        private async Task DoPDHRole()
        {
            var stats = await LogsiteApi.GetStatsAsync();
            var pdh = stats.Formats[MagicFormat.PennyDreadfulCommander.ToString()];
            var tasks = pdh.LastMonth.Players.Select(DiscordIDAsync).ToArray();
            await Task.WhenAll(tasks);
            var players = tasks.Select(t => t.Result).Where(id => id != null).ToArray();
            await DiscordService.SyncRoleAsync(207281932214599682, "PDH", players);
        }

        private async Task<long?> DiscordIDAsync(string username)
        {
            if (MtgoToDiscordMapping.ContainsKey(username))
                return MtgoToDiscordMapping[username];
            var person = await DecksiteApi.GetPersonAsync(username);
            return MtgoToDiscordMapping[username] = person.discord_id;
        }

        public Task EveryMinute()
        {
            return Task.FromResult(false);
        }
    }
}
