using System;
using System.Collections.Generic;
using System.Text;

namespace Gatherling.Models
{
    public class Pairing
    {
        public string A { get; internal set; }
        public string B { get; internal set; }
        public string Res { get; internal set; }
        public string Verification { get; internal set; }

        public override string ToString()
        {
            if (Res == null)
            {
                if (A == B)
                {
                    Res = "BYE";
                }
                else if (Verification == "verified")
                {
                    Res = $"?-?";
                }
                else
                {
                    Res = "vs.";
                }
            }
            if (Res == "BYE")
                return $"{A} has the BYE!";
            return $"{A} {Res} {B}";
        }
    }

}
