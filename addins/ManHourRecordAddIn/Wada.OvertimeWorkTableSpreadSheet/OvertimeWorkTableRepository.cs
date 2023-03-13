using ClosedXML.Excel;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using Wada.AOP.Logging;
using Wada.ManHourRecordService;
using Wada.ManHourRecordService.AttendanceAggregation;
using Wada.ManHourRecordService.OvertimeWorkTableCreator;
using Wada.ManHourRecordService.OwnCompanyCalendarAggregation;
using Wada.ManHourRecordService.ValueObjects;

namespace Wada.OvertimeWorkTableSpreadSheet
{
    public class OvertimeWorkTableRepository : IOvertimeWorkTableRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IOwnCompanyHolidayRepository _ownCompanyHolidayRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public OvertimeWorkTableRepository(IConfiguration configuration, IOwnCompanyHolidayRepository overtimeWorkTableRepository, IEmployeeRepository employeeRepository)
        {
            _configuration = configuration;
            _ownCompanyHolidayRepository = overtimeWorkTableRepository;
            _employeeRepository = employeeRepository;
        }

        [Logging]
        public async Task AddAsync(Stream stream, Attendance workDay)
        {
            // 社員情報取得
            var employee = await _employeeRepository.FindByEmployeeNumberAsync(workDay.EmployeeNumber);

            using var xlBook = new XLWorkbook(stream);
            var targetCell = await SearchWorkerCellAsync(xlBook, employee.Name, workDay.AchievementDate.Value);

            // セル背景を塗る
            PaintCellBackground(targetCell, workDay);

            // 残業時間を書き込む
            await WriteOvertimeManHourAsync(targetCell, workDay);

            xlBook.Save();
        }

