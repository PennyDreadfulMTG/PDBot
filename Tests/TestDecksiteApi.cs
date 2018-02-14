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
        public void CheckPopularCards()
        {
            var cards = DecksiteApi.PopularCards();
            Assert.IsNotEmpty(cards);
            var first = cards.First();
            Assert.IsNotEmpty(first.name);
            foreach (var item in cards)
            {
                Assert.IsNotNull(item.id);
            }
        }
    }
}
