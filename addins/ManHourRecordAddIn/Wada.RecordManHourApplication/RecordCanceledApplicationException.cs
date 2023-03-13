using System.Runtime.Serialization;

namespace Wada.RecordManHourApplication
{
    [Serializable]
    public class RecordCanceledApplicationException : Exception
    {
        public RecordCanceledApplicationException()
        {
        }

        public RecordCanceledApplicationException(string message) : base(message)
        {
        }

        public RecordCanceledApplicationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RecordCanceledApplicationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}