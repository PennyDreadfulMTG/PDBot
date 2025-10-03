using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace PDBot.Data
{
    public enum CardLayouts { Unknown, Normal, Split, Flip };

    public class Card : IEquatable<Card>
    {

        public Card(JObject blob)
        {
            FullName = blob.Value<string>("name");

            if (blob.TryGetValue("mana_cost", out var manaCost))
                ManaCost = (manaCost as JValue).Value as string;

            if (Enum.TryParse<CardLayouts>(blob.Value<string>("layout"), true, out var layout))
                Layout = layout;
            else
            {
                Layout = CardLayouts.Unknown;
                Console.WriteLine($"Unknown Layout type! `{blob.Value<string>("layout")}`");
            }

            if (blob.TryGetValue("card_faces", out var card_faces))
            {
                var faces = card_faces as JArray;
                Names = [FullName, .. (from f in faces select f.Value<string>("name"))];
            }

            if (blob.TryGetValue("mtgo_id", out var catId))
            {
                CatID = catId.Value<int>();
            }
            else
                CatID = -1;

            Names ??= [FullName];

            if (blob.TryGetValue("printed_name", out var printed_name))
            {
                Names = [.. Names, (string)printed_name];
            }

            if (blob.TryGetValue("flavor_name", out var flavor_name))
            {
                Names = [.. Names, (string)flavor_name];
            }

        }

        public string FullName { get; private set; }

        public string[] Names { get; private set; }
        public string ManaCost { get; private set; }
        public CardLayouts Layout { get; private set; }
        public int CatID { get; private set; }

        public bool Equals(Card other)
        {
            return false;
        }
    }
}
