namespace BtcPayLibrary.BlockBookClient.Models
{
    public record Vin
    {
        public string Txid { get; set; }
        public int N { get; set; }
        public long Sequence { get; set; }
        public ScriptSig ScriptSig { get; set; }

        public string[] Addresses { get; set; }
        public bool IsAddress { get; set; }
        public string Value { get; set; }
        public string Hex { get; set; }
    }
}
