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

        public Task EveryHour()
        {
            return DoPDHRole();
        }

        private async Task DoPDHRole()
        {
            var stats = await LogsiteApi.GetStatsAsync();
            var pdh = stats.Formats[MagicFormat.PennyDreadfulCommander.ToString()];
            var tasks = pdh.LastMonth.Players.Select(DiscordIDAsync).ToArray();
            await Task.WhenAll(tasks);
            var players = tasks.Select(t => t.Result).Where(id => id != null).ToArray();
            await DiscordService.SyncRole(207281932214599682, "PDH", players);
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
