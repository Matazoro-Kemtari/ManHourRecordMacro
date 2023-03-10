using Wada.ManHourRecordService.AttendanceAggregation;
using Wada.ManHourRecordService.OvertimeWorkTableCreator;

namespace Wada.ManHourRecordService;

/// <summary>
/// 残業実績表
/// </summary>
public interface IOvertimeWorkTableRepository
{
    /// <summary>
    /// 残業実績表に追加する
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="workDay"></param>
    /// <returns></returns>
    Task AddAsync(Stream stream, Attendance workDay);

    /// <summary>
    /// 残業実績エクセルを作成する
    /// </summary>
    /// <param name="overtimeWorkTableOwner"></param>
    /// <returns></returns>
    Task CreateAsync(OvertimeWorkTableOwner overtimeWorkTableOwner);

    /// <summary>
    /// 残業実績エクセルの存在確認
    /// </summary>
    /// <param name="overtimeWorkTableOwner"></param>
    /// <returns></returns>
    bool OvertimeTableExists(OvertimeWorkTableOwner overtimeWorkTableOwner);
}
