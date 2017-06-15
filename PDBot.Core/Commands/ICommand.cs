using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Commands
{
    public interface ICommand
    {
        string[] Handle { get; }

        bool AcceptsGameChat { get; }

        bool AcceptsPM { get; }

        Task<string> Run(string player, string[] args);
    }
}
