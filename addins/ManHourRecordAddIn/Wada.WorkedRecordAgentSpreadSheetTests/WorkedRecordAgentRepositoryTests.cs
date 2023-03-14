using ClosedXML.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.ManHourRecordService;
using Wada.ManHourRecordService.AttendanceAggregation;
using Wada.ManHourRecordService.EmployeeAggregation;
using Wada.ManHourRecordService.ValueObjects;
using Wada.ManHourRecordService.WorkingLedgerAggregation;

namespace Wada.WorkedRecordAgentSpreadSheet.Tests;

[TestClass()]
public class WorkedRecordAgentRepositoryTests
{
    [TestMethod()]
    public void 正常系_受注管理日報が作成できること()
    {
        // given
        // then
        Mock<IEmployeeRepository> mock_employee = new();
        Mock<IWorkingLedgerRepository> mock_ledger = new();
        IWorkedRecordAgentRepository repository =
            new WorkedRecordAgentRepository(mock_employee.Object, mock_ledger.Object);

        using Stream xlsStream = new MemoryStream();
        repository.Create(xlsStream);

        // when
        using var xlBook = new XLWorkbook(xlsStream);
        var sheet = xlBook.Worksheet(1);
        Assert.AreEqual("入力シート", sheet.Name);
        Assert.AreEqual(11, sheet.Style.Font.FontSize);
        Assert.AreEqual("游ゴシック", sheet.Style.Font.FontName);
        var headers = new string[]
        {
            "日付","社員番号","氏名","実働時間","作業番号", "コード",
            "作業名","特記事項","目標工数","工数","ヘッダ","番号",
        };
        var actual = sheet.Cells("A1:L1").Select(x => x.GetString()).ToArray();
        CollectionAssert.AreEqual(headers, actual);
    }

    [TestMethod()]
    public async Task 正常系_新規作成の受注管理日報に追加できること()
    {
        // given
        // when
        Mock<IEmployeeRepository> mock_employee = new();
        var employee = TestEmployeeFactory.Create();
        mock_employee.Setup(x => x.FindByEmployeeNumberAsync(It.IsAny<uint>()))
            .ReturnsAsync(employee);
        Mock<IWorkingLedgerRepository> mock_ledger = new();
        var workingLedger = TestWorkingLedgerFactory.Create();
        mock_ledger.Setup(x => x.FindByWorkingNumberAsync(It.IsAny<WorkingNumber>()))
            .ReturnsAsync(workingLedger);
        IWorkedRecordAgentRepository repository =
            new WorkedRecordAgentRepository(mock_employee.Object, mock_ledger.Object); ;

        using Stream xlsStream = new MemoryStream();
        repository.Create(xlsStream);

        var attendance = TestAttendanceFactory.Create();
        await repository.AddAsync(xlsStream, attendance);

        // then
        using var xlBook = new XLWorkbook(xlsStream);
        var sheet = xlBook.Worksheet(1);
        var table = sheet.Tables.Table(0);
        Assert.AreEqual(attendance.Achievements.Count(), table.DataRange.RowCount());
        var expected = attendance.Achievements.Select(x => new
        {
            attendance.AchievementDate,
            attendance.EmployeeNumber,
            EmployeeName = employee.Name,
            TotalManHour = attendance.Achievements.Sum(x => x.ManHour),
            WorkingNumber = x.WorkingNumber.ToString(),
            workingLedger.JigCode,
            WorkingName = x.AchievementProcess,
            x.Note,
            TargetManHour = x.ManHour,
            x.ManHour,
            HeaderOfWorkingNumber = x.WorkingNumber.Header + '-',
            NumberOfWorkingNumber = x.WorkingNumber.Number,
        }).ToList();
        var actual = table.DataRange.Rows().Select(x => new
        {
            AchievementDate = new AchievementDate(x.Cell(1).GetDateTime()),
            EmployeeNumber = x.Cell(2).GetValue<uint>(),
            EmployeeName = x.Cell(3).GetString(),
            TotalManHour = x.Cell(4).GetValue<decimal>(),
            WorkingNumber = x.Cell(5).GetString(),
            JigCode = x.Cell(6).GetString(),
            WorkingName = x.Cell(7).GetString(),
            Note = x.Cell(8).GetString(),
            TargetManHour = x.Cell(9).GetValue<decimal>(),
            ManHour = x.Cell(10).GetValue<decimal>(),
            HeaderOfWorkingNumber = x.Cell(11).GetString(),
            NumberOfWorkingNumber = x.Cell(12).GetValue<uint>(),
        }).ToList();
        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod()]
    public async Task 正常系_既存の受注管理日報に追加できること()
    {
        // given
        Mock<IEmployeeRepository> mock_employee = new();
        var employee = TestEmployeeFactory.Create();
        mock_employee.Setup(x => x.FindByEmployeeNumberAsync(It.IsAny<uint>()))
            .ReturnsAsync(employee);
        Mock<IWorkingLedgerRepository> mock_ledger = new();
        var workingLedger = TestWorkingLedgerFactory.Create();
        mock_ledger.Setup(x => x.FindByWorkingNumberAsync(It.IsAny<WorkingNumber>()))
            .ReturnsAsync(workingLedger);
        IWorkedRecordAgentRepository repository =
            new WorkedRecordAgentRepository(mock_employee.Object, mock_ledger.Object); ;

        using Stream xlsStream = new MemoryStream();
        repository.Create(xlsStream);

        var attendanceOther1 = TestAttendanceFactory.Create(employeeNumber: 1000u);
        await repository.AddAsync(xlsStream, attendanceOther1);
        var attendanceOther2 = TestAttendanceFactory.Create(achievements: new Achievement[]
        {
            TestAchievementFactory.Create(manHour: 4),
            TestAchievementFactory.Create(manHour: 4),
        });
        await repository.AddAsync(xlsStream, attendanceOther2);
        // when
        var attendance = TestAttendanceFactory.Create();
        await repository.AddAsync(xlsStream, attendance);

        // then
        using var xlBook = new XLWorkbook(xlsStream);
        var sheet = xlBook.Worksheet(1);
        Assert.AreEqual(1, sheet.Tables.Count());
        var table = sheet.Tables.Table(0);
        Assert.AreEqual(attendanceOther1.Achievements.Count() + attendance.Achievements.Count(),
                        table.DataRange.RowCount());
    }
}