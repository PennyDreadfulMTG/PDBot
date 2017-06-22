using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.GameObservers
{
    /// <summary>
    /// Non-definitive 
    /// </summary>
    public enum MagicFormat { Standard, Modern, Legacy, Vintage, Commander, Freeform, FreeformVanguard, Planechase, PennyDreadful, PennyDreadfulCommander, MomirBasic };

    public static class DetermineFormat
    {
        public static MagicFormat GuessFormat(string comment, string format)
        {
            comment = comment.ToLower();
            MagicFormat value;
            if (!Enum.TryParse<MagicFormat>(format, out value))
            {
                throw new ArgumentException($"{format} is not a valid format!");
            }
            if (value == MagicFormat.Freeform && IsPenny(comment))
                value = MagicFormat.PennyDreadful;
            if (value == MagicFormat.Commander && IsPenny(comment))
                value = MagicFormat.PennyDreadfulCommander;
            // If we want to someday support Frontier/Heirloom/other weird formats, add checks them here.
            return value;
        }

        public static bool IsPenny(string comment)
        {
            var words = comment.Split();
            if (comment.StartsWith("not "))
            {
                // Some people just want the algorithms to burn.
                return false;
            }
            else if (comment.Contains("penny") || comment == "pd4" || words.Contains("pd") || words.Contains("pdh"))
            {
                // Regular PD games
                return true;
            }
            else if (words.Contains("pdt") || words.Contains("pds") || words.Contains("pdm"))
            {
                // Thems tournament words.
                return true;
            }
            return false;
        }
    }
}
