namespace TebexDonations.Models
{
    public class Queue
    {
        public QueueMeta meta { get; set; }
        public TebexPlayer[] players { get; set; }
    }

    public class QueueMeta
    {
        public bool execute_offline { get; set; }
        public int next_check { get; set; }
        public bool more { get; set; }
    }
}
