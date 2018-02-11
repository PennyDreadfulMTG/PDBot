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
                    round.RoundNum = int.Parse(m.Groups[nameof(round)].Value);
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

        internal static Round FromJson(JArray matches, Event tournament = null)
        {
            var round = new Round();
            foreach (var m in matches)
            {
                if (m.Value<int>(nameof(round)) != round.RoundNum)
                {
                    // TODO: Store old rounds on tournament object.
                    round = new Round
                    {
                        RoundNum = m.Value<int>(nameof(round)),
                        IsFinals = m.Value<int>("timing") > 1,
                    };
                }
                var p = new Pairing
                {
                    A = m.Value<string>("playera"),
                    B = m.Value<string>("playerb"),
                    Verification = m.Value<string>("verification"),
                };
                if (m["playera_wins"] != null)
                {
                    p.A_wins = m.Value<int>("playera_wins");
                    p.B_wins = m.Value<int>("playerb_wins");
                }
                try
                {

                if (m["res"] != null)
                    p.Res = m.Value<string>("res");
                }
                catch (NullReferenceException c)
                {
                    Console.WriteLine(c);
                }
                round.Matches.Add(p);
            }

            return round;
        }
    }
}
