namespace BtcPayLibrary.TransactionObserver.Models
{
    public record TransactionObserved
    {
        public string Txid { get; set; }

        public double Fee { get; set; }

        public double PaidAmount { get; set; }
    }
}
