using PDBot.Core.API;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public static bool JoinGames { get; set; }
        public static bool ConnectToDiscord { get; set; }

        public static bool CreateVoiceChannels { get; set; }
        public static string PdmApiKey { get; set; }
        public static string GithubToken { get; set; }

        static Features()
        {
            PublishResults = true;
            AnnouncePairings = true;
            JoinGames = true;
            CreateVoiceChannels = false;
            ConnectToDiscord = true;
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
                if (c is WebException we)
                {
                    if (we.Status == WebExceptionStatus.ProtocolError)
                        return;
                }
                try
                {
                    SentrySdk.CaptureException(c);
                }
                catch (Exception)
                {

                }

            }
        }
    }
}