        [Logging]
        private async Task WriteOvertimeManHourAsync(IXLCell targetCell, Attendance attendance)
        {
            decimal? overtimeHour = null;
            if (attendance.HasActualOvertime)
            {
                var total = attendance.Achievements.Sum(x => x.ManHour);
                try
                {
                    // 休日リストを取得
                    var ownHolidays = await _ownCompanyHolidayRepository.FindByYearMonthAsync(
                            attendance.AchievementDate.Value.Year,
                            attendance.AchievementDate.Value.Month);

                    if (ownHolidays.Where(x => x.HolidayDate == attendance.AchievementDate.Value).Any())
                        overtimeHour = total;
                    else
                        overtimeHour = total - 8;
                }
                catch (OwnCompanyCalendarAggregationException)
                {
                    if (attendance.AchievementDate.Value.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                        overtimeHour = total;
                    else
                        overtimeHour = total - 8;
                }
            }

            targetCell.SetValue(overtimeHour).Style.NumberFormat.SetFormat("0.0");
        }

        [Logging]
        private static void PaintCellBackground(IXLCell targetCell, Attendance attendance)
        {
            targetCell.Style.Fill.BackgroundColor = attendance.DayOffClassification switch
            {
                DayOffClassification.PaidLeave => XLColor.FromArgb(242, 220, 219),
                DayOffClassification.AMPaidLeave => XLColor.FromArgb(235, 241, 222),
                DayOffClassification.PMPaidLeave => XLColor.FromArgb(218, 238, 243),
                DayOffClassification.TransferedAttendance => XLColor.FromArgb(177, 160, 199),
                _ => XLColor.NoColor,
            };
        }

        [Logging]
        private static async Task<IXLCell> SearchWorkerCellAsync(IXLWorkbook xlBook, string workerName, DateTime overtimeDay)
        {
            // 名前の区切り空白を全角半角をあいまいに検索
            var regName = Regex.Replace(workerName, @"[\s　]", @"[\s　]");

            var empCels = await Task.WhenAll(
                xlBook.Worksheets.Where(x => !Regex.IsMatch(x.Name, @"(一覧|三六|祝日)"))
                                 .Select(async sheet => await Task.Run(() => sheet.Cells("B9:B35")
                                 .Where(x => !x.IsEmpty())
                                 .FirstOrDefault(cell => Regex.IsMatch(cell.GetString(), regName)))));

            var empCel = empCels.Where(x => x != null).FirstOrDefault()
                ?? throw new OvertimeWorkTableEmployeeDoseNotFoundException(
                    $"残業実績エクセルにあなたの名前が見つかりません 名前: {workerName}");
            return empCel.CellRight(2 + overtimeDay.Day);
        }

        [Logging]
        public async Task CreateAsync(OvertimeWorkTableOwner overtimeWorkTableOwner)
        {
            if (OvertimeTableExists(overtimeWorkTableOwner))
                return;

            var overtimeDirBase = _configuration["applicationConfiguration:OvertimeTableDirectoryBase"]
                ?? throw new InvalidOperationException("設定情報が取得出来ません(AttendanceTableDirectoryBase) システム担当まで連絡してください");

            var overtimeTablePath = Path.Combine(
                overtimeDirBase,
                $"{overtimeWorkTableOwner.FiscalYear}年度",
                $"{overtimeWorkTableOwner.AttendanceYear}年{overtimeWorkTableOwner.AttendanceMonth:00}月",
                $"残業実績表({overtimeWorkTableOwner.Department}).xlsx");

            // ディレクトリ作成
            FileInfo overtimeTableFileInfo = new(overtimeTablePath);
            if (overtimeTableFileInfo.Directory != null && !overtimeTableFileInfo.Directory.Exists)
                overtimeTableFileInfo.Directory?.Create();

            var template = _configuration["applicationConfiguration:OvertimeTableTemplateDirectory"]
                ?? throw new InvalidOperationException("設定情報が取得出来ません(AttendanceTableDirectoryBase) システム担当まで連絡してください");

            var templatePath = Path.Combine(
                template,
                $"残業実績表({overtimeWorkTableOwner.Department}).xlsx");

            FileInfo templateFileInfo = new(templatePath);
            if (!templateFileInfo.Exists)
                throw new InvalidOperationException(
                    $"残業実績エクセルのテンプレートが見つかりません 部署: {overtimeWorkTableOwner.Department}");

            // ファイルのコピー
            await CopyTemplateToAsync(overtimeTableFileInfo, templateFileInfo);

            // 残業実績エクセルの年月設定
            try
            {
                await InitializeOvertimeTableAsync(overtimeTableFileInfo.FullName, overtimeWorkTableOwner);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                throw new OvertimeWorkTableCreatorException($"テンプレートが壊れています 確認してください 部署: {overtimeWorkTableOwner.Department}", ex);
            }
        }

        [Logging]
        private async Task InitializeOvertimeTableAsync(string overtimeTablePath, OvertimeWorkTableOwner overtimeWorkTableOwner)
        {
            if (!File.Exists(overtimeTablePath))
                throw new FileNotFoundException("残業実績表ファイルが見つかりません", overtimeTablePath);

            using var xlBook = new XLWorkbook(overtimeTablePath);

            xlBook.Worksheets.Where(x => !Regex.IsMatch(x.Name, @"(一覧|三六|祝日)"))
                .ToList()
                .ForEach(x =>
                {
                    x.Cell("W2").Value = overtimeWorkTableOwner.AttendanceYear;
                    x.Cell("Z2").Value = overtimeWorkTableOwner.AttendanceMonth;
                });

            var holiday = await _ownCompanyHolidayRepository.FindByYearMonthAsync(
                overtimeWorkTableOwner.AttendanceYear, overtimeWorkTableOwner.AttendanceMonth);

            var holidaySheet = xlBook.Worksheet("祝日");
            holidaySheet.Column("B").CellsUsed().Last().InsertData(holiday.Select(x => x.HolidayDate));
            xlBook.Save();
        }

        [Logging]
        private static Task CopyTemplateToAsync(FileInfo destFileInfo, FileInfo templateFileInfo)
            => Task.Run(() =>
            {
                if (destFileInfo.Directory != null && !destFileInfo.Directory.Exists)
                    destFileInfo.Directory.Create();
                templateFileInfo.CopyTo(destFileInfo.FullName);
            });

        [Logging]
        public bool OvertimeTableExists(OvertimeWorkTableOwner overtimeWorkTableOwner)
        {
            var overtimeDirBase = _configuration["applicationConfiguration:OvertimeTableDirectoryBase"]
                ?? throw new InvalidOperationException("設定情報が取得出来ません(AttendanceTableDirectoryBase) システム担当まで連絡してください");

            var overtimeTablePath = Path.Combine(
                overtimeDirBase,
                $"{overtimeWorkTableOwner.FiscalYear}年度",
                $"{overtimeWorkTableOwner.AttendanceYear}年{overtimeWorkTableOwner.AttendanceMonth:00}月",
                $"残業実績表({overtimeWorkTableOwner.Department}).xlsx");

            return File.Exists(overtimeTablePath);
        }
    }
}