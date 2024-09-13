namespace BtcPayLibrary.TransactionObserver.Models
{
    public record ObserveAddressRequest
    {
        public string Address { get; set; }

        public DateTime StarterTime { get; set; }

        public TimeSpan Timeout { get; set; }
    }
}
