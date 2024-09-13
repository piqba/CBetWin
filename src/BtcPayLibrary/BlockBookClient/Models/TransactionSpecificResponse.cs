namespace BtcPayLibrary.BlockBookClient.Models
{
    public record TransactionSpecificResponse
    {
        public string Txid { get; set; }

        public string Hash { get; set; }

        public int Version { get; set; }

        public int Size { get; set; }

        public int Vsize { get; set; }

        public int Weight { get; set; }

        public int Locktime { get; set; }

        public List<VinTransaction> Vin { get; set; }

        public List<VoutTransaction> Vout { get; set; }

        public string Hex { get; set; }

        public string Blockhash { get; set; }

        public int Confirmations { get; set; }

        public int Time { get; set; }

        public int Blocktime { get; set; }
    }
}
