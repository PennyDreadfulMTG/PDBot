using Discord;
using Discord.Commands;
using PDBot.Core.API;
using PDBot.Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.Discord
{
    class DiscordCommands : ModuleBase<SocketCommandContext>
    {
        [Command("avatar")]
        public Task AvatarAsync() => ReplyAsync($"My current Avatar is {DiscordService.CurrentAvatar}.");

        [Command("retire")]
        [Alias("drop")]
        public async Task RetireAsync()
        {
            var person = await API.DecksiteApi.GetPersonAsync(Context.User.Id.ToString());
            if (string.IsNullOrEmpty(person.Name))
            {
                await ReplyAsync("I don't know who you are.  Please message me on MTGO, or (link)[https://pennydreadfulmagic.com/link] your account first.");
                return;
            }
            var run = await DecksiteApi.GetRunAsync(person.Name);
            if (run == null)
            {
                await ReplyAsync($"You do not have an active deck in {DecksiteApi.CurrentLeagueName()}.");
                return;
            }

            var res = await run.Retire();
            if (res)
                await ReplyAsync($"Your deck {run.Name} has been retired from the {run.CompetitionName}");
            else
                await ReplyAsync($"Unable to retire your deck.  Please message Katelyn on discord.");
        }

        [Command("StillBugged")]
        public async Task StillBuggedAsync([Remainder] string CardName)
        {
            await Context.Channel.TriggerTypingAsync();
            var person = await API.DecksiteApi.GetPersonAsync(Context.User.Id.ToString());
            var name = person.Name ?? Context.User.Username;
            var (success, message) = await BuggedCards.UpdateBuggedAsync(CardName, name, -1, false);
            await ReplyAsync(message);
        }
    }
}
