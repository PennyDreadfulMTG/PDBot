using PDBot.Core.Interfaces;
using PDBot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PDBot.Core.Data
{
    public class GameLogLine
    {
        static Regex NewToken = new Regex(@"creates (a|two) (?<name>[\w\s]+).", RegexOptions.Compiled);
        static Regex Transreliquat = new Regex(@"targeting \[(?<name>[\w\s]+)\] token \(.* becomes a copy of target", RegexOptions.Compiled);
        /// <summary>
        /// A list of tokens that are too good for the word "token"
        /// </summary>
        public static string[] LegendaryTokens { get; } = new string[] {
            "Marit Lage", "Kaldra", "Ragavan", "Ashaya, the Awoken World", "Stangg Twin",
            "Voja", "Urami", "Tuktuk the Returned", "Servo", "Nightmare Horror",
            "Voja, Friend to Elves", "Vitu-Ghazi", "Etherium Cell", "Wizard",
        };

        public string Line { get; private set; }
        public List<string> Cards { get; } = new List<string>();
        public List<string> Tokens { get; } = new List<string>();

        public GameLogLine(string line, IMatch match)
        {
            var createsMatch = NewToken.Match(line);
            if (createsMatch.Success)
            {
                var name = createsMatch.Groups["name"].Value;
                if (!name.EndsWith("token") && !LegendaryTokens.Contains(name) && !match.NamedTokens.Contains(name))
                {
                    match.NamedTokens.Add(name);
                }
            }
            var copiesToken = Transreliquat.Match(line);
            if (copiesToken.Success)
            {
                var name = copiesToken.Groups["name"].Value;
                if (!LegendaryTokens.Contains(name) && !match.NamedTokens.Contains(name))
                {
                    match.NamedTokens.Add(name);
                }
            }

            this.Line = line;
            var i = -1;
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
                name = CardName.FixAccents(name);
                line = line.Substring(end + 1);
                var IsToken = line.TrimStart().StartsWith("token");
                if (LegendaryTokens.Contains(name) || match.NamedTokens.Contains(name))
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
