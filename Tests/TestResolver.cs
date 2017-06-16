using NUnit.Framework;
using PDBot.Commands;
using PDBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    class TestResolver
    {
        [Test]
        public void TestCommands()
        {
            var commands = Resolver.GetInstances<ICommand>();
            Assert.IsNotEmpty(commands);

        }
    }
}
