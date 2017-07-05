using PDBot.Core.GameObservers;
using PDBot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Interfaces
{
    public interface ICommand
    {
        string[] Handle { get; }

        bool AcceptsGameChat { get; }

        bool AcceptsPM { get; }

        Task<string> RunAsync(string player, IMatch game, string[] args);
    }
}
