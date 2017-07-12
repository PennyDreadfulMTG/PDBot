using NUnit.Framework;
using PDBot.Core.Data;
using PDBot.Core.GameObservers;
using PDBot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDBot.Core;

namespace Tests.Mocks
{
    class MockMatch : IMatch
    {
        public MockMatch(string comments= "Penny Dreadful", string[] players = null, MagicFormat format = MagicFormat.PennyDreadful)
        {
            Comments = comments;
            if (players == null)
                Players = new string[] { "silasary", "hexalite" };
            else
                Players = players;

            Format = format;

            Task<IGameObserver[]> task = Resolver.GetObservers(this);
            task.Wait();
            Observers = task.Result;

        }

        public string[] Players { get; set; }

        public string Comments { get; set; }

        public WinnerDictionary Winners { get; set; } = new WinnerDictionary()
        {
            // Game ID,  Winner name
            //{ 539763038, "silasary" },
            //{ 539763832,  "hexalite" }
        };

        public IGameObserver[] Observers { get; set; } = new IGameObserver[0];

        public Room GameRoom => Room.JustForFun;

        public MagicFormat Format { get; }

        public void SendChat(string message)
        {
            Assert.IsNotNull(message);
            Assert.IsNotEmpty(message);
        }
    }
}
