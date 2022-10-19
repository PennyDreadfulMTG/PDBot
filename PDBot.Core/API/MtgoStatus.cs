using Newtonsoft.Json.Linq;
using System.Net;

namespace PDBot.Core.API
{
    public class MtgoStatus
    {
        static readonly string[] urls = new string[] { "https://s3-us-west-2.amazonaws.com/s3-mtgo-greendot/status.json", "https://magic.wizards.com/sites/all/modules/custom/wiz_services/mtgo_status.php" };

        public static bool IsServerUp()
        {
            return true; // Temporary

            foreach (var url in urls)
            {
                try
                {
                    using (var wc = new WebClient())
                    {
                        var blob = wc.DownloadString(url);
                        var x = JObject.Parse(blob);
                        return x.Value<string>("status") == "UP";
                    }
                }
                catch (WebException)
                {
                    continue;
                }
            }
            return false;
        }
    }
}
