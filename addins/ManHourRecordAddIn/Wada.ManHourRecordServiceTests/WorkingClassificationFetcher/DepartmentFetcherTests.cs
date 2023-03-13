using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wada.ManHourRecordService.WorkingClassificationFetcher.Tests;

[TestClass()]
public class DepartmentFetcherTests
{
    [TestMethod()]
    public void 正常系_部署リストが取得できること()
    {
        // given
        // when
        object?[][] cellValues = new object?[][]
        {
            new object?[] { "ここはスキップ", "ここはスキップ", "ここはスキップ" },
            new object?[] { "A部署", "ここはスキップ", "ここはスキップ" },
            new object?[] { null, "ここはスキップ", "ここはスキップ" },
            new object?[] { "B部署", "ここはスキップ", "ここはスキップ" },
            new object?[] { null, "ここはスキップ", "ここはスキップ" },
            new object?[] { null, "ここはスキップ", "ここはスキップ" },
            new object?[] { "C部署", "ここはスキップ", "ここはスキップ" },
            new object?[] { null, "ここはスキップ", "ここはスキップ" },
            new object?[] { null, "ここはスキップ", "ここはスキップ" },
            new object?[] { null, "ここはスキップ", "ここはスキップ" },
            new object?[] { null, "ここはスキップ", "ここはスキップ" },
            new object?[] { "D部署", "ここはスキップ", "ここはスキップ" },
            new object?[] { null, "ここはスキップ", "ここはスキップ" },
            new object?[] { "E部署", "ここはスキップ", "ここはスキップ" },
            new object?[] { null, "ここはスキップ", "ここはスキップ" },
            new object?[] { null, "ここはスキップ", "ここはスキップ" },
        };

        IDepartmentFetcher useCase = new DepartmentFetcher();
        var actual = useCase.Fetch(cellValues);

        // then
        var expected = new string[]
        {
            "A部署","B部署","C部署","D部署","E部署",
        };
        CollectionAssert.AreEqual(expected, actual.ToArray());
    }
}