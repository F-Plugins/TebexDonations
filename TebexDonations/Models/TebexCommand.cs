using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TebexDonations.Models
{
    internal class TebexCommand
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("command")]
        public string Command { get; set; } = string.Empty;

        [JsonProperty("payment")]
        public int Payment { get; set; }

        [JsonProperty("package")]
        public int Package { get; set; }

        [JsonProperty("conditions")]
        public TebexCommandConditions Conditions { get; set; } = null!;

        [JsonProperty("player")]
        public TebexPlayer Player { get; set; } = null!;


        internal class TebexCommandConditions
        {
            [JsonProperty("delay")]
            public double Delay { get; set; } = 0;
            [JsonProperty("slots")]
            public int Slots { get; set; } = 0;
        }
    }
}
