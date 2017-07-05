using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.Data
{
    public class GameLogLine
    {
        /// <summary>
        /// A list of tokens that are too good for the word "token"
        /// </summary>
        public static string[] LegendaryTokens = new string[] { "Marit Lage", "Kaldra", "Ragavan", "Ashaya, the Awoken World", "Stangg Twin", "Voja", "Urami", "Tuktuk the Returned", "Servo" };

        public string Line { get; private set; }
        public List<string> Cards = new List<string>();
        public List<string> Tokens = new List<string>();

        public GameLogLine(string line)
        {
            this.Line = line;
            int i = -1;
            while ((i = line.IndexOf('[')) != -1)
            {
                line = line.Substring(i);
                var end = line.IndexOf("]");
                if (end == -1 && line.Contains("..."))
                {
                    // Ugh.
                    return;
                }
                var name = line.Substring(1, end - 1);
                line = line.Substring(end + 1);
                var IsToken = line.TrimStart().StartsWith("token");
                if (LegendaryTokens.Contains(name))
                    IsToken = true;

                if (IsToken)
                {
                    this.Tokens.Add(name);
                }
                else
                {
                    this.Cards.Add(name);
                }
            }
        }

        public override string ToString()
        {
            return Line;
        }
    }
}
