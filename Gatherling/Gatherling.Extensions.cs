using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.API.GatherlingExtensions
{
    public static class Extensions
    {
        public static bool PageRequiresLogin(this HtmlDocument playerCP)
        {
            return playerCP.DocumentNode.Descendants("a")?.FirstOrDefault(a => a.Attributes["href"]?.Value == "login.php") != null;
        }
    }
}
