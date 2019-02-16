using System;
using System.Collections.Generic;
using System.Text;

namespace Gatherling.Models
{
    public class Pairing
    {
        public string A { get; internal set; }
        public int A_wins { get; internal set; }
        public string B { get; internal set; }
        public int B_wins { get; internal set; }
        public string Res { get; internal set; }
        public string Verification { get; internal set; }

        public string[] Players => A == B ? new string[] { A } : new string[] { A, B };

        public override string ToString()
        {
            CalculateRes();
            if (Res == "BYE")
                return $"{A} has the BYE!";
            return $"{A} {Res} {B}";
        }

        public void CalculateRes()
        {
            if (Res == null)
            {
                if (A == B)
                {
                    Res = "BYE";
                }
                else if (Verification == "verified")
                {
                    if (A_wins == 0 && B_wins == 0)
                    {
                        Res = $"?-?";
                    }
                    else
                    {
                        Res = $"{A_wins}-{B_wins}";
                    }
                }
                else
                {
                    Res = "vs.";
                }
            }
        }
    }

}
