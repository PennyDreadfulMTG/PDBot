using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.GameObservers
{
    public interface IMatch
    {
        string[] Players { get; }
        string Comments { get; }
        Dictionary<int, string> Winners { get; }

        void GetRecord(out KeyValuePair<string, int> first, out string record);
        void SendChat(string message);
    }
}
