using Discord;
using Discord.WebSocket;
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
        static Dictionary<string, ulong?> MtgoToDiscordMapping { get; } = new Dictionary<string, ulong?>();
        private ITournamentManager m_tournamentManager;

        ITournamentManager TournamentManager => m_tournamentManager ?? (m_tournamentManager = Resolver.Helpers.GetTournamentManager());

        public async Task EveryHourAsync()
        {
            await WeeklyRecapAsync();
            await DoPDHRole();
        }

        public async Task EveryMinuteAsync()
        {
            await DoTournamentRoleAsync();
            await MakeVoiceRoomsAsync();

        }

        public async static Task WeeklyRecapAsync()
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
            await DiscordService.SendToAnnouncementsAsync(sb.ToString().Replace(" %", "%"));
        }

        public static async Task<ulong?> DiscordIDAsync(string username)
        {
            if (MtgoToDiscordMapping.ContainsKey(username))
                return MtgoToDiscordMapping[username];
            var person = await DecksiteApi.GetPersonAsync(username);
            return MtgoToDiscordMapping[username] = person.discord_id;
        }

        public static async Task<string> MentionOrElseNameAsync(string username)
        {
            var ID = await DiscordIDAsync(username);
            if (ID == null)
                return username;

            var member = DiscordService.client.GetGuild(207281932214599682).GetUser(ID.Value);
            if (member != null && (member.Nickname ?? member.Username).ToLower().Contains(username.ToLower()))
                return $"<@{ID}>";
            return $"<@{ID}> ({username})";
        }

        public async Task DoPDHRole()
        {
            var stats = await LogsiteApi.GetStatsAsync();
            var pdh = stats.Formats[MagicFormat.PennyDreadfulCommander.ToString()];
            var players = await GetDiscordIDsAsync(pdh.LastMonth.Players);
            await DiscordService.SyncRoleAsync(207281932214599682, "PDH", players);
        }

        private async static Task<ulong?[]> GetDiscordIDsAsync(IEnumerable<string> playerNames)
        {
            var tasks = playerNames.Select(DiscordIDAsync).ToArray();
            await Task.WhenAll(tasks);
#pragma warning disable AsyncFixer02 // Long running or blocking operations under an async method
            var players = tasks.Select(t => t.Result).Where(id => id != null).ToArray();
#pragma warning restore AsyncFixer02 // Long running or blocking operations under an async method
            return players;
        }

        public async Task DoTournamentRoleAsync()
        {
            var playerNames = new List<string>();
            var waiting_on = new List<string>();
            foreach (var tournament in TournamentManager.ActiveEvents)
            {
                // Only PD Tournaments
                if (tournament.Key.Channel.StartsWith("#PD", StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (var m in tournament.Value.Matches)
                    {
                        playerNames.Add(m.A);
                        playerNames.Add(m.B);
                    }
                    if (tournament.Key.Unreported != null)
                        waiting_on.AddRange(tournament.Key.Unreported.Where(p => !TournamentManager.ActiveMatches.SelectMany(m => m.Players).Contains(p)));
                }
            }
            var playerIDs = await GetDiscordIDsAsync(playerNames);
            await DiscordService.SyncRoleAsync(207281932214599682, "Tournament Players", playerIDs);
            await DiscordService.SyncRoleAsync(207281932214599682, "Waiting On", await GetDiscordIDsAsync(waiting_on));
        }

        public async Task MakeVoiceRoomsAsync()
        {
            string MatchEmoji(IMatch match)
            {
                if (match.Observers.OfType<GameObservers.Tourney>().SingleOrDefault() is GameObservers.Tourney)
                {
                    return "📅 ";
                }
                if (match.Observers.OfType<GameObservers.LeagueObserver>().SingleOrDefault() is GameObservers.LeagueObserver l)
                {
                    if (l.IsLeagueGame)
                        return "🏆 ";
                }
                if (match.Players.Count() > 2)
                {
                    return "⚔ ";
                }
                if (match.Players.Count() == 1)
                {
                    return "🃏 ";
                }
                return "🍵 ";
            }
            bool IsGenerated(IVoiceChannel chan)
            {
                if (chan.Name.StartsWith("📅") || chan.Name.StartsWith("🏆") || chan.Name.StartsWith("🍵") || chan.Name.StartsWith("⚔") || chan.Name.StartsWith("🃏"))
                    return true;
                return false;
            }

            var Games = Resolver.Helpers.GetGameList().ActiveMatches
                .Where(m => !m.Completed)
                .Where(m => m.Format == MagicFormat.PennyDreadful || m.Format == MagicFormat.PennyDreadfulCommander).ToArray();
            var ActiveCategory = DiscordService.client.GetChannel(492518614272835594) as SocketCategoryChannel;
            var expected = Games.Select(m => MatchEmoji(m) + string.Join(" vs ", m.Players)).ToArray();

            var toDelete = ActiveCategory.Guild.VoiceChannels.Where(c => !expected.Contains(c.Name)).ToArray();
            var toCreate = expected.Where(n => ActiveCategory.Guild.VoiceChannels.FirstOrDefault(c => c.Name == n) == null).ToArray();

            var dumpChan = ActiveCategory.Guild.VoiceChannels.First();

            if (Features.CreateVoiceChannels)
            {
                foreach (var name in toCreate)
                {
                    Console.WriteLine($"Creating VC: {name}");
                    var chan = await ActiveCategory.Guild.CreateVoiceChannelAsync(name);
                    try
                    {
                        await chan.ModifyAsync(x => x.CategoryId = ActiveCategory.Id);
                    }
                    catch (AggregateException)
                    {
                        await DiscordService.SendToTestAsync("Voice Channels overflowed, disabling.");
                        Features.CreateVoiceChannels = false;
                    }

                    var players = name.Split(new string[] { " vs " }, StringSplitOptions.RemoveEmptyEntries);
                    var users = (await GetDiscordIDsAsync(players)).Where(l => l.HasValue).Select(l => l.Value).ToArray();
                    var AllUsers = ActiveCategory.Guild.VoiceChannels.SelectMany(vc => vc.Users);
                    foreach (var c in AllUsers)
                    {
                        if (users.Contains(c.Id))
                        {
                            await c.ModifyAsync(u => u.Channel = chan);
                        }
                    }
                }
            }

            foreach (var chan in toDelete)
            {
                if (chan is SocketVoiceChannel vc && IsGenerated(chan))
                {
                    foreach (var user in vc.Users)
                    {
                        await user.ModifyAsync(u => u.Channel = dumpChan);
                    }
                    Console.WriteLine($"Deleting {chan.Name}");
                    await chan.DeleteAsync();
                }
            }

            foreach (var dup in ActiveCategory.Guild.VoiceChannels.GroupBy(c => c.Name))
            {
                if (dup.Count() > 1)
                {
                    var c = dup.First();
                    if (IsGenerated(c))
                        await c.DeleteAsync();
                }
            }
        }
    }
}
