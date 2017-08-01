using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Data
{
    public class CardName : IEquatable<CardName>, IEquatable<string>
    {
        public static string NormalizeString(string name)
        {
            String normalizedString = name.Normalize(NormalizationForm.FormKD);
            StringBuilder stringBuilder = new StringBuilder();

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
            if (FullName.StartsWith("\""))
                FullName = FullName.Trim('\"');
            this.FullName = FullName;
            List<string> names = new List<string>();
            names.Add(FullName);

            var normalized = NormalizeString(FullName);
            if (normalized != FullName)
            {
                names.Add(normalized);
            }

            if (FullName.Contains(" // "))
            {

                string realname = FullName.Replace(" // ", "/");
                names.Add(realname);
                names.Add(realname.Replace("/", " & "));
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
