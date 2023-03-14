using ClosedXML.Excel;
using System.Reflection;
using Wada.AOP.Logging;
using Wada.Extensions;
using Wada.ManHourRecordService;
using Wada.ManHourRecordService.AttendanceAggregation;
using Wada.ManHourRecordService.AttendanceTableCreator;
using Wada.ManHourRecordService.EmployeeAggregation;
using Wada.ManHourRecordService.ValueObjects;

namespace Wada.AttendanceSpreadSheet;

public class AttendanceTableRepository : IAttendanceTableRepository
{
    private readonly IOwnCompanyHolidayRepository _workCompanyHolidayRepository;
    private readonly IEmployeeRepository _employeeRepositor;

    public AttendanceTableRepository(IOwnCompanyHolidayRepository workCompanyHolidayRepository, IEmployeeRepository employeeRepository)
    {
        _workCompanyHolidayRepository = workCompanyHolidayRepository;
        _employeeRepositor = employeeRepository;
    }

    [Logging]
    public async Task AddWokedDayAsync(Stream stream, Attendance workDay, Func<string, bool> canOverwriting)
    {
        using var xlBook = new XLWorkbook(stream);
        IXLWorksheet targetSheet = await SearchMonthSheetAsync(xlBook,
                                                          workDay.AchievementDate.Value,
                                                          workDay.EmployeeNumber);

        AddAttendanceRecord(targetSheet,
                            workDay.AchievementDate.Value,
                            workDay.DayOffClassification,
                            workDay.StartTime,
                            workDay.EndTime,
                            canOverwriting);

        xlBook.Save();
    }

    [Logging]
    public async Task CreateAsync(Stream stream, AttendanceTableOwner attendanceTableOwner)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var templateStream = assembly.GetManifestResourceStream("Wada.AttendanceTableSpreadSheet.Resources.AttendanceTemplate.xlsx")
            ?? throw new AttendanceTableCreatorException("リソースが取得出来ません(Resources.AttendanceTemplate.xlsx) システム担当まで連絡してください");

        using var workbook = new XLWorkbook(templateStream);
        IXLWorksheet worksheet = workbook.Worksheet(1);

        // シート名変更
        MakeSheetName(attendanceTableOwner.AttendanceYearMonth, worksheet);

        // カレンダーを作る
        await MakeCalendarAsync(worksheet, attendanceTableOwner.AttendanceYearMonth);

        // 社員情報を変更
        await MakeEmployeeAsync(worksheet, attendanceTableOwner.EmployeeNumber);

