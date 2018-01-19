using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Gatherling.Models
{
    public class Round
    {
        public int RoundNum { get; private set; }

        public bool IsFinals { get; private set; }

        public List<Pairing> Matches { get; } = new List<Pairing>();

        public static Round FromPaste(string[] lines)
        {
            var round = new Round();
            foreach (var line in lines)
            {
                var m = Regex.Match(line, @"(?:/me )?Pairings for Round (?<round>\d+)", RegexOptions.Compiled);
                if (m.Success)
                {
                    round.RoundNum = int.Parse(m.Groups["round"].Value);
                }
                else if ((m = Regex.Match(line, @"^(?:/me )?(?<a>\w+) (?<res>vs.|\d-\d) (?<b>\w+)$", RegexOptions.Compiled)).Success)
                {
                    var pairing = new Pairing
                    {
                        A = m.Groups["a"].Value,
                        B = m.Groups["b"].Value,
                        Res = m.Groups["res"].Value,
                    };
                    round.Matches.Add(pairing);
                }
                else if ((m = Regex.Match(line, @"^(?:/me )?(?<a>\w+) has the BYE$", RegexOptions.Compiled)).Success)
                {
                    round.Matches.Add(new Pairing
                    {
                        A = m.Groups["a"].Value,
                        B = m.Groups["a"].Value,
                        Res = "BYE",
                    });
                }
                else if ((m = Regex.Match(line, @"^(?:/me )?Good luck everyone", RegexOptions.Compiled)).Success)
                {
                    return round;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(line), line, "Unable to parse line.");
                }
            }
            return round;
        }

        internal static Round FromJson(JObject jObject)
        {
            return null;
        }
    }
}
