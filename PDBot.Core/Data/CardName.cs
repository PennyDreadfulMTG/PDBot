using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PDBot.Data
{
    public class CardName : IEquatable<CardName>, IEquatable<string>
    {
        /// <summary>
        /// A list of cards that meet a regex below, but are not the card in that regex.
        /// </summary>
        static string[] RealCards = ["Sunlance"];


        /// <summary>
        /// Takes a name, and fixes up any messy encoding issues that might have occured.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string FixAccents(string name)
        {
            return Encoding.UTF8.GetString(Encoding.GetEncoding("iso-8859-1").GetBytes(name));
        }

        public static string NormalizeString(string name)
        {
            var normalizedString = name.Normalize(NormalizationForm.FormKD);
            var stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                switch (CharUnicodeInfo.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                        stringBuilder.Append(c);
                        break;
                    case UnicodeCategory.SpaceSeparator:
                        stringBuilder.Append(' ');
                        break;
                    case UnicodeCategory.ConnectorPunctuation:
                    case UnicodeCategory.DashPunctuation:
                        stringBuilder.Append('-');
                        break;
                }
            }
            return stringBuilder.ToString();
        }

        public string FullName { get; private set; }
        public string[] Names { get; private set; }

        public CardName(string FullName)
        {

            FullName = FullName.Trim('\r');
            if (FullName.StartsWith("\""))
                FullName = FullName.Trim('\"');
            if (!RealCards.Contains(FullName))
                FullName = FixAccents(FullName);
            if (Regex.IsMatch(FullName, @"(\w+)/(\w+)"))
                FullName = FullName.Replace("/", " // ");
            this.FullName = FullName;
            var names = new List<string>
            {
                FullName
            };

            var normalized = NormalizeString(FullName);
            if (normalized != FullName)
            {
                names.Add(normalized);
            }

            if (FullName.Contains(" // "))
            {
                var realname = FullName.Replace(" // ", "/");
                names.Add(realname);
                names.Add(realname.Replace("/", " & "));
                names.Add(realname.Replace("/", " && "));
                names.AddRange(realname.Split('/'));
            }
            Names = names.ToArray();
        }

        public CardName(IEnumerable<string> names) : this(names.First())
        {
            if (names.Count() > 1)
            {
                // Flip/Transform cards.
                Names = Names.Union(names).ToArray();
            }
        }

        public bool Equals(CardName other)
        {
            return this.FullName == other.FullName;
        }

        public bool Equals(string other)
        {
            return Names.Contains(other);
        }
    }
}
