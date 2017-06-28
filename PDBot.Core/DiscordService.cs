using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Net.Providers.WS4Net;
using System.Diagnostics;
using System.Threading;

namespace PDBot.Discord
{
    public class DiscordService
    {
        static DiscordSocketClient client;

        public static async Task Init(string token)
        {
            if (client != null)
                return;
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                WebSocketProvider = WS4NetProvider.Instance
            });
            client.Log += Client_Log;
            client.Ready += Client_ReadyAsync;
            client.Disconnected += Client_Disconnected;
            client.MessageReceived += Client_MessageReceived;
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
        }

        readonly static string[] modo_commands = new string[] { "!drop", "!retire"};

        public static string Playing { get; private set; }

        private static async Task Client_MessageReceived(SocketMessage arg)
        {
            if (arg.Author.IsBot)
                return;
            if (arg.Channel is SocketDMChannel)
            {
                await arg.Channel.SendMessageAsync("I don't respond to messages over discord.  Please send that to me through Magic Online instead.");
                return;
            }

            if (modo_commands.Contains(arg.Content.ToLower()))
            {
                await arg.Channel.SendMessageAsync("I don't respond to messages over discord.  Please send that to me through Magic Online instead.");
                return;
            }
        }

        private static async Task Client_Log(LogMessage arg)
        {
            //Console.WriteLine(arg);
        }

        private static async Task Client_Disconnected(Exception arg)
        {

        }

        private static async Task Client_ReadyAsync()
        {
            if (!string.IsNullOrEmpty(Playing))
                SetGame(Playing);
            //client.CurrentUser.ModifyAsync((e) => e.Avatar)
        }

        public static async void SendToGeneralAsync(string msg)
        {
            SocketGuild Server = client.Guilds.Single(s => s.Name == "Penny Dreadful");
            var channel = Server.GetTextChannel(207281932214599682);
            await channel.SendMessageAsync(msg);
        }

        public static async void SendToAllServersAsync(string msg)
        {
            foreach (SocketGuild Server in client.Guilds)
            {
                await Server.DefaultChannel.SendMessageAsync(msg);
            }
        }

        public static async void SendToLFGAsync(string msg)
        {
            SocketGuild Server = client.Guilds.Single(s => s.Name == "Penny Dreadful");
            var channel = Server.GetTextChannel(209488769567424512);
            await channel.SendMessageAsync(msg);
        }

        public static async void SendToPDHAsync(string msg)
        {
            SocketGuild Server = client.Guilds.Single(s => s.Name == "Penny Dreadful");
            var channel = Server.GetTextChannel(234787370711515136);
            await channel.SendMessageAsync(msg);
        }

        public static async void SendToLeagueAsync(string msg)
        {
            SocketGuild Server = client.Guilds.Single(s => s.Name == "Penny Dreadful");
            var channel = Server.GetTextChannel(220320082998460416);
            await channel.SendMessageAsync(msg);
        }

        [Conditional("DEBUG")]
        public static async void SendToTestAsync(string msg)
        {
            // This one goes to #botspam on Katelyn's test server. 
            SocketGuild Server = client.Guilds.Single(s => s.Name == "TestServer");
            var channel = Server.GetTextChannel(226920619302715392);
            await channel.SendMessageAsync(msg);
        }

        public static async void SetGame(string game)
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

        public static void SetAvatar(string image)
        {
            client.CurrentUser.ModifyAsync((props) =>
            {
                props.Avatar = new Optional<Image?>(new Image(image));
            });
        }

        public static void Disconnect()
        {
            client.LogoutAsync().Wait();
        }
    }
}