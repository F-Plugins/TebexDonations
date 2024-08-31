using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TebexDonations.Models
{
    internal class TebexCommands
    {
        [JsonProperty("commands")]
        public List<TebexCommand> Commands { get; set; } = null!;
    }
}
