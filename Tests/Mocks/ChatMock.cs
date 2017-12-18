using NUnit.Framework;
using PDBot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Mocks
{
    class ChatMock : IChatDispatcher
    {
        public void Join(string room)
        {
            Assert.That(room, Is.Not.Null.Or.Empty);

        }

        public bool SendPM(string Username, string message)
        {
            Assert.That(Username, Is.Not.Null.Or.Empty);
            Assert.That(message, Is.Not.Null.Or.Empty);
            return true;
        }
    }
}
