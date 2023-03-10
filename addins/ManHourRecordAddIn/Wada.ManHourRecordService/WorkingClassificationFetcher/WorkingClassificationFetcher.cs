using Wada.AOP.Logging;

namespace Wada.ManHourRecordService.WorkingClassificationFetcher;

public interface IWorkingClassificationFetcher
{
    /// <summary>
    /// 項目分類を取得する
    /// </summary>
    /// <param name="cellValues"></param>
    /// <param name="departmentName"></param>
    /// <returns></returns>
    Task<WorkingClassificationRecord> FetchAsync(IEnumerable<IEnumerable<object>> cellValues, string departmentName);
}

public class WorkingClassificationFetcher : IWorkingClassificationFetcher
{
    [Logging]
    public async Task<WorkingClassificationRecord> FetchAsync(IEnumerable<IEnumerable<object>> cellValues, string departmentName)
    {
        // ジャグ配列をインデックス付きにする
        var cellValuesWithIndex = AttachIndexJaggedArray(cellValues);

        // 大分類のポジションを取得
        var majorRangeTask = FindMajorRangeAsync(cellValuesWithIndex, departmentName);

        // 中分類のポジションを取得
        var middleRangeTask = FindMiddleRangeAsync(cellValuesWithIndex, departmentName);

        // Task待ち
        var majorRange = await majorRangeTask;
        var middleRange = await middleRangeTask;

        return new(majorRange, middleRange);
    }

    [Logging]
    private static Task<Dictionary<string, ClassificationRangeRecord>> FindMiddleRangeAsync(IEnumerable<JaggedArrayWithIndex> cellValuesWithIndex, string departmentName)
        => Task.Run(async () =>
        {
            // 大分類の行Indexを取得
            var majorRow = cellValuesWithIndex.Where(x => x.ColumnIndex == 0)
                                              .Where(x => x.JaggedValue?.ToString() == departmentName)
                                              .First().RowIndex;

            // 見つけた所属の行(大分類の行)の次の所属の行を取得
            JaggedArrayWithIndex nextDepartmentStruct =
                cellValuesWithIndex.Where(x => x.ColumnIndex == 0)
                                   .Where(x => x.RowIndex > majorRow)
                                   .Where(x => cellValuesWithIndex.Where(y => y.RowIndex == x.RowIndex)
                                                                  .All(x => x.JaggedValue == null)
                                               || x.JaggedValue != null)
                                   .FirstOrDefault();

            var nextDepartmentRow = nextDepartmentStruct == default
            ? cellValuesWithIndex.Max(x => x.RowIndex) + 1
            : nextDepartmentStruct.RowIndex;

            // 大分類を取得
            var majorClasses = cellValuesWithIndex.Where(x => x.RowIndex == majorRow)
                                                  .Where(x => x.ColumnIndex > 1)
                                                  .Where(x => x.JaggedValue != null);

            var middleRangesTask = majorClasses.Select(x =>
            {
                return Task.Run(() =>
                {
                    var middleClasses = cellValuesWithIndex.Where(y => y.RowIndex > majorRow)
                                                           .Where(y => y.RowIndex < nextDepartmentRow)
                                                           .Where(y => y.ColumnIndex == x.ColumnIndex)
                                                           .Where(y => y.JaggedValue != null);

                    return (departmentName: x.JaggedValue?.ToString(), middleRanges: new ClassificationRangeRecord(
                        new(middleClasses.Min(y => y.RowIndex), middleClasses.Min(y => y.ColumnIndex)),
                        new(middleClasses.Max(y => y.RowIndex), middleClasses.Max(y => y.ColumnIndex))));
                });
            });
            var middleRanges = await Task.WhenAll(middleRangesTask);
            Dictionary<string, ClassificationRangeRecord> result = new();
            middleRanges.Where(x => x.departmentName != null)
                        .ToList()
                        .ForEach(x => result[x.departmentName!] = x.middleRanges);

            return result;
        });

