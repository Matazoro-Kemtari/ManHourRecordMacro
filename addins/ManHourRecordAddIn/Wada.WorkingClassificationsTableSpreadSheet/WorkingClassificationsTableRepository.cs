using ClosedXML.Excel;
using Wada.AOP.Logging;
using Wada.ManHourRecordService;

namespace Wada.WorkingClassificationsTableSpreadSheet;

public class WorkingClassificationsTableRepository : IWorkingClassificationsTableRepository
{
    [Logging]
    public IEnumerable<IEnumerable<object?>> FetchAll(Stream stream)
    {
        using var xlBook = new XLWorkbook(stream);

        var sheet = xlBook.Worksheet(1);

        var range = sheet.RangeUsed();

        var task = Task.WhenAll(
            range.Rows().Select(
                async row => await Task.WhenAll(row.Cells().Select(
                        async cell => await Task.Run(() => cell.IsEmpty() ? null : cell.Value.ToString())))));
        return task.Result;
    }
}
