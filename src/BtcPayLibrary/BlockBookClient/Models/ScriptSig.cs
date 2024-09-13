namespace BtcPayLibrary.BlockBookClient.Models
{
    public record ScriptSig
    {
        public string Asm { get; set; }
        public string Hex { get; set; }
    }
}
