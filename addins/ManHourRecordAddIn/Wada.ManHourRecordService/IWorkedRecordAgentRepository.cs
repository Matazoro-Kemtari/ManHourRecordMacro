using Wada.ManHourRecordService.AttendanceAggregation;

namespace Wada.ManHourRecordService;

public interface IWorkedRecordAgentRepository
{
    /// <summary>
    /// 受注管理日報レポートに追加する
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="workDay"></param>
    Task AddAsync(Stream stream, Attendance workDay);

    /// <summary>
    /// 受注管理日報を作成する
    /// </summary>
    /// <param name="stream"></param>
    void Create(Stream stream);
}
