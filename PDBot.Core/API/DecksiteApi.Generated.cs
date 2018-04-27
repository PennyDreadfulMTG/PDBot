using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.API
{
    partial class DecksiteApi
    {
        public class Legalities
        {
            public string Commander { get; set; }
            public string Vintage { get; set; }
            public string Legacy { get; set; }
            public string Modern { get; set; }
        }

        public class SeasonStats
        {
            public int? count_decks { get; set; }
            public int? n_decks { get; set; }

            public int? count_maindecks { get; set; }
            public int? n_maindecks { get; set; }

            public int? n_sideboards { get; set; }
            public int? count_sideboards { get; set; }

            public int? wins { get; set; }
            public int? draws { get; set; }
            public int? losses { get; set; }
        }

        public class CardStat
        {
            public string name { get; set; }
            public object loyalty { get; set; }
            public int face_id { get; set; }
            public object hand { get; set; }
            public object bug_last_confirmed { get; set; }
            public string[] names { get; set; }
            public string card_id { get; set; }
            public object starter { get; set; }
            public string position { get; set; }
            public string draws_season { get; set; }
            public Legalities legalities { get; set; }
            public object toughness { get; set; }
            public string image_name { get; set; }
            public string text { get; set; }
            public string losses_season { get; set; }
            public string name_ascii { get; set; }
            public string type { get; set; }
            public int id { get; set; }
            public object life { get; set; }
            public double cmc { get; set; }
            public object power { get; set; }
            public object bug_class { get; set; }
            public string pd_legal { get; set; }
            public object bug_desc { get; set; }
            public string layout { get; set; }
            public List<string> mana_cost { get; set; }

            [JsonProperty("season")]
            public SeasonStats SeasonStats { get; set; }
            [JsonProperty("all")]
            public SeasonStats AllTimeStats { get; set; }
        }

        public struct Rotation
        {
            [JsonProperty("diff")]
            public double Diff { get; set; }

            [JsonProperty("friendly_diff")]
            public string FriendlyDiff { get; set; }

            public Set last { get; set; }

            public Set next { get; set; }

            public class Set
            {
                [JsonProperty("name")]
                public string Name { get; set; }
                public string rough_exit_date { get; set; }
                public string block { get; set; }
                public string enter_date { get; set; }
                public object exit_date { get; set; }
                public string code { get; set; }
            }
        }

        public struct Person
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            public long? discord_id { get; set; }
        }

    }
}
