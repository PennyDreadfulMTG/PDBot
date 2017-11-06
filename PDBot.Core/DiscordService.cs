using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Discord.Net.Providers.WS4Net;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Threading;
using Discord.Rest;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Net.Http;
using PDBot.Core;

namespace PDBot.Discord
{
    public static class DiscordService
    {
        static DiscordSocketClient client = new DiscordSocketClient(new DiscordSocketConfig
        {
            WebSocketProvider = WS4NetProvider.Instance
        });

        public static event EventHandler Ready;

        private static bool Initialized = false;

        public static async Task Init(string token)
        {
            if (Initialized)
                return;
            Initialized = true;
            client.Log += Client_LogAsync;
            client.Ready += Client_ReadyAsync;
            client.Disconnected += Client_DisconnectedAsync;
            client.MessageReceived += Client_MessageReceived;
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
        }

        readonly static string[] modo_commands = new string[] { "!drop", "!retire"};

        public static string Playing { get; private set; }
        public static string CurrentAvatar { get; private set; } = "unknown";

        private static async Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg.Author.IsBot)
                return;

            if (modo_commands.Contains(arg.Content.ToLower()))
            {
                await arg.Channel.SendMessageAsync("I don't respond to messages over discord.  Please send that to me through Magic Online instead.");
                return;
            }

            var words = arg.Content.Split();

            if (arg.Content.ToLower() == "!avatar")
            {
                await arg.Channel.SendMessageAsync($"My current Avatar is {CurrentAvatar}.");
                return;
            }

            if (words.FirstOrDefault().ToLower() == "!log")
            {
                var file = Path.Combine("Logs", words.Skip(1).FirstOrDefault().ToString() + ".txt");
                if (File.Exists(file))
                {
                    var contents = File.ReadAllLines(file);
                    var caption = $"Format={contents[0]}, Comment=\"{contents[1]}\", Players=[{contents[3]}]";
                    await arg.Channel.TriggerTypingAsync();
                    await arg.Channel.SendFileAsync(file, caption);
                    return;
                }
            }

            if (arg.Content.StartsWith("#"))
            {
                var success = Resolver.Helpers.GetChatDispatcher().SendPM(words[0], $"【Discord】 {arg.Author.Username}: {arg.Content.Substring(arg.Content.IndexOf(' ')).Trim()}");
                if (success && arg.Content.Length > 200)
                {
                    try
                    {
                        await arg.DeleteAsync(new RequestOptions
                        {
                            AuditLogReason = "Cleaning up an overly long message before we echo it."
                        });
                    }
                    catch (Exception)
                    {
                        // Shrug.
                    }
                }
                return;
            }
        }

        private static async Task Client_LogAsync(LogMessage arg)
        {
            Console.WriteLine($"[Discord] {arg.Message}");
        }

        private static async Task Client_DisconnectedAsync(Exception arg)
        {
            //await client.StopAsync();
            //await client.StartAsync();
        }

        private static async Task Client_ReadyAsync()
        {
            if (!string.IsNullOrEmpty(Playing))
                await SetGame(Playing);
            Ready?.Invoke(client, new EventArgs());
        }

        public static async Task SendToGeneralAsync(string msg, bool pin = false)
        {
            var channel = FindChannel("Penny Dreadful", 207281932214599682);
            var res = await SendMessageAsync(msg, channel);
            if (pin)
            {
                try
                {
                    await res.PinAsync();
                }
                catch (Exception) { }
            }
        }

        public static async void SendToAllServersAsync(string msg)
        {
            string[] SpammyServers =  { "Penny Dreadful", "TestServer" };
            var guilds = client.Guilds.Where(g => SpammyServers.Contains(g.Name, StringComparer.CurrentCultureIgnoreCase));
            foreach (SocketGuild Server in guilds)
            {
                await SendMessageAsync(msg, Server.DefaultChannel);
            }
        }

        public static Task SendToLFGAsync(string msg)
        {
            var channel = FindChannel("Penny Dreadful", 209488769567424512);
            return SendMessageAsync(msg, channel);
        }

        public static Task SendToPDHAsync(string msg)
        {
            var channel = FindChannel("Penny Dreadful", 234787370711515136);
            return SendMessageAsync(msg, channel);
        }

        public static Task SendToLeagueAsync(string msg)
        {
            var channel = FindChannel("Penny Dreadful", 220320082998460416);
            return SendMessageAsync(msg, channel);
        }

        public static Task SendToChatRoomsAsync(string msg)
        {
            var channel = FindChannel("Penny Dreadful", 334220558159970304);
            return SendMessageAsync(msg, channel);
        }

        public static Task SendToCommunityLegacyLeague(string msg)
        {
            var channel = FindChannel("Community Legacy League", 341709019058143242);
            return SendMessageAsync(msg, channel);
        }

        public static Task SendToHeirloom(string msg)
        {
            var channel = FindChannel("torskafton", 246656730535034881);
            return SendMessageAsync(msg, channel);
        }

        public static Task SendToTestAsync(string msg)
        {
            // This one goes to #botspam on Katelyn's test server.
            var channel = FindChannel("TestServer", 226920619302715392);
            return SendMessageAsync(msg, channel);
        }

        public static Task SendToPMLog(string msg)
        {
            var channel = FindChannel("TestServer", 331405678218313730);
            return SendMessageAsync(msg, channel);
        }

        public static Task SendToArbiraryChannel(string msg, string ServerName, ulong Channel)
        {
            var channel = FindChannel(ServerName, Channel);
            return SendMessageAsync(msg, channel);
        }

        private static SocketTextChannel FindChannel(string GuildName, ulong chanId)
        {
            while (!client.Guilds.Any() || !client.Guilds.All(c => c.IsSynced))
                Thread.Sleep(100);
            try
            {
                return client.GetChannel(chanId) as SocketTextChannel;
                //SocketGuild Server = client.Guilds.Single(s => s.Name == GuildName);
                //var channel = Server.GetTextChannel(chanId);
                //return channel;
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
            catch (RateLimitedException c)
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

        public static async Task SetGame(string game)
        {
            if (client.ConnectionState < ConnectionState.Connected)
            {
                Playing = game;
                return;
            }
                
            if (game == null && !client.CurrentUser.Game.HasValue)
                return;
            if (client.CurrentUser.Game.HasValue && client.CurrentUser.Game.Value.Name == game)
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
            catch (RateLimitedException r)
            {
                await Task.Delay(TimeSpan.FromMinutes(10));
                await SetAvatarAsync(image, name);
            }
        }

        public static void Disconnect()
        {
            client.LogoutAsync().Wait();
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
            // TODO: twobrid
            // TODO: Implement the rest of these
            { "sS", ":slight_smile:" },
            { "sF", ":frowning:" },
            { "sY", ":nauseated_face:" },
        };
    }
}