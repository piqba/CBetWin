namespace BtcPayLibrary.BlockBookClient.Models
{
    public record Address
    {
        public int Page { get; set; }

        public int TotalPages { get; set; }

        public int ItemsOnPage { get; set; }

        public string address { get; set; }

        public string Balance { get; set; }

        public string TotalReceived { get; set; }

        public string TotalSent { get; set; }

        public string UnconfirmedBalance { get; set; }

        public int UnconfirmedTxs { get; set; }

        public int Txs { get; set; }

        public List<TransactionResponse> Transactions { get; set; } = [];


        public List<string> Txids { get; set; } = [];


        public string Nonce { get; set; }
        public List<Token> Tokens { get; set; } = [];

    }
}
