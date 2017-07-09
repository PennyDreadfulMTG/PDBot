using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core
{
    /// <summary>
    /// Incomplete list of all Magic Formats.
    /// </summary>
    public enum MagicFormat
    {
        Standard = 0, Modern, Legacy, Vintage, Commander, Pauper, Commander1v1 = 18,
        Freeform = 6, FreeformVanguard, Test,
        PennyDreadful = 9, PennyDreadfulCommander,
        MomirBasic = 11, Planechase, Planeswalker,
        Hierloom = 14, Frontier,
        JhioraBasic = 16, MoStoJho,
        
    };

    public static class DetermineFormat
    {
        public static MagicFormat GuessFormat(string comment, string format)
        {
            format = format.Replace(" ", "").Replace("(", "").Replace(")", "");
            comment = comment.ToLower();
            if (!Enum.TryParse<MagicFormat>(format, out MagicFormat value))
            {
                throw new ArgumentException($"{format} is not a valid format!");
            }

            if (value == MagicFormat.Freeform && IsPenny(comment))
                value = MagicFormat.PennyDreadful;
            if (value == MagicFormat.Commander && IsPenny(comment))
                value = MagicFormat.PennyDreadfulCommander;
            if (value == MagicFormat.Commander1v1 && IsPenny(comment))
                value = MagicFormat.PennyDreadfulCommander;
            if (value == MagicFormat.Legacy && IsHeirloom(comment))
                value = MagicFormat.Hierloom;
            if (value == MagicFormat.Freeform && IsFrontier(comment))
                value = MagicFormat.Frontier;
            // If we want to someday support other weird formats, add checks them here.
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

        private static bool IsFrontier(string comment)
        {
            return comment.Contains("frontier");
        }

        private static bool IsHeirloom(string comment)
        {
            return comment.Contains("heirloom");
        }
    }
}
