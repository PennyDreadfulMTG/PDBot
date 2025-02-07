using NUnit.Framework;
using NUnit.Framework.Legacy;
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
            ClassicAssert.IsNotEmpty(cards);
            var first = cards.First();
            ClassicAssert.IsNotEmpty(first.name);
            foreach (var item in cards)
            {
                ClassicAssert.IsNotNull(item.id);
            }
        }

        [TestCase("-diamonddust-")]
        public async Task TestPersonApi(string name)
        {
            var person = await DecksiteApi.GetPersonAsync(name);
            ClassicAssert.AreEqual(person.Name, name);
        }

        [Test]
        public async Task TestGetTournaments()
        {
            var info = await DecksiteApi.GetTournaments();
            ClassicAssert.NotNull(info);
        }

        //[Test]
        //public async Task DropSilasary()
        //{
        //    var me = await DecksiteApi.GetRunAsync("Silasary");
        //    await me.RetireAsync();
        //}
    }
}
