namespace BtcPayLibrary.BlockBookClient.Models
{
    public record Vout
    {
        public string Value { get; set; }

        public int N { get; set; }

        public string Hex { get; set; }

        public string[] Addresses { get; set; }

        public bool IsAddress { get; set; }
        public bool IsOwn { get; set; }
    }
}
