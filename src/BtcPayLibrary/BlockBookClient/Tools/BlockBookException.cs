using System.Runtime.Serialization;

namespace BtcPayLibrary.BlockBookClient.Tools
{
    public class BlockBookException : Exception
    {
        public BlockBookException()
        {
        }

        public BlockBookException(string message)
            : base(message)
        {
        }

        public BlockBookException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected BlockBookException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
