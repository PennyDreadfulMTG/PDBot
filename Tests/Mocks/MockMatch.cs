using NUnit.Framework;
using PDBot.Core.GameObservers;
using PDBot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Mocks
{
    class MockMatch : IMatch
    {
        public string[] Players { get; } = new string[] { "silasary", "hexalite" };

        public string Comments { get; } = "Penny Dreadful";

        public Dictionary<int, string> Winners => throw new NotImplementedException();

        public IGameObserver[] Observers => new IGameObserver[0];

        public Room GameRoom => Room.JustForFun;

        public void GetRecord(out KeyValuePair<string, int> first, out string record)
        {
            first = default(KeyValuePair<string, int>);
            record = "0-0";
        }

        public void SendChat(string message)
        {
            Assert.IsNotNull(message);
            Assert.IsNotEmpty(message);
        }
    }
}
