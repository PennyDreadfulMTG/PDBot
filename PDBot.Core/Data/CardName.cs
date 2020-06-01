using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PDBot.Data
{
    public class CardName : IEquatable<CardName>, IEquatable<string>
    {
        static readonly Regex LimDul = new Regex("Lim-D.{1,2}l", RegexOptions.Compiled);
        static readonly Regex Seance = new Regex("S.{1,2}ance", RegexOptions.Compiled);
        static readonly Regex Jotun = new Regex("J.{1,2}tun", RegexOptions.Compiled);
        static readonly Regex DanDan = new Regex("Dand.{1,2}n", RegexOptions.Compiled);
        static readonly Regex Ghazban = new Regex("Ghazb.{1,2}n Ogre", RegexOptions.Compiled);
        static readonly Regex Khabal = new Regex("Khab.{1,2}l Ghoul", RegexOptions.Compiled);
        static readonly Regex Junun = new Regex("Jun.{1,2}n Efreet", RegexOptions.Compiled);
        static readonly Regex Marton = new Regex("M.{1,2}rton Stromgald", RegexOptions.Compiled);
        /// <summary>
        /// Takes a name, and fixes up any messy encoding issues that might have occured.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string FixAccents(string name)
        {
            name = LimDul.Replace(name, "Lim-Dûl");
            name = Seance.Replace(name, "Séance");
            name = Jotun.Replace(name, "Jötun");
            name = DanDan.Replace(name, "Dandân");
            name = Ghazban.Replace(name, "Ghazbán Ogre");
            name = Khabal.Replace(name, "Khabál Ghoul");
            name = Junun.Replace(name, "Junún Efreet");
            name = Marton.Replace(name, "Márton Stromgald");
            return name;
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
