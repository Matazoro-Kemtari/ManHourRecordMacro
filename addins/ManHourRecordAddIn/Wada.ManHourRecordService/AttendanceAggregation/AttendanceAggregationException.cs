using System.Runtime.Serialization;

namespace Wada.ManHourRecordService.AttendanceAggregation;

[Serializable]
public class AttendanceAggregationException : Exception
{
    public AttendanceAggregationException()
    {
    }

    public AttendanceAggregationException(string? message) : base(message)
    {
    }

    public AttendanceAggregationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected AttendanceAggregationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}