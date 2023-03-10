using Wada.ManHourRecordService.EmployeeAggregation;

namespace Wada.ManHourRecordService;

public interface IEmployeeRepository
{
    /// <summary>
    /// 社員一覧を取得する
    /// </summary>
    /// <returns>社員一覧</returns>
    Task<IEnumerable<Employee>> FindAllAsync();

    /// <summary>
    /// 社員情報を取得する
    /// </summary>
    /// <param name="employeeNumber">社員番号</param>
    /// <returns>社員情報</returns>
    /// <exception cref="EmployeeAggregationException"/>
    Task<Employee> FindByEmployeeNumberAsync(uint employeeNumber);
}
