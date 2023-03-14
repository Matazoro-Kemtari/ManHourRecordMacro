using System;
using System.Runtime.Serialization;

namespace Wada.ManHourRecordFunctions
{
    [Serializable]
    public class PresentationException : Exception
    {
        public PresentationException()
        {
        }

        public PresentationException(string message) : base(message)
        {
        }

        public PresentationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PresentationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}