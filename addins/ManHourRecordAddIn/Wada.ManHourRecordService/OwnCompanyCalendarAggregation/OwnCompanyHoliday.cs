using Wada.ManHourRecordService.ValueObjects;

namespace Wada.ManHourRecordService.OwnCompanyCalendarAggregation;

public record class OwnCompanyHoliday
{
    private OwnCompanyHoliday(DateTime holidayDate, HolidayClassification holidayClassification)
    {
        HolidayDate = holidayDate;
        HolidayClassification = holidayClassification;
    }

    public static OwnCompanyHoliday Reconstruct(DateTime holidayDate,
                                                HolidayClassification holidayClassification)
        => new(holidayDate, holidayClassification);

    /// <summary>
    /// 日付
    /// </summary>
    public DateTime HolidayDate { get; }

    /// <summary>
    /// 休日区分
    /// </summary>
    public HolidayClassification HolidayClassification { get; }
}
