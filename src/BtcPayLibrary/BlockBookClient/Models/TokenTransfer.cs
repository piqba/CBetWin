namespace BtcPayLibrary.BlockBookClient.Models
{
    public record TokenTransfer
    {
        public string Type { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string Token { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public int Decimals { get; set; }

        public string Value { get; set; }
    }
}
