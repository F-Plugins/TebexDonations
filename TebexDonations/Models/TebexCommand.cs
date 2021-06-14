namespace TebexDonations.Models
{
    public class TebexCommand
    {
        public int id { get; set; }
        public string command { get; set; }
        public int payment { get; set; }
        public int package { get; set; }
        public Conditions conditions { get; set; }
        public TebexPlayer player { get; set; }

    }
}
