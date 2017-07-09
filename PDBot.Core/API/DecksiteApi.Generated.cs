using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.API
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

        public class CardStat
        {
            public string name { get; set; }
            public object loyalty { get; set; }
            public int face_id { get; set; }
            public object hand { get; set; }
            public object bug_last_confirmed { get; set; }
            public string count_maindecks_season { get; set; }
            public List<string> names { get; set; }
            public string count_maindecks_all { get; set; }
            public string card_id { get; set; }
            public string draws_all { get; set; }
            public object starter { get; set; }
            public string position { get; set; }
            public string draws_season { get; set; }
            public Legalities legalities { get; set; }
            public string losses_all { get; set; }
            public string wins_season { get; set; }
            public string n_sideboards_all { get; set; }
            public object toughness { get; set; }
            public string image_name { get; set; }
            public int n_decks_all { get; set; }
            public string win_percent_all { get; set; }
            public string text { get; set; }
            public string losses_season { get; set; }
            public string name_ascii { get; set; }
            public string type { get; set; }
            public string count_sideboards_all { get; set; }
            public int id { get; set; }
            public object life { get; set; }
            public double cmc { get; set; }
            public string n_maindecks_season { get; set; }
            public object power { get; set; }
            public object bug_class { get; set; }
            public string pd_legal { get; set; }
            public string count_decks_all { get; set; }
            public object bug_desc { get; set; }
            public string layout { get; set; }
            public List<string> mana_cost { get; set; }
            public string n_sideboards_season { get; set; }
            public string win_percent_season { get; set; }
            public int count_decks_season { get; set; }
            public string n_decks_season { get; set; }
            public string count_sideboards_season { get; set; }
            public string wins_all { get; set; }
            public string n_maindecks_all { get; set; }
        }

        public struct Rotation
        {
            [JsonProperty("diff")]
            public int Diff { get; set; }

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

    }
}