using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TebexDonations.Models
{
    internal class TebexQueue
    {
        internal class TebexMeta
        {
            [JsonProperty("execute_offline")]
            public bool ExecuteOffline { get; set; }

            [JsonProperty("next_check")]
            public double NextCheck { get; set; }

            [JsonProperty("more")]
            public bool More { get; set; }
        }

        [JsonProperty("meta")]
        public TebexMeta Meta { get; set; } = null!;

        [JsonProperty("players")]
        public List<TebexPlayer> Players { get; set; } = new();
    }
}
