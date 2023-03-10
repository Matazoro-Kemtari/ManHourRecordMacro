using Wada.AOP.Logging;
using Wada.Extensions;

namespace Wada.ManHourRecordService.ValueObjects;

public enum DayOffClassification
{
    [EnumDisplayName("通常勤務")]
    [EnumDisplayShortName]
    None,

    /// <summary>
    /// 有給休暇
    /// </summary>
    [EnumDisplayName("有給休暇")]
    [EnumDisplayShortName("有休")]
    PaidLeave,

    /// <summary>
    /// 午前有給休暇
    /// </summary>
    [EnumDisplayName("午前有給休暇")]
    [EnumDisplayShortName("AM有")]
    AMPaidLeave,

    /// <summary>
    /// 午後有給休暇
    /// </summary>
    [EnumDisplayName("午後有給休暇")]
    [EnumDisplayShortName("PM有")]
    PMPaidLeave,

    /// <summary>
    /// 振替休日
    /// </summary>
    [EnumDisplayName("振替休日")]
    [EnumDisplayShortName("振休")]
    SubstitutedHoliday,

    /// <summary>
    /// 振替出勤
    /// </summary>
    [EnumDisplayName("振替出勤")]
    [EnumDisplayShortName("振出")]
    TransferedAttendance,

    /// <summary>
    /// 休日出勤
    /// </summary>
    [EnumDisplayName("休日出勤")]
    [EnumDisplayShortName("休出")]
    HolidayWorked,

    /// <summary>
    /// 特別休暇(有給)
    /// </summary>
    [EnumDisplayName("特別休暇(有給)")]
    [EnumDisplayShortName("特休有給")]
    PaidSpecialLeave,

    /// <summary>
    /// 特別休暇(無給)
    /// </summary>
    [EnumDisplayName("特別休暇(無給)")]
    [EnumDisplayShortName("特休無給")]
    UnpaidSpecialLeave,

    /// <summary>
    /// 欠勤
    /// </summary>
    [EnumDisplayName("欠勤")]
    [EnumDisplayShortName("欠勤")]
    Absence,

    /// <summary>
    /// 遅刻
    /// </summary>
    [EnumDisplayName("遅刻")]
    [EnumDisplayShortName("遅刻")]
    Lateness,

    /// <summary>
    /// 早退
    /// </summary>
    [EnumDisplayName("早退")]
    [EnumDisplayShortName("早退")]
    EarlyLeave,
}

public static class DayOffClassificationExtensions
{
    [Logging]
    public static DayOffClassification ToDayOffClassification(this string value)
    {
        var shortNames = new Dictionary<string, DayOffClassification>
        {
            { DayOffClassification.PaidLeave.GetEnumDisplayShortName()!, DayOffClassification.PaidLeave },
            { DayOffClassification.AMPaidLeave.GetEnumDisplayShortName()!, DayOffClassification.AMPaidLeave },
            { DayOffClassification.PMPaidLeave.GetEnumDisplayShortName()!, DayOffClassification.PMPaidLeave },
            { DayOffClassification.SubstitutedHoliday.GetEnumDisplayShortName()!, DayOffClassification.SubstitutedHoliday },
            { DayOffClassification.TransferedAttendance.GetEnumDisplayShortName()!, DayOffClassification.TransferedAttendance },
            { DayOffClassification.HolidayWorked.GetEnumDisplayShortName()!, DayOffClassification.HolidayWorked },
            { DayOffClassification.PaidSpecialLeave.GetEnumDisplayShortName()!, DayOffClassification.PaidSpecialLeave },
            { DayOffClassification.UnpaidSpecialLeave.GetEnumDisplayShortName()!, DayOffClassification.UnpaidSpecialLeave },
            { DayOffClassification.Absence.GetEnumDisplayShortName()!, DayOffClassification.Absence },
            { DayOffClassification.Lateness.GetEnumDisplayShortName()!, DayOffClassification.Lateness },
            { DayOffClassification.EarlyLeave.GetEnumDisplayShortName()!, DayOffClassification.EarlyLeave },
        };
        return shortNames.Where(x => x.Key == value).Select(x => x.Value).FirstOrDefault();
    }
}
