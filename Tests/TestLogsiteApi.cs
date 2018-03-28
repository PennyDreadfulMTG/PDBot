using NUnit.Framework;
using PDBot.Core;
using PDBot.Core.API;
using PDBot.Discord;
using System;
using System.Threading.Tasks;

namespace Tests
{
    class TestLogsiteApi
    {
        [Test]
        public async Task TestStats()
        {
            var stats = await LogsiteApi.GetStatsAsync();
            Assert.That(stats, Is.Not.Null);
            foreach (var f in stats.Formats)
            {
                Assert.That(f.Value.Name, Is.Not.Null.Or.Empty);
            }

            Assert.That(stats.LastSwitcheroo < DateTime.Now);
        }

        [Test]
        public async Task TestSwitcheroo()
        {
            var stats = await LogsiteApi.GetStatsAsync();
            if (DateTimeOffset.UtcNow.Subtract(stats.LastSwitcheroo).TotalHours < 6)
            {
                Assert.That(!Features.PublishResults);
            }
            else
            {
                Assert.That(Features.PublishResults);
            }
        }
    }
}
