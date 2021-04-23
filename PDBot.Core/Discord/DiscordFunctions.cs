using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Gatherling.Models;
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
            try
            {
                var person = await DecksiteApi.GetPersonAsync(username);
                return MtgoToDiscordMapping[username] = person.discord_id;
            }
            catch (Exception c)
            {
                Sentry.SentrySdk.CaptureException(c);
                return null;
            }
        }

        public static async Task<string> MentionOrElseNameAsync(Person person, SocketGuild guild)
        {
            if (person.DiscordId.HasValue)
            {
                ulong id = (ulong)person.DiscordId.Value;
                if (guild?.GetUser(id) == null)
                {
                    var escaped = person.Name.Replace("_", @"\_");
                    return $"<@{id}> ({escaped})";
                }
                return $"<@{id}>";
            }
            else if (!string.IsNullOrEmpty(person.MtgoUsername))
                return await MentionOrElseNameAsync(person.MtgoUsername);
            else
                return await MentionOrElseNameAsync(person.Name);
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
            await DiscordService.SyncRoleAsync(PENNY_DREADFUL_GUILD_ID, "Recent PDH", players);
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

        public static ulong? GetDiscordChannel(Event eventModel)
        {
            ulong? ChanId = null;

            if (eventModel.Series.Contains("Penny Dreadful"))
                ChanId = 334220558159970304;
            else if (eventModel.Series.Contains("7 Point"))
                ChanId = 600281000739733514;
            else if (eventModel.Series.Contains("Community Legacy League") || eventModel.Series.Contains("Community Modern League"))
                ChanId = 750017068392513612;
            else if (eventModel.Series.StartsWith("Pennylander"))
                ChanId = 733261347894329345;
            else if (eventModel.Series == "Magic Online Society Monthly Series")
                ChanId = 746007224148688966;
            else if (eventModel.Series == "Pauper Classic Tuesdays")
                ChanId = 387127632266788870;
            return ChanId;
        }

        List<ulong> RememberedTournamentChannels { get; } = new List<ulong>()
        {
            334220558159970304,
            750017068392513612,
        };

        public async Task DoTournamentRoleAsync()
        {
            var tournament_players = new Dictionary<ulong, List<string>>();
            var waiting_on = new Dictionary<ulong, List<string>>();

            void setup(ulong channel)
            {
                tournament_players[channel] = new List<string>();
                waiting_on[channel] = new List<string>();
            }

            foreach (var channel in RememberedTournamentChannels)
            {
                setup(channel);
            }

            foreach (var tournament in TournamentManager.ActiveEvents)
            {
                var channelID = GetDiscordChannel(tournament.Key);
                if (!channelID.HasValue)
                    continue;

                if (!tournament.Value.IsFinals && tournament.Key.Main.Mode == EventStructure.League)
                {
                    // Do League things.
                    continue;
                }

                if (!tournament_players.ContainsKey(channelID.Value))
                {
                    setup(channelID.Value);
                }

                foreach (var m in tournament.Value.Matches)
                {
                    tournament_players[channelID.Value].Add(m.A);
                    tournament_players[channelID.Value].Add(m.B);
                }

                if (tournament.Key.Unreported != null)
                    waiting_on[channelID.Value].AddRange(tournament.Key.Unreported.Where(p => !TournamentManager.ActiveMatches.SelectMany(m => m.Players).Contains(p, StringComparer.CurrentCultureIgnoreCase)));
            }

            foreach (var channel in tournament_players)
            {
                var playerIDs = await GetDiscordIDsAsync(channel.Value);
                var remember = await DiscordService.SyncRoleAsync(channel.Key, "Tournament Players", playerIDs);
                remember = await DiscordService.SyncRoleAsync(channel.Key, "Waiting On", await GetDiscordIDsAsync(waiting_on[channel.Key])) || remember;
                if (remember && !RememberedTournamentChannels.Contains(channel.Key))
                    RememberedTournamentChannels.Add(channel.Key);
            }
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
                    try
                    {
                        await chan.DeleteAsync();
                    }
                    catch (HttpException)
                    {

                    }
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
            if (TournamentRoom == null)
                return;
            pairingsText = DiscordService.SubstituteEmotes(pairingsText, TournamentRoom.Guild);
            if (!string.IsNullOrEmpty(preamble) && (preamble.Length + pairingsText.Length < 2000))
            {
                pairingsText = preamble + '\n' + pairingsText;
                preamble = null;
            }
            if (pairingsText.Length >= 2000)
            {
                pairingsText = pairingsText.Split('\n')[0] + $"\n{AtTournamentPlayers(TournamentRoom.Guild)} Check Gatherling for your pairings!";
            }

            string[] lines = pairingsText.Split('\n');
            var expected_round = lines[0];
            if (expected_round.StartsWith("Welcome to"))
                expected_round = lines[1];


            var pinned = await TournamentRoom.GetPinnedMessagesAsync();
            foreach (var pin in pinned)
            {
                Console.WriteLine($"pinned post: {pin}");
                var post = pin as RestUserMessage;
                if (post.Author.Id != DiscordService.client.CurrentUser.Id)
                    continue;
                lines = post.Content.Split('\n');
                var round = lines[0];
                if (round.StartsWith("Welcome to"))
                    round = lines[1];

                var eq = round == expected_round ? "=" : "!=";

                if (round.StartsWith("<:sEventTicket:") && expected_round.StartsWith("<:sEventTicket:")) // Overlap
                {
                    Console.WriteLine($"\"{round}\"{eq}\"{expected_round}\"");
                    if (round != expected_round)
                        continue;
                    expected_round = pairingsText.Split('\n')[1];
                    round = post.Content.Split('\n')[1];
                    eq = round == expected_round ? "=" : "!=";
                }

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

        private static object AtTournamentPlayers(SocketGuild guild)
        {
            foreach (var role in guild.Roles)
            {
                if (role.Name == "Tournament Players")
                    return $"<@&{role.Id}>";
            }
            return "@Tournament Players";
        }
    }
}
