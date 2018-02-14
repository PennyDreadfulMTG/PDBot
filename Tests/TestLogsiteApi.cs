using NUnit.Framework;
using PDBot.Core;
using PDBot.Core.API;
using PDBot.Discord;
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
        }
    }
}
