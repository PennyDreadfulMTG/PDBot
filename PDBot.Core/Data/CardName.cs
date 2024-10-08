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

        static readonly Regex LimDul = new("Lim-D.{1,2}l", RegexOptions.Compiled);
        static readonly Regex Seance = new("S.{1,2}ance", RegexOptions.Compiled);
        static readonly Regex Jotun = new("J.{1,2}tun", RegexOptions.Compiled);
        static readonly Regex DanDan = new("Dand.{1,2}n", RegexOptions.Compiled);
        static readonly Regex Ghazban = new("Ghazb.{1,2}n Ogre", RegexOptions.Compiled);
        static readonly Regex Khabal = new("Khab.{1,2}l Ghoul", RegexOptions.Compiled);
        static readonly Regex Junun = new("Jun.{1,2}n Efreet", RegexOptions.Compiled);
        static readonly Regex Marton = new("M.{1,2}rton Stromgald", RegexOptions.Compiled);
        static readonly Regex IfhBiff = new("Ifh-B.{1,2}ff Efreet", RegexOptions.Compiled);
        static readonly Regex BaradDur = new("Barad-d.{1,4}r", RegexOptions.Compiled);
        static readonly Regex Theoden = new("Th.{1,2}den", RegexOptions.Compiled);
        static readonly Regex Andúril = new("And.{1,4}ril", RegexOptions.Compiled);
        static readonly Regex Dúnedain = new("D.{1,2}nedain", RegexOptions.Compiled);
        static readonly Regex Lothlórien = new("Lothl.{1,2}rien", RegexOptions.Compiled);
        static readonly Regex Glóin = new("Gl.{1,2}in", RegexOptions.Compiled);
        static readonly Regex Grishnákh = new("Grishn.{1,2}kh", RegexOptions.Compiled);
        static readonly Regex Gríma = new("Gr.{1,2}ma", RegexOptions.Compiled);
        static readonly Regex Marûf = new("Ma'r.{1,2}f", RegexOptions.Compiled);
        static readonly Regex Sméagol = new("Sm.{1,2}agol", RegexOptions.Compiled);
        static readonly Regex Tinúviel = new("Tin.{1,2}viel", RegexOptions.Compiled);
        static readonly Regex Théoden = new("Th.{1,2}oden", RegexOptions.Compiled);
        static readonly Regex Kennerüd = new("Kenner.{1,2}d", RegexOptions.Compiled);
        static readonly Regex ÉomerOfTheRiddermark = new(".{1,3}omer of the Riddermark", RegexOptions.Compiled);
        static readonly Regex ÉomerMarshalOfRohan = new(".{1,3}omer, Marshal of Rohan", RegexOptions.Compiled);
        static readonly Regex Éowyn = new(".{1,3}owyn", RegexOptions.Compiled);
        static readonly Regex Bartolomé = new("Bartolom.{1,3}", RegexOptions.Compiled);
        static readonly Regex Ratonhnhaketon = new ("Ratonhnhak.{1,5}ton");

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
            name = IfhBiff.Replace(name, "Ifh-Bíff Efreet");
            name = Kennerüd.Replace(name, "Kennerüd");
            name = BaradDur.Replace(name, "Barad-dûr");
            name = Theoden.Replace(name, "Théoden");
            name = Andúril.Replace(name, "Andúril");
            name = Dúnedain.Replace(name, "Dúnedain");
            name = Lothlórien.Replace(name, "Lothlórien");
            name = Glóin.Replace(name, "Glóin");
            name = Grishnákh.Replace(name, "Grishnákh");
            name = Gríma.Replace(name, "Gríma");
            name = Marûf.Replace(name, "Ma'rûf");
            name = Sméagol.Replace(name, "Sméagol");
            name = Tinúviel.Replace(name, "Tinúviel");
            name = Théoden.Replace(name, "Théoden");
            name = ÉomerOfTheRiddermark.Replace(name, "Éomer of the Riddermark");
            name = ÉomerMarshalOfRohan.Replace(name, "Éomer, Marshal of Rohan");
            name = Éowyn.Replace(name, "Éowyn");
            name = Bartolomé.Replace(name, "Bartolomé");
            name = Ratonhnhaketon.Replace(name, "Ratonhnhaké꞉ton");
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
