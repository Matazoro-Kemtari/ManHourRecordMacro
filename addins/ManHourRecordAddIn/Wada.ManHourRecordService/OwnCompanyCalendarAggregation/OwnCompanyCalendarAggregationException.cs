using System.Runtime.Serialization;

namespace Wada.ManHourRecordService.OwnCompanyCalendarAggregation;

[Serializable]
public class OwnCompanyCalendarAggregationException : Exception
{
    public OwnCompanyCalendarAggregationException()
    {
    }

    public OwnCompanyCalendarAggregationException(string? message) : base(message)
    {
    }

    public OwnCompanyCalendarAggregationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected OwnCompanyCalendarAggregationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}