namespace BtcPayLibrary.BlockBookClient.Models
{
    public record SendTransactionResponse
    {
        public string Result { get; set; }

        public string Error { get; set; }
    }
}
