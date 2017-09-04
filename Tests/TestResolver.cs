using NUnit.Framework;
using PDBot.Commands;
using PDBot.Core;
using PDBot.Core.Interfaces;
using PDBot.Interfaces;
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

        [Test]
        public void TestObservers()
        {
            var match = new Mocks.MockMatch(SkipObservers: true);
            foreach (var observer in Resolver.GetInstances<IGameObserver>())
            {
                var actual = observer.GetInstanceForMatchAsync(match);
                Assert.IsNotNull(actual, $"{observer.GetType().Name} returned null.  Should have returned a task.");
            }
        }
    }
}
