using Wada.AOP.Logging;

namespace Wada.ManHourRecordService.WorkingClassificationFetcher;

public interface IDepartmentFetcher
{
    /// <summary>
    /// 所属一覧を取得する
    /// </summary>
    /// <param name="cellValues"></param>
    /// <returns></returns>
    IEnumerable<string> Fetch(IEnumerable<IEnumerable<object?>> cellValues);
}

public class DepartmentFetcher : IDepartmentFetcher
{
    [Logging]
    public IEnumerable<string> Fetch(IEnumerable<IEnumerable<object?>> cellValues)
        => cellValues.Skip(1)
                     .Select(rows => rows.ToArray()[0])
                     .Where(x => x != null)
                     .Cast<string>();
}
