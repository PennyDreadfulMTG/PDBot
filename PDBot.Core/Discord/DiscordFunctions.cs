using Discord;
using Discord.Net;
using Discord.Rest;
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

        private const long PENNY_DREADFUL_GUILD_ID = 207281932214599682;
        private ITournamentManager m_tournamentManager;

        ITournamentManager TournamentManager => m_tournamentManager ?? (m_tournamentManager = Resolver.Helpers.GetTournamentManager());

        public async Task EveryHourAsync()
        {
            if (!Features.ConnectToDiscord)
                return;

            await WeeklyRecapAsync();
            await DoPDHRoleAsync();
        }

        public async Task EveryMinuteAsync()
        {
            if (!Features.ConnectToDiscord)
                return;

            await DoTournamentRoleAsync();
            await MakeVoiceRoomsAsync();

        }

        public async static Task WeeklyRecapAsync()
        {
            if (await DiscordService.CheckForPinnedMessageAsync())
                return;
            var stats = await LogsiteApi.GetStatsAsync();

            var PdGames = stats.Formats[MagicFormat.PennyDreadful.ToString()].LastWeek.NumMatches;
            var PdhGames = stats.Formats[MagicFormat.PennyDreadfulCommander.ToString()]?.LastWeek?.NumMatches ?? 0;

            var prevPdGames = stats.Formats[MagicFormat.PennyDreadful.ToString()].LastLastWeek?.NumMatches ?? 0;
            var prevPdhGames = stats.Formats[MagicFormat.PennyDreadfulCommander.ToString()].LastLastWeek?.NumMatches ?? 0;

            var players = stats.Formats[MagicFormat.PennyDreadful.ToString()].LastWeek.Players.Union(stats.Formats[MagicFormat.PennyDreadfulCommander.ToString()]?.LastWeek?.Players ?? new string[0]);

            var sb = new StringBuilder();
            sb.Append($"In the last week, I saw {players.Count()} people from the Penny Dreadful community play {PdGames} Penny Dreadful matches");
            if (prevPdGames > 0)
            {
                var percent = ((PdGames - prevPdGames) / (double)prevPdGames);
                var pstr = Math.Abs(percent).ToString("p0");
                pstr = percent >= 0 ? $"up {pstr}" : $"down {pstr}";
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

        public static async Task<ulong?> DiscordIDAsync(string username)
        {
            if (MtgoToDiscordMapping.ContainsKey(username))
                return MtgoToDiscordMapping[username];
            var person = await DecksiteApi.GetPersonAsync(username);
            return MtgoToDiscordMapping[username] = person.discord_id;
        }

        public static async Task<string> MentionOrElseNameAsync(string username)
        {
            var escaped = username.Replace("_", @"\_");
            var ID = await DiscordIDAsync(username);
            if (ID == null)
                return escaped;

            var member = DiscordService.client.GetGuild(PENNY_DREADFUL_GUILD_ID).GetUser(ID.Value);
            if (member != null && (member.Nickname ?? member.Username).ToLower().Contains(username.ToLower()))
                return $"<@{ID}>";
            return $"<@{ID}> ({escaped})";
        }

        internal static async Task<bool> Mentionable(string username)
        {
            return await DiscordIDAsync(username) != null;
        }

        public async static Task DoPDHRoleAsync()
        {
            var stats = await LogsiteApi.GetStatsAsync();
            var pdh = stats.Formats[MagicFormat.PennyDreadfulCommander.ToString()];
            var players = await GetDiscordIDsAsync(pdh.LastMonth.Players);
            await DiscordService.SyncRoleAsync(PENNY_DREADFUL_GUILD_ID, "PDH", players);
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
                        waiting_on.AddRange(tournament.Key.Unreported.Where(p => !TournamentManager.ActiveMatches.SelectMany(m => m.Players).Contains(p, StringComparer.CurrentCultureIgnoreCase)));
                }
            }
            var playerIDs = await GetDiscordIDsAsync(playerNames);
            await DiscordService.SyncRoleAsync(PENNY_DREADFUL_GUILD_ID, "Tournament Players", playerIDs);
            await DiscordService.SyncRoleAsync(PENNY_DREADFUL_GUILD_ID, "Waiting On", await GetDiscordIDsAsync(waiting_on));
        }

        public async static Task MakeVoiceRoomsAsync()
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
                        await DiscordService.SendToArbiraryChannelAsync("Voice Channels overflowed, disabling.", 230056266938974218);
                        Features.CreateVoiceChannels = false;
                    }
                    catch (HttpException c)
                    {
                        await DiscordService.SendToArbiraryChannelAsync("Voice Channels overflowed, disabling.", 230056266938974218);
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

        internal static async Task PostTournamentPairingsAsync(ulong ChanId, string pairingsText, string doorPrize, string preamble)
        {
            var TournamentRoom = DiscordService.FindChannel(ChanId);
            pairingsText = DiscordService.SubstituteEmotes(pairingsText, TournamentRoom.Guild);
            if (!string.IsNullOrEmpty(preamble) && (preamble.Length + pairingsText.Length < 2000))
            {
                pairingsText = preamble + '\n' + pairingsText;
                preamble = null;
            }
            if (pairingsText.Length >= 2000)
            {
                pairingsText = pairingsText.Split('\n')[0] + "\n@Tournament Players Check Gatherling for your pairings!";
            }
            var expected_round = pairingsText.Split('\n')[0];
            var pinned = await TournamentRoom.GetPinnedMessagesAsync();
            foreach (var pin in pinned)
            {
                Console.WriteLine($"pinned post: {pin}");
                var post = pin as RestUserMessage;
                if (post.Author.Id != DiscordService.client.CurrentUser.Id)
                    continue;
                var round = post.Content.Split('\n')[0];
                var eq = round == expected_round ? "=" : "!=";
                Console.WriteLine($"\"{round}\"{eq}\"{expected_round}\"");
                if (round == expected_round)
                {
                    if (post.Content != pairingsText)
                    {
                        await post.ModifyAsync(m => m.Content = pairingsText);
                        Console.WriteLine("Updating");
                    }
                    return;
                }
                await post.UnpinAsync();
                Console.WriteLine("unpinning");
            }
            if (!string.IsNullOrEmpty(preamble))
                await TournamentRoom.SendMessageAsync(preamble);
            var msg = await TournamentRoom.SendMessageAsync(pairingsText);
            await msg.PinAsync();
            if (!string.IsNullOrEmpty(doorPrize))
                await DiscordService.SendToTournamentRoomAsync(doorPrize);
        }
    }
}
