using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Wada.AOP.Logging;
using Wada.Data.DesignDepartmentDataBse.Entities;
using Wada.ManHourRecordService;
using Wada.ManHourRecordService.OwnCompanyCalendarAggregation;
using Wada.ManHourRecordService.ValueObjects;

namespace Wada.Data.DesignDepartmentDataBse
{
    public class OwnCompanyHolidayRepository : IOwnCompanyHolidayRepository
    {
        [Logging]
        public async Task<IEnumerable<ManHourRecordService.OwnCompanyCalendarAggregation.OwnCompanyHoliday>> FindByYearMonthAsync(int year, int month)
        {
            using (var dbContext = new DesignDepartmentEntities())
            {
                var _beginDate = new DateTime(year, month, 1);
                var _finishDate = _beginDate.AddMonths(1);
                var ownHoliday = await dbContext.OwnCompanyHolidays
                    .Where(x => x.HolidayDate >= _beginDate)
                    .Where(x => x.HolidayDate < _finishDate)
                    .ToListAsync();

                if (!ownHoliday.Any())
                {
                    string msg = $"自社カレンダーに該当がありませんでした "
                                 + $"対象年月: {year}年{month}月";
                    throw new OwnCompanyCalendarAggregationException(msg);
                }

                return ownHoliday.Select(
                    x =>
                    ManHourRecordService.OwnCompanyCalendarAggregation.OwnCompanyHoliday.Reconstruct(
                        x.HolidayDate,
                        x.LegalHoliday ? HolidayClassification.LegalHoliday : HolidayClassification.RegularHoliday));

            }
        }
    }
}
