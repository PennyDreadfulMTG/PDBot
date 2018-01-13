using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gatherling.Models
{
    public class Deck
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("found")]
        public bool Found { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("archetype")]
        public string Archetype { get; set; }
    }

}
