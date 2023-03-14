using System;
using System.Runtime.Serialization;

namespace Wada.ManHourRecordFunctions
{
    [Serializable]
    public class ValidateAbortException : PresentationException
    {
        public ValidateAbortException()
        {
        }

        public ValidateAbortException(string message) : base(message)
        {
        }

        public ValidateAbortException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ValidateAbortException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}