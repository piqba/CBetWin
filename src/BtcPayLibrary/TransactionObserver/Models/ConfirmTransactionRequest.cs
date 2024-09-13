namespace BtcPayLibrary.TransactionObserver.Models
{
    public class ConfirmTransactionRequest
    {
        public string TxId { get; set; }
        public int Confirmations { get; set; }

        public TimeSpan Timeout { get; set; }
    }
}
