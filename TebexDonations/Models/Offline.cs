namespace TebexDonations.Models
{
    public class Offline
    {
        public OfflineMeta meta { get; set; }
        public TebexCommand[] commands { get; set; }
    }

    public class OfflineMeta
    {
        public bool limited { get; set; }
    }

    public class Conditions
    {
        public int delay { get; set; }
    }
}