    /// <summary>
    /// ジャグ配列にインデックスを付ける
    /// </summary>
    /// <param name="cellValues"></param>
    /// <param name="departmentName"></param>
    /// <returns></returns>
    [Logging]
    private static IEnumerable<JaggedArrayWithIndex> AttachIndexJaggedArray(IEnumerable<IEnumerable<object>> cellValues) =>
        cellValues.Select((value, row) => new { row, value })
        .Select(x => x.value.Select((cell, column) => new { cell, column })
                            .Select(y => new JaggedArrayWithIndex(x.row, y.column, y.cell)))
        .SelectMany(x => x);

    /// <summary>
    /// 大分類のレンジを返す
    /// </summary>
    /// <param name="cellValuesWithIndex"></param>
    /// <param name="departmentName"></param>
    /// <returns></returns>
    [Logging]
    private static Task<ClassificationRangeRecord> FindMajorRangeAsync(IEnumerable<JaggedArrayWithIndex> cellValuesWithIndex, string departmentName) =>
        Task.Run(() =>
        {
            var majorRow = cellValuesWithIndex.Where(x => x.JaggedValue?.ToString() == departmentName)
                                              .First().RowIndex;
            var result = cellValuesWithIndex.Where(x => x.RowIndex == majorRow)
                                            .Where(x => x.ColumnIndex > 1)
                                            .Where(x => x.JaggedValue != null);
            return new ClassificationRangeRecord(
                            new(result.Min(x => x.RowIndex), result.Min(x => x.ColumnIndex)),
                            new(result.Max(x => x.RowIndex), result.Max(x => x.ColumnIndex)));
        });
}

public record class WorkingClassificationRecord(
    ClassificationRangeRecord MajorRange,
    Dictionary<string, ClassificationRangeRecord> MiddleClassification)
{
    public ClassificationRangeRecord MajorRange { get; } = MajorRange;

    public Dictionary<string, ClassificationRangeRecord> MiddleClassification { get; } = MiddleClassification;
}

public class TestWorkingClassificationRecordFactory
{
    public static WorkingClassificationRecord Create(
        ClassificationRangeRecord? majorRange = default,
        Dictionary<string, ClassificationRangeRecord>? middleClassification = default)
    {
        majorRange ??= TestClassificationRangeRecordFactory.Create();
        if (middleClassification == null)
        {
            Dictionary<string, ClassificationRangeRecord> middle = new()
            {
                ["生産管理部"] = TestClassificationRangeRecordFactory.Create(),
                ["総務部"] = TestClassificationRangeRecordFactory.Create()
            };
            middleClassification = middle;
        }
        return new(majorRange, middleClassification);
    }
}

public record class ClassificationRangeRecord(ClassificationPositionRecord Bigen, ClassificationPositionRecord Finish)
{
    public ClassificationPositionRecord Bigen { get; } = Bigen;

    public ClassificationPositionRecord Finish { get; } = Finish;
}

public class TestClassificationRangeRecordFactory
{
    public static ClassificationRangeRecord Create(
        ClassificationPositionRecord? bigen = default,
        ClassificationPositionRecord? finish = default)
    {
        bigen ??= TestClassificationPositionRecordFactory.Create();
        finish ??= TestClassificationPositionRecordFactory.Create(column: 5);
        return new(bigen, finish);
    }
}

public record class ClassificationPositionRecord(int Row, int Column)
{
    public int Row { get; } = Row;

    public int Column { get; } = Column;
}

public class TestClassificationPositionRecordFactory
{
    public static ClassificationPositionRecord Create(int row = default, int column = default)
        => new(row, column);
}

/// <summary>
/// インデックス付きジャグ配列の構造体
/// </summary>
/// <param name="RowIndex"></param>
/// <param name="ColumnIndex"></param>
/// <param name="JaggedValue"></param>
internal record struct JaggedArrayWithIndex(int RowIndex, int ColumnIndex, object JaggedValue)
{
    public static implicit operator (int, int, object)(JaggedArrayWithIndex value)
    {
        return (value.RowIndex, value.ColumnIndex, value.JaggedValue);
    }

    public static implicit operator JaggedArrayWithIndex((int, int, object) value)
    {
        return new JaggedArrayWithIndex(value.Item1, value.Item2, value.Item3);
    }
}
