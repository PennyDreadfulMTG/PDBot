using NUnit.Framework;
using PDBot.Core.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    class TestDecksiteApi
    {
        [Test]
        [Ignore("Broken?")]
        public async Task CheckPopularCards()
        {
            var cards = await DecksiteApi.PopularCardsAsync();
            Assert.IsNotEmpty(cards);
            var first = cards.First();
            Assert.IsNotEmpty(first.name);
            foreach (var item in cards)
            {
                Assert.IsNotNull(item.id);
            }
        }

        [TestCase("-diamonddust-")]
        public async Task TestPersonApi(string name)
        {
            var person = await DecksiteApi.GetPersonAsync(name);
            Assert.AreEqual(person.Name, name);
        }

        [Test]
        public async Task TestGetTournaments()
        {
            var info = await DecksiteApi.GetTournaments();
            Assert.NotNull(info);
        }
    }
}
