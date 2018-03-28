using PDBot.Core.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core
{
    public class Features
    {
        /// <summary>
        /// There is a bug with MTGO that gets the names wrong.
        /// We don't want to tell people incorrect results when this happens.
        /// </summary>
        public static bool PublishResults { get; set; }

        /// <summary>
        /// Do we want to announce Gatherling pairings?
        /// </summary>
        public static bool AnnouncePairings { get; set; }

        static Features()
        {
            PublishResults = true;
            AnnouncePairings = true;
            try
            {
                var stats = LogsiteApi.GetStatsAsync().GetAwaiter().GetResult();
                if (DateTimeOffset.UtcNow.Subtract(stats.LastSwitcheroo).TotalHours < 6)
                {
                    PublishResults = false;
                }
            }
            catch (Exception c)
            {
                Console.WriteLine(c);
            }

        }
    }
}
