using ClosedXML.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.Extensions;
using Wada.ManHourRecordService;
using Wada.ManHourRecordService.AttendanceAggregation;
using Wada.ManHourRecordService.AttendanceTableCreator;
using Wada.ManHourRecordService.EmployeeAggregation;
using Wada.ManHourRecordService.OwnCompanyCalendarAggregation;
using Wada.ManHourRecordService.ValueObjects;

namespace Wada.AttendanceSpreadSheet.Tests;

[TestClass()]
public class AttendanceTableRepositoryTests
{
    [TestMethod()]
    public async Task 正常系_勤務表に実績が追加されること()
    {
        // given
        using Stream xlsStream = new MemoryStream();
        var workDay = TestAttendanceFactory.Create();
        using (var workbook = MakeTestBook(workDay.EmployeeNumber))
            workbook.SaveAs(xlsStream);

        // when
        Mock<IOwnCompanyHolidayRepository> mock_holiday = new();
        Mock<IEmployeeRepository> mock_employee = new();
        IAttendanceTableRepository repository = new AttendanceTableRepository(mock_holiday.Object, mock_employee.Object);
        await repository.AddWokedDayAsync(xlsStream, workDay, (message) => true);

        // then
        using (var workbook = new XLWorkbook(xlsStream))
        {
            /*
             * NOTE: Excelから時間を取ると循環小数の切り捨ての関係で誤差が生じる
             * セルがTimeSpanであってもDateTime型で取得して時間だけ抜き取ること
             * https://deskworkkaizen.com/jikan-gosa/
            */
            var sheet = workbook.Worksheet($"{workDay.AchievementDate.Value.Month}月");
            Assert.AreEqual(workDay.DayOffClassification.GetEnumDisplayShortName(), sheet.Cell($"D{workDay.AchievementDate.Value.Day + 4}").Value);
            Assert.IsTrue(sheet.Cell($"E{workDay.AchievementDate.Value.Day + 4}").TryGetValue(out DateTime actualStart));
            Assert.AreEqual(workDay.StartTime, actualStart.TimeOfDay);
            Assert.IsTrue(sheet.Cell($"F{workDay.AchievementDate.Value.Day + 4}").TryGetValue(out DateTime actualEnd));
            Assert.AreEqual(workDay.EndTime, actualEnd.TimeOfDay);
        }
    }

    private static IXLWorkbook MakeTestBook(uint employeeNumber, int[]? months = null)
    {
        months ??= new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var workbook = new XLWorkbook();
        months.ToList().ForEach(month =>
        {
            var sht = workbook.AddWorksheet($"{month}月");
            sht.Cell("A1").Value = new DateTime(DateTime.Now.Year, month, 1);
            sht.Cell("G2").Value = employeeNumber;
        });
        return workbook;
    }

    [TestMethod]
    public async Task 異常系_実績月のシートがない場合例外になること()
    {
        // given
        using Stream xlsStream = new MemoryStream();
        var workDay = TestAttendanceFactory.Create();
        using var workbook = MakeTestBook(
            workDay.EmployeeNumber, new int[] { DateTime.Now.Month == 1 ? 12 : 1 });
        workbook.SaveAs(xlsStream);

        // when
        Mock<IOwnCompanyHolidayRepository> mock_holiday = new();
        Mock<IEmployeeRepository> mock_employee = new();
        IAttendanceTableRepository repository = new AttendanceTableRepository(mock_holiday.Object, mock_employee.Object);
        Task target()
            => repository.AddWokedDayAsync(xlsStream, workDay, (message) => true);

        // then
        var ex = await Assert.ThrowsExceptionAsync<DomainException>(target);
        var expected = $"{DateTime.Now.Month}月のシートが見つかりません";
        Assert.AreEqual(expected, ex.Message);
    }

