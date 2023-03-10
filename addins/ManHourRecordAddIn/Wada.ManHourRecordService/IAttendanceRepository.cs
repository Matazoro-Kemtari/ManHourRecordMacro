using System.Data;
using Wada.ManHourRecordService.AttendanceAggregation;

namespace Wada.ManHourRecordService;

/// <summary>
/// 勤務実績(日報)
/// </summary>
public interface IAttendanceRepository
{
    /// <summary>
    /// 勤務実績を追加する
    /// </summary>
    /// <param name="attendance"></param>
    Task AddAsync(Attendance attendance);

    /// <summary>
    /// 勤務実績を削除する
    /// </summary>
    /// <param name="attendance"></param>
    /// <returns></returns>
    Task RemoveByEmployeeNumberAndAchievementDateAsync(uint employeeNumber, DateTime achievementDate);

    /// <summary>
    /// 勤務実績を取得する
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="AttendanceAggregationException"/>
    Task<Attendance> FindByIdAsync(string id);

    /// <summary>
    /// 勤務実績を取得する
    /// </summary>
    /// <param name="employeeNumber"></param>
    /// <param name="achievementDate"></param>
    /// <returns></returns>
    Task<Attendance> FindByEmployeeNumberAndAchievementDateAsync(uint employeeNumber, DateTime achievementDate);
}
