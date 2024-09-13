namespace BtcPayLibrary.BlockBookClient.Models
{
    public record ScriptPubKey
    {
        public string Asm { get; set; }

        public string Hex { get; set; }

        public string Type { get; set; }

        public string Address { get; set; }
    }
}
