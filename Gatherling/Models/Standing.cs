using Newtonsoft.Json;

namespace Gatherling.Models
{
    public class Standing
    {
        [JsonProperty("player")]
        public string Player { get; set; }

        [JsonProperty("active")] // TODO: Use ItemConverterType to convert int to bool.
        public bool Active { get; set; } 
        [JsonProperty("score")]
        public int Score { get; set; }
        [JsonProperty("matches_played")]
        public int MatchesPlayed { get; set; }
        [JsonProperty("matches_won")]
        public int MatchesWon { get; set; }
        public int Draws { get; set; }
        public int GamesWon { get; set; }
        public int GamesPlayed { get; set; }
        public int Byes { get; set; }
        public double OpMatch { get; set; }
        public double PlGame { get; set; }
        public double OpGame { get; set; }
        [JsonProperty("seed")]
        public int Seed { get; set; }
    }
}