    [TestMethod]
    public async Task 異常系_実績月のA1セルの日付が取得できない場合例外になること()
    {
        // given
        using Stream xlsStream = new MemoryStream();
        var workDay = TestAttendanceFactory.Create();
        using var workbook = MakeTestBook(workDay.EmployeeNumber);
        var sh = workbook.Worksheet($"{DateTime.Now.Month}月");
        sh.Range("A1").Clear();
        workbook.SaveAs(xlsStream);

        // when
        Mock<IOwnCompanyHolidayRepository> mock_holiday = new();
        Mock<IEmployeeRepository> mock_employee = new();
        IAttendanceTableRepository repository = new AttendanceTableRepository(mock_holiday.Object, mock_employee.Object);
        Task target()
            => repository.AddWokedDayAsync(xlsStream, workDay, (message) => true);

        // then
        var ex = await Assert.ThrowsExceptionAsync<DomainException>(target);
        var expected = $"年月が取得できません シート:{DateTime.Now.Month}月, セル:A1";
        Assert.AreEqual(expected, ex.Message);
    }

    [TestMethod]
    public async Task 異常系_実績月の社員番号が取得できない場合例外になること()
    {
        // given
        using Stream xlsStream = new MemoryStream();
        var workDay = TestAttendanceFactory.Create();
        using var workbook = MakeTestBook(workDay.EmployeeNumber);
        var sh = workbook.Worksheet($"{DateTime.Now.Month}月");
        sh.Range("G2").Clear();
        workbook.SaveAs(xlsStream);

        // when
        Mock<IOwnCompanyHolidayRepository> mock_holiday = new();
        Mock<IEmployeeRepository> mock_employee = new();
        IAttendanceTableRepository repository = new AttendanceTableRepository(mock_holiday.Object, mock_employee.Object);
        Task target()
        => repository.AddWokedDayAsync(xlsStream, workDay, (message) => true);

        // then
        var ex = await Assert.ThrowsExceptionAsync<DomainException>(target);
        var expected = $"社員番号が取得できません シート:{DateTime.Now.Month}月, セル:G2";
        Assert.AreEqual(expected, ex.Message);
    }

    [TestMethod]
    public async Task 異常系_実績月の社員番号が違う場合例外になること()
    {
        // given
        using Stream xlsStream = new MemoryStream();
        var workDay = TestAttendanceFactory.Create();
        using var workbook = MakeTestBook(workDay.EmployeeNumber + 1);
        var sh = workbook.Worksheet($"{DateTime.Now.Month}月");
        workbook.SaveAs(xlsStream);

        // when
        Mock<IOwnCompanyHolidayRepository> mock_holiday = new();
        Mock<IEmployeeRepository> mock_employee = new();
        IAttendanceTableRepository repository = new AttendanceTableRepository(mock_holiday.Object, mock_employee.Object);
        Task target()
            => repository.AddWokedDayAsync(xlsStream, workDay, (message) => true);

        // then
        var ex = await Assert.ThrowsExceptionAsync<DomainException>(target);
        var expected = $"勤務表の社員番号が異なります シート:{DateTime.Now.Month}月, 社員番号:{workDay.EmployeeNumber + 1}";
        Assert.AreEqual(expected, ex.Message);
    }

