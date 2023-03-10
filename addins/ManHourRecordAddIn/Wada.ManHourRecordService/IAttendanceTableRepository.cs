using Wada.ManHourRecordService.AttendanceAggregation;
using Wada.ManHourRecordService.AttendanceTableCreator;

namespace Wada.ManHourRecordService;

/// <summary>
/// 勤務表
/// </summary>
public interface IAttendanceTableRepository
{
    /// <summary>
    /// 勤務表に追加する
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="workDay"></param>
    Task AddWokedDayAsync(Stream stream, Attendance workDay, Func<string, bool> canOverwriting);

    /// <summary>
    /// 勤務表を作成する
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="attendanceTableOwner"></param>
    Task CreateAsync(Stream stream, AttendanceTableOwner attendanceTableOwner);
}
