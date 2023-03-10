using ClosedXML.Excel;
using Wada.AOP.Logging;
using Wada.ManHourRecordService;
using Wada.ManHourRecordService.AttendanceAggregation;
using Wada.ManHourRecordService.ValueObjects;

namespace Wada.WorkedRecordAgentSpreadSheet;

public class WorkedRecordAgentRepository : IWorkedRecordAgentRepository
{
    private readonly IEmployeeRepository _employeeRepositor;
    private readonly IWorkingLedgerRepository _workingLedgerRepository;

    public WorkedRecordAgentRepository(IEmployeeRepository employeeRepositor, IWorkingLedgerRepository workingLedgerRepository)
    {
        _employeeRepositor = employeeRepositor;
        _workingLedgerRepository = workingLedgerRepository;
    }

    [Logging]
    public async Task AddAsync(Stream stream, Attendance workDay)
    {
        using var xlBook = new XLWorkbook(stream);
        var sheet = xlBook.Worksheet(1);

        var additionalData = await MakeAdditionalDataAsync(workDay);
        if (sheet.Tables.Any())
        {
            var reportTable = sheet.Tables.Table(0);
            // 既存の社員番号の行を取得
            var writedRow = reportTable.DataRange.Rows(x => x.Cell("B").GetValue<uint>().Equals(workDay.EmployeeNumber));
            // 社員番号を0に書き換え
            writedRow.ToList().ForEach(x => x.Cell("B").SetValue(0u));
            // 新規追加
            reportTable.AppendData(additionalData);
            // 先ほど社員番号を0にした行を削除
            var deletableRow = reportTable.DataRange.Rows(x => x.Cell("B").GetValue<uint>().Equals(0u));
            deletableRow.Delete();
        }
        else
        {
            sheet.Columns("A:L").Width = 11.88;

            var range = sheet.RangeUsed();
            _ = sheet.Cell(2, 1).InsertData(additionalData);
            sheet.RangeUsed().CreateTable();
        }

        xlBook.Save();
    }

    private record class AdditionalAchievementRecord(
        DateTime AchievementDate,
        uint EmployeeNumber,
        string Employee,
        decimal TotalManHour,
        WorkingNumber WorkingNumber,
        string JigCode,
        string WorkingName,
        string? Note,
        decimal TargetManHour,
        decimal ManHour,
        string HeaderOfWorkingNumber,
        uint NumberOfWorkingNumber)
    {
        public DateTime AchievementDate { get; } = AchievementDate;
        public uint EmployeeNumber { get; } = EmployeeNumber;
        public string Employee { get; } = Employee;
        public decimal TotalManHour { get; } = TotalManHour;
        public WorkingNumber WorkingNumber { get; } = WorkingNumber;
        public string JigCode { get; } = JigCode;
        public string WorkingName { get; } = WorkingName;
        public string? Note { get; } = Note;
        public decimal TargetManHour { get; } = TargetManHour;
        public decimal ManHour { get; } = ManHour;
        public string HeaderOfWorkingNumber { get; } = HeaderOfWorkingNumber;
        public uint NumberOfWorkingNumber { get; } = NumberOfWorkingNumber;
    }

    [Logging]
    private async Task<AdditionalAchievementRecord[]> MakeAdditionalDataAsync(Attendance workDay)
    {
        var employee = await _employeeRepositor.FindByEmployeeNumberAsync(workDay.EmployeeNumber);
        return await Task.WhenAll(workDay.Achievements.Select(async x =>
            {
                var jigCode = await FetchJigCodeAsync(x.WorkingNumber);

                return new AdditionalAchievementRecord(
                    // 日付
                    workDay.AchievementDate.Value,
                    // 社員番号
                    workDay.EmployeeNumber,
                    // 氏名
                    employee.Name,
                    // 実労働時間
                    workDay.Achievements.Sum(y => y.ManHour),
                    // 作業番号
                    x.WorkingNumber,
                    // コード
                    jigCode,
                    // 作業名
                    x.MajorWorkingClassification,
                    // 特記事項
                    x.Note,
                    // 目標工数
                    x.ManHour,
                    // 工数
                    x.ManHour,
                    // ヘッダ
                    x.WorkingNumber.Header + '-',
                    // 番号
                    x.WorkingNumber.Number
                );
            }));
    }

    [Logging]
    private Task<string> FetchJigCodeAsync(WorkingNumber workingNumber)
        => Task.Run(async () =>
        {
            var workingLedger = await _workingLedgerRepository.FindByWorkingNumberAsync(workingNumber);
            return workingLedger.JigCode switch
            {
                null => workingNumber.Symbol == "Z" ? "間接" : "その他",
                _ => workingLedger.JigCode,
            };
        });

    [Logging]
    public void Create(Stream stream)
    {
        const string InputSheetName = "入力シート";

        using var xlBook = new XLWorkbook();

        //Sheet作成
        var workSheet = xlBook.AddWorksheet(InputSheetName);
        workSheet.Style.Font.FontSize = 11;
        workSheet.Style.Font.FontName = "游ゴシック";

        //ヘッダ作成
        //日付
        workSheet.Cell(1, 1).Value = "日付";
        workSheet.Column(1).Style.NumberFormat.Format = "yyyy/mm/dd";
        //社員番号
        workSheet.Cell(1, 2).Value = "社員番号";
        //氏名
        workSheet.Cell(1, 3).Value = "氏名";
        //実労時間
        workSheet.Cell(1, 4).Value = "実働時間";
        workSheet.Column(4).Style.NumberFormat.Format = "0.0";
        //作業番号
        workSheet.Cell(1, 5).Value = "作業番号";
        //コード
        workSheet.Cell(1, 6).Value = "コード";
        //作業名
        workSheet.Cell(1, 7).Value = "作業名";
        //特記事項
        workSheet.Cell(1, 8).Value = "特記事項";
        //目標工数
        workSheet.Cell(1, 9).Value = "目標工数";
        workSheet.Column(9).Style.NumberFormat.Format = "0.0";
        //工数
        workSheet.Cell(1, 10).Value = "工数";
        workSheet.Column(10).Style.NumberFormat.Format = "0.0";
        //ヘッダ
        workSheet.Cell(1, 11).Value = "ヘッダ";
        //番号
        workSheet.Cell(1, 12).Value = "番号";


        //_ = workSheet.Range(workSheet.Cell(1, 1), workSheet.Cell(1, 12)).CreateTable();

        xlBook.SaveAs(stream);
    }
}