        workbook.SaveAs(stream);
    }

    /// <summary>
    /// 社員情報を作る
    /// </summary>
    /// <param name="worksheet"></param>
    /// <param name="employeeNumber"></param>
    /// <exception cref="NotImplementedException"></exception>
    [Logging]
    private async Task MakeEmployeeAsync(IXLWorksheet worksheet, uint employeeNumber)
    {
        // 社員番号
        worksheet.Cell("G2").SetValue(employeeNumber);

        try
        {
            // 社員名
            var employee = await _employeeRepositor.FindByEmployeeNumberAsync(employeeNumber);
            worksheet.Cell("J2").SetValue(employee.Name);
        }
        catch (EmployeeAggregationException ex)
        {
            throw new AttendanceTableCreatorException(ex.Message, ex);
        }
    }

    [Logging]
    private static void MakeSheetName(DateTime attendanceYearMonth, IXLWorksheet worksheet)
        => worksheet.Name = $"{attendanceYearMonth.Month}月";

    /// <summary>
    /// カレンダー部分を作る
    /// </summary>
    /// <param name="worksheet"></param>
    /// <param name="attendanceYearMonth"></param>
    private async Task MakeCalendarAsync(IXLWorksheet worksheet, DateTime attendanceYearMonth)
    {
        // 行頭の年月
        worksheet.Cell("A1").SetValue(attendanceYearMonth);

        // 月欄
        worksheet.Cell("A5").SetValue(attendanceYearMonth.Month);

        // 会社カレンダー取得
        var ownCalendar = await _workCompanyHolidayRepository.FindByYearMonthAsync(attendanceYearMonth.Year, attendanceYearMonth.Month);

        // 日付行ループ
        var dailyRows = worksheet.Rows("5:35"); // 何月でも31日分
        const string DayColumnLetter = "B";
        const string WeekColumnLetter = "C";
        const string TableEndColumnLetter = "L";
        DateTime startDate = new(attendanceYearMonth.Year, attendanceYearMonth.Month, 1);
        int i = 0;
        foreach (var row in dailyRows)
        {
            var today = startDate.AddDays(i);
            if (today.Month == attendanceYearMonth.Month)
            {
                // 曜日
                row.Cell(WeekColumnLetter).Value = $"{today:ddd}";
                // 塗りつぶし
                if (ownCalendar.Any())
                {
                    switch (ownCalendar.Where(x => x.HolidayDate == today)
                                       .Select(x => x.HolidayClassification)
                                       .FirstOrDefault())
                    {
                        case HolidayClassification.LegalHoliday:
                            worksheet.Range(row.Cell(DayColumnLetter), row.Cell(TableEndColumnLetter))
                            .Style.Fill.SetBackgroundColor(XLColor.Gray);
                            break;
                        case HolidayClassification.RegularHoliday:
                            worksheet.Range(row.Cell(DayColumnLetter), row.Cell(TableEndColumnLetter))
                            .Style.Fill.SetBackgroundColor(XLColor.LightGray);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    // 自社カレンダーが無いときのために曜日で塗りつぶし
                    switch (today.DayOfWeek)
                    {
                        case DayOfWeek.Sunday:
                            worksheet.Range(row.Cell(DayColumnLetter), row.Cell(TableEndColumnLetter))
                            .Style.Fill.BackgroundColor = XLColor.Gray;
                            break;
                        case DayOfWeek.Saturday:
                            worksheet.Range(row.Cell(DayColumnLetter), row.Cell(TableEndColumnLetter))
                            .Style.Fill.BackgroundColor = XLColor.LightGray;
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                // 日付消す
                row.Cell(DayColumnLetter).Clear(XLClearOptions.Contents);
                // 塗りつぶし
                worksheet.Range(row.Cell(DayColumnLetter), row.Cell(TableEndColumnLetter))
                .Style.Fill.BackgroundColor = XLColor.DarkGray;
            }
            i++;
        }
    }

    [Logging]
    private static void AddAttendanceRecord(
        IXLWorksheet targetSheet,
        DateTime achievementDate,
        DayOffClassification dayOffClassification,
        TimeSpan? startTime,
        TimeSpan? endTime,
        Func<string, bool> canOverwriting)
    {
        // 目的の行を取得
        var targetRowNumber = achievementDate.Day + 4;
        var attendanceRow = targetSheet.Row(targetRowNumber);

        const string DayOffColumnLetter = "D";
        const string StartedTimeColumnLetter = "E";
        const string EndedTimeColumnLetter = "F";

        // 上書き確認
        if (!attendanceRow.Cell(DayOffColumnLetter).IsEmpty()
            || !attendanceRow.Cell(StartedTimeColumnLetter).IsEmpty()
            || !attendanceRow.Cell(EndedTimeColumnLetter).IsEmpty())
            if (!canOverwriting(
                $"シート: {targetSheet.Name}, 日付: {achievementDate:yyyy/MM/dd}"))
                throw new RecordAbortException("中止しました");

        attendanceRow.Cell(DayOffColumnLetter).Value = dayOffClassification.GetEnumDisplayShortName();
        switch (dayOffClassification)
        {
            case DayOffClassification.None:
            case DayOffClassification.AMPaidLeave:
            case DayOffClassification.PMPaidLeave:
            case DayOffClassification.TransferedAttendance:
            case DayOffClassification.HolidayWorked:
            case DayOffClassification.Lateness:
            case DayOffClassification.EarlyLeave:
                attendanceRow.Cell(StartedTimeColumnLetter).Value = startTime;
                attendanceRow.Cell(EndedTimeColumnLetter).Value = endTime;
                break;
            default:
                attendanceRow.Cell(StartedTimeColumnLetter).Clear(XLClearOptions.Contents);
                attendanceRow.Cell(EndedTimeColumnLetter).Clear(XLClearOptions.Contents);
                break;
        }
    }

    [Logging]
    private static async Task<IXLWorksheet> SearchMonthSheetAsync(IXLWorkbook xlBook, DateTime workedDay, uint workedEmployeeNumber)
    {
        var month = workedDay.Month;
        string searchingSheetName = $"{month}月";

        try
        {
            var targetSheet = await xlBook.Worksheets
                .Where(x => x.Name == searchingSheetName)
                .Select(async sheet => await Task.Run(() =>
                {
                    if (!sheet.Cell("A1").TryGetValue(out DateTime yearMonth))
                        throw new DomainException(
                        $"年月が取得できません シート:{sheet.Name}, セル:A1");

                    if (!sheet.Cell("G2").TryGetValue(out uint employeeNumber))
                        throw new DomainException(
                        $"社員番号が取得できません シート:{sheet.Name}, セル:G2");

                    if (employeeNumber != workedEmployeeNumber)
                        throw new DomainException(
                        $"勤務表の社員番号が異なります シート:{sheet.Name}, 社員番号:{employeeNumber}");

                    if (yearMonth.Year != workedDay.Year
                    || yearMonth.Month != workedDay.Month)
                        throw new DomainException(
                        $"{month}月のシートが見つかりません");

                    return sheet;
                }))
                .First();

            return targetSheet;
        }
        catch (InvalidOperationException ex)
        {
            throw new DomainException(
                $"{month}月のシートが見つかりません", ex);
        }
    }
}