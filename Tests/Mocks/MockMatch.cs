using NUnit.Framework;
using PDBot.Core.Data;
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
        public string[] Players { get; set; } = new string[] { "silasary", "hexalite" };

        public string Comments { get; set; } = "Penny Dreadful";

        public WinnerDictionary Winners { get; set; } = new WinnerDictionary()
        {
            // Game ID,  Winner name
            //{ 539763038, "silasary" },
            //{ 539763832,  "hexalite" }
        };

        public IGameObserver[] Observers { get; set; } = new IGameObserver[0];

        public Room GameRoom => Room.JustForFun;

        public void SendChat(string message)
        {
            Assert.IsNotNull(message);
            Assert.IsNotEmpty(message);
        }
    }
}
