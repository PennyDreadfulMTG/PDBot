using Newtonsoft.Json.Linq;
using System.Net;

namespace PDBot.API
{
    public class MtgoStatus
    {
        public static bool IsServerUp()
        {
            try
            {

                WebClient wc = new WebClient();
                var blob = wc.DownloadString("https://magic.wizards.com/sites/all/modules/custom/wiz_services/mtgo_status.php");
                var x = JObject.Parse(blob);
                return x.Value<string>("status") == "UP";
            }
            catch (WebException)
            {
                return false;
            }
        }
    }
}
