using Wada.Extensions;

namespace Wada.ManHourRecordService.ValueObjects;

public enum HolidayClassification
{
    [EnumDisplayName("営業日")] 
    None,

    /// <summary>
    /// 法定休日
    /// </summary>
    [EnumDisplayName("法定休日")]
    LegalHoliday,

    /// <summary>
    /// 法定外休日
    /// </summary>
    [EnumDisplayName("法定外休日")]
    RegularHoliday,
}
