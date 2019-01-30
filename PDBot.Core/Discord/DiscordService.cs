using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Discord.Commands;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Threading;
using Discord.Rest;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net.Http;
using PDBot.Core;
using PDBot.Core.API;
using Microsoft.Extensions.DependencyInjection;
using PDBot.Core.Discord;

namespace PDBot.Discord
{
    public static class DiscordService
    {

        // https://discordapp.com/oauth2/authorize?client_id=227647606149480449&scope=bot&permissions=270400

        internal static DiscordSocketClient client;
        private static CommandService commands;
        private static IServiceProvider services;


        public static event EventHandler Ready;

        private static bool Initialized;

        private static IServiceProvider ConfigureServices() => new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig()))
                .AddSingleton(new CommandService(new CommandServiceConfig { DefaultRunMode = RunMode.Async }))
                .BuildServiceProvider();

        public static async Task InitAsync(string token)
        {
            if (Initialized)
                return;
            Initialized = true;
            services = ConfigureServices();
            client = services.GetRequiredService<DiscordSocketClient>();
            commands = services.GetRequiredService<CommandService>();
            await commands.AddModuleAsync<DiscordCommands>();
            client.Log += Client_LogAsync;
            client.Ready += Client_ReadyAsync;
            client.Disconnected += Client_DisconnectedAsync;
            client.MessageReceived += Client_MessageReceivedAsync;
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
        }

        internal static async Task SyncRoleAsync(ulong serverID, string RoleName, ulong?[] users, bool remove = true)
        {
            var server = client.GetGuild(serverID);
            var role = server.Roles.FirstOrDefault(r => r.Name == RoleName);
            if (role == null)
                throw new NullReferenceException($"Could not find role '{RoleName}'");
            var changes = 0;
            const int MAX_CHANGES = 4;
            if (remove)
            {
                var toRemove = role.Members.Where(m => !users.Contains(m.Id));
                foreach (var rem in toRemove)
                {
                    Console.WriteLine($"Removing {rem.Username} from {RoleName}");
                    await rem.RemoveRoleAsync(role);
                    if (changes++ > MAX_CHANGES)
                        return;
                }
            }
            var toAdd = server.Users.Where(u => users.Contains(u.Id) && !u.Roles.Contains(role));
            foreach (var rem in toAdd)
            {
                Console.WriteLine($"Adding {rem.Username} to {RoleName}");
                await rem.AddRoleAsync(role);
                if (changes++ > MAX_CHANGES)
                    return;
            }
        }

        public static string Playing { get; private set; }
        public static string CurrentAvatar { get; private set; } = "unknown";
        public static bool Connected => client.ConnectionState == ConnectionState.Connected;

        private static async Task Client_MessageReceivedAsync(SocketMessage arg)
        {
            if (arg.Author.IsBot)
            {
                return;
            }

            var words = arg.Content.Split();

            if (arg.Content.StartsWith("#"))
            {
                var username = arg.Author.Username;
                if (arg.Author is IGuildUser gu && !string.IsNullOrWhiteSpace(gu.Nickname))
                    username = gu.Nickname;
                var content = arg.Content.Substring(arg.Content.IndexOf(' ')).Trim();
                var success = Resolver.Helpers.GetChatDispatcher().SendPM(words[0], $"【Discord】 {username}: {content}");
                if (success && arg.Content.Length > 200)
                {
                    try
                    {
                        await arg.DeleteAsync(new RequestOptions
                        {
                            AuditLogReason = "Cleaning up an overly long message after we echoed it."
                        });
                    }
                    catch (Exception)
                    {
                        // Shrug.
                        // This usually means we don't have permission to Moderate messages on the server.
                    }
                }
                else if (!success && arg is SocketUserMessage msg)
                {
                    await msg.AddReactionAsync(new Emoji("📵"));
                }
                return;
            }

            var channel = arg.Channel;
            if (arg is SocketUserMessage message)
            {
                var argPos = 0;
                if (message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))
                {
                    var context = new SocketCommandContext(client, message);
                    var result = await commands.ExecuteAsync(context, argPos, services);
                    if (result.IsSuccess)
                        return;
                    if (result.Error.HasValue && result.Error.Value != CommandError.UnknownCommand)
                    {
                        await context.Channel.SendMessageAsync(result.ToString());
                        return;
                    }
                }
            }

            if (words.FirstOrDefault().ToLower() == "!log")
            {
                var id = words.Skip(1).FirstOrDefault().ToString();
                await SendLogToChannelAsync(channel, id).ConfigureAwait(false);
                return;
            }

            if (arg.Content.ToLower() == "!avatar")
            {
                await channel.SendMessageAsync($"My current Avatar is {CurrentAvatar}.").ConfigureAwait(false);
                return;
            }
        }

        public async static Task<bool> CheckForPinnedMessageAsync()
        {
            var pinned = await FindChannel(207281932214599682).GetPinnedMessagesAsync();
            var LastWednesday = DateTime.Now.Date;
            while (LastWednesday.DayOfWeek != DayOfWeek.Wednesday)
            {
                LastWednesday = LastWednesday.Subtract(TimeSpan.FromDays(1));
            }
            if (pinned.Any(m => m.CreatedAt > LastWednesday && m.Author.Id == client.CurrentUser.Id))
                return true; // Created since last wednesday.
            if (pinned.Any(m => m.CreatedAt > DateTime.Now.Subtract(TimeSpan.FromDays(1)) && m.Author.Id == client.CurrentUser.Id))
                return true; // Fallback, created since yesterday.

            return false; // Green light, make that post.
        }

        public static Task SendLogToChannelAsync(ulong channel, int id)
        {
            return SendLogToChannelAsync(FindChannel(channel), id.ToString());
        }

        private static async Task SendLogToChannelAsync(ISocketMessageChannel channel, string id)
        {
            var file = Path.Combine("Logs", id + ".txt");
            if (!File.Exists(file))
                file = Path.Combine("Logs", "Archive", id + ".txt");
            if (!File.Exists(file))
                file = Path.Combine("Logs", "Failed", id + ".txt");

            if (File.Exists(file))
            {
                await channel.TriggerTypingAsync();
                try
                {
                    await DecksiteApi.UploadLogAsync(int.Parse(id));
                    await channel.SendMessageAsync($"https://logs.pennydreadfulmagic.com/match/{id}/");

                }
                catch (Exception e)
                {
                    var contents = File.ReadAllLines(file);
                    var caption = $"Format={contents[0]}, Comment=\"{contents[1]}\", Players=[{contents[3]}]";
                    await channel.SendFileAsync(file, caption);
                    Console.WriteLine($"Couldn't upload to logsite, {e}");

                }
            }
            else
            {
                await channel.TriggerTypingAsync();
                await channel.SendMessageAsync($"Could not find log for MatchID {id}");
            }
        }

        private static Task Client_LogAsync(LogMessage arg)
        {
            Console.WriteLine($"[Discord] {arg.ToString()}");
            return Task.FromResult(true);
        }

        private static Task Client_DisconnectedAsync(Exception arg)
        {
            return Task.FromResult(true);
        }

        private static async Task Client_ReadyAsync()
        {
            if (!string.IsNullOrEmpty(Playing))
                await SetGameAsync(Playing);
            Ready?.Invoke(client, new EventArgs());
        }

        public static async Task SendToGeneralAsync(string msg, bool pin = false)
        {
            var channel = FindChannel(207281932214599682);
            var res = await SendMessageAsync(msg, channel);
            if (pin)
            {
                try
                {
                    await res.PinAsync();
                }
                catch (Exception c)
                {
                    Console.WriteLine(c);
                }
            }
        }

        public static async Task SendToAllServersAsync(string msg)
        {
            string[] SpammyServers =  { "Penny Dreadful", "TestServer" };
            var guilds = client.Guilds.Where(g => SpammyServers.Contains(g.Name, StringComparer.CurrentCultureIgnoreCase));
            foreach (SocketGuild Server in guilds)
            {
                await SendMessageAsync(msg, Server.DefaultChannel);
            }
        }

        public static async Task<bool> SendToLFGAsync(string msg)
        {
            // Penny Dreadful server: #looking-for-games
            var channel = FindChannel(209488769567424512);
            return (await SendMessageAsync(msg, channel)) != null;
        }

        public static async Task<bool> SendToPDHAsync(string msg)
        {
            // Penny Dreadful server: #pdh
            var channel = FindChannel(234787370711515136);
            return (await SendMessageAsync(msg, channel)) != null;
        }

        public static async Task<bool> SendToLeagueAsync(string msg)
        {
            // Penny Dreadful server: #league
            var channel = FindChannel(220320082998460416);
            return (await SendMessageAsync(msg, channel)) != null;
        }

        public static async Task<bool> SendToTournamentRoomAsync(string msg)
        {
            // Penny Dreadful server: #tournament-room
            var channel = FindChannel(334220558159970304);
            return (await SendMessageAsync(msg, channel)) != null;
        }

        public static async Task<bool> SendToCommunityLegacyLeagueAsync(string msg)
        {
            var channel = FindChannel(341709019058143242);
            return (await SendMessageAsync(msg, channel)) != null;
        }

        public static async Task<bool> SendToHeirloomAsync(string msg)
        {
            var channel = FindChannel(246656730535034881);
            return (await SendMessageAsync(msg, channel)) != null;
        }

        public static async Task<bool> SendToTestAsync(string msg)
        {
            // This one goes to #botspam on Katelyn's test server.
            var channel = FindChannel(226920619302715392);
            return (await SendMessageAsync(msg, channel)) != null;
        }

        public static async Task<bool> SendToPMLogAsync(string msg)
        {
            var channel = FindChannel(331405678218313730);
            return (await SendMessageAsync(msg, channel)) != null;
        }

        public static async Task<bool> SendToArbiraryChannelAsync(string msg, ulong Channel)
        {
            var channel = FindChannel(Channel);
            return (await SendMessageAsync(msg, channel)) != null;
        }

        private static SocketTextChannel FindChannel(ulong chanId)
        {
            while (!client.Guilds.Any() || !client.Guilds.All(c => c.IsSynced))
                Thread.Sleep(100);
            try
            {
                return client.GetChannel(chanId) as SocketTextChannel;
            }
            catch (InvalidOperationException)
            {
                return client.GetChannel(chanId) as SocketTextChannel;
            }
        }

        private static async Task<RestUserMessage> SendMessageAsync(string msg, SocketTextChannel channel)
        {
            msg = SubstitueEmotes(msg, channel.Guild);
            if (channel == null)
            {
                return null;
            }
            try
            {
                return await channel.SendMessageAsync(msg);
            }
            catch (WebException c)
            {
                Console.WriteLine(c.Message);
            }
            catch (HttpException c)
            {
                Console.WriteLine(c.Message);
            }
            catch (HttpRequestException c)
            {
                Console.WriteLine(c);
            }
            catch (RateLimitedException)
            {
                // Thanks, Brainlesss's cat.
                Console.WriteLine("Hit Rate Limit.");
            }
            return null;
        }

        private static string SubstitueEmotes(string msg, SocketGuild guild)
        {
            var emote = new Regex(@"\[(\w+)\]", RegexOptions.Compiled);
            return emote.Replace(msg, (match) =>
                {
                    var symbol = match.Groups[1].Value;
                    if (Emotes.ContainsKey(symbol))
                    {
                        symbol = Emotes[symbol];
                    }

                    if (symbol.StartsWith(":"))
                    {
                        return symbol;
                    }

                    var found = FindEmote(symbol, guild);
                    if  (!string.IsNullOrEmpty(found))
                    {
                        return found;
                    }

                    return $"[{match.Value}]";
                }
            );
        }

        private static string FindEmote(string v, SocketGuild guild)
        {
            var emote = guild.Emotes.FirstOrDefault(e => e.Name.Trim(':') == v);
            if (emote != null)
                return emote.ToString();
            emote = client.Guilds.SelectMany(g => g.Emotes).FirstOrDefault(e => e.Name.Trim(':') == v);
            if (emote != null)
                return emote.ToString();
            return v;
        }

        public static async Task SetGameAsync(string game)
        {
            if (client.ConnectionState < ConnectionState.Connected)
            {
                Playing = game;
                return;
            }

            if (game == null && client.CurrentUser.Activity == null)
                return;
            if (client.CurrentUser.Activity != null && client.CurrentUser.Activity.Name == game)
                return;
            Console.WriteLine($"Setting Discord Game to {game}");
            await client.SetGameAsync(game);
            Playing = game;
        }

        public static async Task SetAvatarAsync(string image, string name)
        {
            Console.WriteLine($"Setting Avatar to {name} ({image})");
            try
            {
                await client.CurrentUser.ModifyAsync((props) =>
                {
                    props.Avatar = new Optional<Image?>(new Image(image));
                });
                CurrentAvatar = name;
                Console.WriteLine("Avatar Updated");
            }
            catch (HttpException)
            {
                Console.WriteLine("HTTP Exception");
            }
            catch (RateLimitedException)
            {
                await Task.Delay(TimeSpan.FromMinutes(10));
                await SetAvatarAsync(image, name);
            }
            catch (Exception c)
            {
                Console.WriteLine("Caught error while updating Avatar:");
                Console.WriteLine(c.ToString());
            }
        }

        public static bool Disconnect()
        {
            try
            {
                return client.LogoutAsync().Wait(TimeSpan.FromSeconds(5));
            }
            catch (Exception)
            {
                Console.WriteLine("Tried disconnecting from Discord, but already disconnected.");
                return false;
            }
        }

        public static async Task<bool> EchoChannelToDiscordAsync(string chan, string message, string author)
        {
            if (author.Equals(nameof(PDBot), StringComparison.InvariantCultureIgnoreCase))
            {
                // TODO: escape problematic player names?
            }
            var success = false;
            switch (chan.ToLowerInvariant())
            {
                case "cll":
                    success = await SendToCommunityLegacyLeagueAsync(message);
                    break;
                case "heirloom":
                    success = await SendToHeirloomAsync(message);
                    break;
                case "squire":
                    success = await SendToArbiraryChannelAsync(message, 377307172599496704);
                    break;
                case "pauperpower":
                case "pct":
                    success = await SendToArbiraryChannelAsync(message, 387127632266788870);
                    break;
                //case "modern":
                //    success = await SendToArbiraryChannelAsync(message, 294436932371611659);
                //    break;
                case "support":
                    success = await SendToArbiraryChannelAsync(message, 466099341359054860);
                    break;
                case "tribal":
                    success = await SendToArbiraryChannelAsync(message, 484265718771351554);
                    break;
                default:
                    break;
            }
            if (success)
                return success;

            if (chan.StartsWith("PD", StringComparison.CurrentCultureIgnoreCase))
                return await SendToTournamentRoomAsync(message);
            else
                return await SendToArbiraryChannelAsync(message, 352107915173167106);
        }

        private static readonly Dictionary<string, string> Emotes = new Dictionary<string, string>
        {
            // WUBRG
            { "sW", "WW" },
            { "sU", "UU" },
            { "sB", "BB" },
            { "sR", "RR" },
            { "sG", "GG" },
            // Tap
            { "sT", "TT" },
            { "sJ", "QQ" },
            // Hybrids
            { "s_", "BG"},
            { "s=", "BR"},
            { "s$", "UB"},
            { "s`", "UR"},
            { "s&amp,", "GU"}, // Yes, this is literally the name of the symbol
            { "s-", "GW"},
            { "s'", "RG"},
            { "s~", "RW" },
            { "s,", "WB" },
            { "s+", "WU" },
            // Snow
            { "so", "SS" },
            // Symbols
            { "sD", ":trophy:" },
            { "sV", ":small_red_triangle_down:" },
            { "sPig", ":pig:" },
            { "sLizard", ":lizard:" },
            { "sLifeHeart", ":heart:" },
            // Numbers
            { "s0", "00" },
            { "s1", "01" },
            { "s2", "02" },
            { "s3", "03" },
            { "s4", "04" },
            { "s5", "05" },
            { "s6", "06" },
            { "s7", "07" },
            { "s8", "08" },
            { "s9", "09" },
            { "sa", "10" },
            { "sb", "11" },
            { "sc", "12" },
            { "sd", "13" },
            { "se", "14" },
            { "sf", "15" },
            { "sg", "16" },
            { "sh", "17" },
            { "si", "18" },
            { "sj", "19" },
            { "sk", "20" },
            { "sX", "XX" },
            // twobrid
            { "s&gt,", "2G" },
            { "s&lt,", "2R" },
            {  "s%", "2B" },
            { "s@", "2U" },
            { "s!", "2W" },
            // Misc
            { "sS", ":slight_smile:" },
            { "sF", ":frowning:" },
            { "sY", ":nauseated_face:" },
            { "sMute", ":zipper_mouth:" },
            { "sZ", ":zzz:" },
            { "sAdept", ":eye:" },
        };
    }
}
