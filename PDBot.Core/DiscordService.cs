using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Net.Providers.WS4Net;
using System.Diagnostics;

namespace PDBot.Discord
{
    public class DiscordService
    {
        static DiscordSocketClient client;

        public static async Task Init(string token)
        {
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
            try
            {
                //SocketGuild TestServer = client.Guilds.Single(s => s.Name == "TestServer");
                //var channel = TestServer.GetTextChannel(226920619302715392);
                //await channel.SendMessageAsync("Test?");
            }
            catch (Exception c)
            {
            }
        }

        public static async void SendToGeneralAsync(string msg)
        {
            SocketGuild Server = client.Guilds.Single(s => s.Name == "Penny Dreadful");
            var channel = Server.GetTextChannel(207281932214599682);
            await channel.SendMessageAsync(msg);
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
            if (game == null && !client.CurrentUser.Game.HasValue)
                return;
            if (client.CurrentUser.Game.HasValue && client.CurrentUser.Game.Value.Name == game)
                return;
            Console.WriteLine($"Setting Discord Game to {game}");
            await client.SetGameAsync(game);
            SocketGuild Server = client.Guilds.Single(s => s.Name == "Penny Dreadful");
        }

        public static void Disconnect()
        {
            client.LogoutAsync().Wait();
        }
    }
}