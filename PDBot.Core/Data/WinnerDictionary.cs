using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core.Data
{
    public class WinnerDictionary : Dictionary<int, string>
    {
        [Serializable]
        public struct Record
        {
            public Record(string player, int wins)
            {
                Player = player;
                Wins = wins;
            }

            public string Player
            {
                get;
                private set;
            }

            public int Wins
            {
                get;
                private set;
            }

            public override string ToString()
            {
                return $"[{Player}={Wins}]";
            }

            public static implicit operator Record(KeyValuePair<string, int> row)
            {
                return new Record(row.Key, row.Value);
            }
        }

        public void GetRecordData(out Record first, out string record)
        {
            if (!this.Any())
            {
                first = default(KeyValuePair<string, int>);
                record = "0–0";
                return;
            }
            var grouped = this.Values.GroupBy(u => u)
                                        .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                                        .OrderByDescending(kv => kv.Value);
            first = grouped.First();
            Record second = grouped.Skip(1).FirstOrDefault();

            record = $"{first.Wins}–{second.Wins}";
        }

        public Record GetWinnningPlayer()
        {
            var grouped = this.Values.GroupBy(u => u)
                                        .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                                        .OrderByDescending(kv => kv.Value);
            return grouped.First();
        }
    }
}
