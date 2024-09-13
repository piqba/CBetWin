namespace BtcPayLibrary.BlockBookClient.Models
{
    public record VoutTransaction
    {
        public decimal Value { get; set; }
        public int N { get; set; }
        public ScriptPubKey ScriptPubKey { get; set; }
    }
}
