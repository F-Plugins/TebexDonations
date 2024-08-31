using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TebexDonations.Models
{
    internal class TebexPlayer
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        [JsonProperty("uuid")]
        public string UId { get; set; } = string.Empty;
    }
}
