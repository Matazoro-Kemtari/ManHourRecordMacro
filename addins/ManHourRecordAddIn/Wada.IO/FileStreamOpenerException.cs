using System.Runtime.Serialization;

namespace Wada.IO
{
    [Serializable]
    public class FileStreamOpenerException : Exception
    {
        public FileStreamOpenerException()
        {
        }

        public FileStreamOpenerException(string? message) : base(message)
        {
        }

        public FileStreamOpenerException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected FileStreamOpenerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}