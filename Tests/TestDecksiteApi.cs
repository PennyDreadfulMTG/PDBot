using NUnit.Framework;
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
            var cards = PDBot.API.DecksiteApi.PopularCards();
            Assert.IsNotEmpty(cards);
            var first = cards.First();
            Assert.IsNotEmpty(first.name);
        }
    }
}
