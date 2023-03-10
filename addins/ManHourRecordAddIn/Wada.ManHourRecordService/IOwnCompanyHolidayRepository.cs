using Wada.ManHourRecordService.OwnCompanyCalendarAggregation;

namespace Wada.ManHourRecordService;

public interface IOwnCompanyHolidayRepository
{
    /// <summary>
    /// 指定した年月のカレンダーを取得する
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <returns></returns>
    Task<IEnumerable<OwnCompanyHoliday>> FindByYearMonthAsync(int year, int month);
}