    [TestMethod()]
    public async Task 正常系_勤務表テンプレートが作成されること()
    {
        // given
        // when
        Mock<IOwnCompanyHolidayRepository> mock_holiday = new();
        var ownCalender = OwnCalender202301();
        mock_holiday.Setup(x => x.FindByYearMonthAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(ownCalender);

        Mock<IEmployeeRepository> mock_employee = new();
        uint employeeNumber = 4001u;
        string employeeName = "本社　無人";
        mock_employee.Setup(x => x.FindByEmployeeNumberAsync(It.IsAny<uint>()))
            .ReturnsAsync(new Employee(employeeNumber, employeeName, null));

        IAttendanceTableRepository repository = new AttendanceTableRepository(mock_holiday.Object, mock_employee.Object);
        using MemoryStream xlsStream = new();

        DateTime attendanceDate = new(2023, 1, 1);
        AttendanceTableOwner attendanceTableOwner = new(attendanceDate, employeeNumber);
        await repository.CreateAsync(xlsStream, attendanceTableOwner);

        // then
        using var workbook = new XLWorkbook(xlsStream);
        Assert.AreEqual(2, workbook.Worksheets.Count);
        Assert.AreEqual($"{attendanceDate.Month}月", workbook.Worksheet(1).Name);
        Assert.AreEqual(attendanceDate, workbook.Worksheet(1).Cell("A1").GetDateTime());

        var worksheet = workbook.Worksheet(1);
        DateTime startDate = new(attendanceTableOwner.AttendanceYearMonth.Year, attendanceTableOwner.AttendanceYearMonth.Month, 1);
        workbook.Worksheet(1).Rows("5:35").Select((row, i) => (row, i)).ToList().ForEach(x =>
        {
            Assert.AreEqual($"{attendanceDate.AddDays(x.i):ddd}", x.row.Cell("C").GetString(), $"{attendanceDate}:曜日の検証");

            if (ownCalender.Where(y => y.HolidayDate == attendanceDate.AddDays(x.i)).Any())
                Assert.AreEqual(
                    ownCalender.Where(y => y.HolidayDate == attendanceDate.AddDays(x.i))
                               .Select(y => y.HolidayClassification)
                               .First() switch
                    {
                        HolidayClassification.LegalHoliday => XLColor.Gray.Color.Name,
                        HolidayClassification.RegularHoliday => XLColor.LightGray.Color.Name,
                        _ => throw new NotImplementedException(),
                    },
                    x.row.Cell("B").Style.Fill.BackgroundColor.Color.Name
                    , $"{attendanceDate}:塗りつぶしの検証");
        });
        Assert.AreEqual(employeeNumber, workbook.Worksheet(1).Cell("G2").Value);
        Assert.AreEqual(employeeName, workbook.Worksheet(1).Cell("J2").GetString());
    }

    private static IEnumerable<OwnCompanyHoliday> OwnCalender202301() => new List<OwnCompanyHoliday>
    {
        OwnCompanyHoliday.Reconstruct(DateTime.Parse("2023/01/01"), HolidayClassification.LegalHoliday),
        OwnCompanyHoliday.Reconstruct(DateTime.Parse("2023/01/02"), HolidayClassification.RegularHoliday),
        OwnCompanyHoliday.Reconstruct(DateTime.Parse("2023/01/03"), HolidayClassification.RegularHoliday),
        OwnCompanyHoliday.Reconstruct(DateTime.Parse("2023/01/04"), HolidayClassification.RegularHoliday),
        OwnCompanyHoliday.Reconstruct(DateTime.Parse("2023/01/07"), HolidayClassification.RegularHoliday),
        OwnCompanyHoliday.Reconstruct(DateTime.Parse("2023/01/08"), HolidayClassification.LegalHoliday),
        OwnCompanyHoliday.Reconstruct(DateTime.Parse("2023/01/09"), HolidayClassification.RegularHoliday),
        OwnCompanyHoliday.Reconstruct(DateTime.Parse("2023/01/14"), HolidayClassification.RegularHoliday),
        OwnCompanyHoliday.Reconstruct(DateTime.Parse("2023/01/15"), HolidayClassification.LegalHoliday),
        OwnCompanyHoliday.Reconstruct(DateTime.Parse("2023/01/21"), HolidayClassification.RegularHoliday),
        OwnCompanyHoliday.Reconstruct(DateTime.Parse("2023/01/22"), HolidayClassification.LegalHoliday),
        OwnCompanyHoliday.Reconstruct(DateTime.Parse("2023/01/28"), HolidayClassification.RegularHoliday),
        OwnCompanyHoliday.Reconstruct(DateTime.Parse("2023/01/29"), HolidayClassification.LegalHoliday),
    };
}