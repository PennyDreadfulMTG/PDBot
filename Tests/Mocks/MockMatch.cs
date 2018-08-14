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
        public MockMatch(string comments= "Penny Dreadful", string[] players = null, MagicFormat format = MagicFormat.PennyDreadful, bool SkipObservers = false)
        {
            Comments = comments;
            if (players == null)
                Players = new string[] { "silasary", "kaet" };
            else
                Players = players;

            Format = format;

            if (SkipObservers)
            {
                Observers = new IGameObserver[0];
            }
            else
            {
                var task = Resolver.Helpers.GetObservers(this);
                task.Wait();
                Observers = task.Result;
            }
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

        public Room GameRoom { get; set; } = Room.JustForFun;

        public MagicFormat Format { get; }

        public List<string> NamedTokens { get; } = new List<string>();

        public int MatchID => 1;

        public bool Completed => false;

        public int MinutesPerPlayer => 25;

        public string Log(string message)
        {
            Console.WriteLine(message);
            return message;
        }

        public void SendChat(string message)
        {
            Assert.IsNotNull(message);
            Assert.IsNotEmpty(message);
        }
    }
}
