using PDBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDBot.Core.GameObservers;
using PDBot.Interfaces;
using PDBot.Core.Interfaces;

namespace PDBot.Core.Commands
{
    class RecordCommand : ICommand
    {
        public string[] Handle { get; } = new string[] { "!record" };

        public bool AcceptsGameChat => true;

        public bool AcceptsPM => false;

        public async Task<string> RunAsync(string player, IMatch game, string[] args)
        {

            game.Winners.GetRecordData(out var first, out var record);
            var loser = game.Players.FirstOrDefault(d => d != first.Player);
            if (string.IsNullOrEmpty(first.Player))
                loser = "";
            return $"[sD]Record:  {first.Player ?? ""} {record} {loser}";
        }
    }
}
