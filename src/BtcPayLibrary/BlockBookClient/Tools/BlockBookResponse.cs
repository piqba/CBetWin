namespace BtcPayLibrary.BlockBookClient.Tools
{
    public record BlockBookResponse<T>
    {
        public T? Response { get; set; }
        public bool Completed { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
