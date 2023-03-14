using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using System.Transactions;
using Wada.AOP.Logging;
using Wada.Extensions;
using Wada.IO;
using Wada.ManHourRecordService;
using Wada.ManHourRecordService.AttendanceAggregation;
using Wada.ManHourRecordService.AttendanceTableCreator;
using Wada.ManHourRecordService.EmployeeAggregation;
using Wada.ManHourRecordService.OvertimeWorkTableCreator;
using Wada.ManHourRecordService.ValueObjects;

namespace Wada.RecordManHourApplication;

public interface IRecordManMonthUseCase
{
    Task ExecuteAsync(AttendanceParam attendancePram,
                      Func<string, bool> canAttendanceTableOverwriting);
}

public class RecordManMonthUseCase : IRecordManMonthUseCase
{
    private readonly IConfiguration _configuration;
    private readonly IFileStreamOpener _streamOpener;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAttendanceTableRepository _attendanceTableRepository;
    private readonly IOvertimeWorkTableRepository _overtimeWorkTableRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IWorkedRecordAgentRepository _workedRecordAgentRepository;

    public RecordManMonthUseCase(IConfiguration configuration, IFileStreamOpener streamOpener, IEmployeeRepository employeeRepository, IAttendanceTableRepository attendanceTableRepository, IOvertimeWorkTableRepository overtimeWorkTableRepository, IAttendanceRepository attendanceRepository, IWorkedRecordAgentRepository workedRecordAgentRepository)
    {
        _configuration = configuration;
        _streamOpener = streamOpener;
        _employeeRepository = employeeRepository;
        _attendanceTableRepository = attendanceTableRepository;
        _overtimeWorkTableRepository = overtimeWorkTableRepository;
        _attendanceRepository = attendanceRepository;
        _workedRecordAgentRepository = workedRecordAgentRepository;
    }

    [Logging]
    public async Task ExecuteAsync(AttendanceParam attendancePram,
                                   Func<string, bool> canAttendanceTableOverwriting)
    {
        using MemoryStream attendanceMemoryStream = new();
        using MemoryStream overtimeMemoryStream = new();
        using MemoryStream dailyReportMemoryStream = new();
        Stream[] streams;
        try
        {
            streams = await Task.WhenAll(
                // 勤務表を開く
                OpenAttendanceStreamAsync(attendancePram, attendanceMemoryStream),
                // 残業実績表を開く
                OpenOvertimeStreamAsync(attendancePram, overtimeMemoryStream),
                // 日報を開く
                OpenDailyAchievementStreamAsync(attendancePram, dailyReportMemoryStream));
        }
        catch (Exception ex) when (ex is InvalidOperationException or EmployeeAggregationException
                or OvertimeWorkTableCreatorException or UseCaseException or FileStreamOpenerException)
        {
            throw new UseCaseException(ex.Message, ex);
        }

        using Stream attendanceFileStream = streams[0];
        using Stream overtimeFileStream = streams[1];
        using Stream dailyReortFileStram = streams[2];

        Attendance achievement;
        try
        {
            achievement = attendancePram.Convert();
        }
        catch (Exception ex) when (ex is AttendanceAggregationException or WorkingNumberException)
        {
            throw new RecordAbortException(ex.Message, ex);
        }

        var workBookTask = Task.WhenAll(
            // 勤務表に追加する
            _attendanceTableRepository.AddWokedDayAsync(attendanceMemoryStream, achievement, canAttendanceTableOverwriting),

            // 残業実績表に追加する
            _overtimeWorkTableRepository.AddAsync(overtimeMemoryStream, achievement),

            // 受注管理日報に追加する
            _workedRecordAgentRepository.AddAsync(dailyReportMemoryStream, achievement)
        );

        try
        {
            await workBookTask.ContinueWith(async _ =>
            {
                attendanceMemoryStream.Seek(0, SeekOrigin.Begin);
                overtimeMemoryStream.Seek(0, SeekOrigin.Begin);
                dailyReportMemoryStream.Seek(0, SeekOrigin.Begin);
                attendanceFileStream.SetLength(0);
                overtimeFileStream.SetLength(0);
                dailyReortFileStram.SetLength(0);
                // 全て成功したらファイルに書き込む
                await Task.WhenAll(
                    attendanceMemoryStream.CopyToAsync(attendanceFileStream),
                    overtimeMemoryStream.CopyToAsync(overtimeFileStream),
                    dailyReportMemoryStream.CopyToAsync(dailyReortFileStram)
                );

                // streamより前で宣言するとエラーになる!?
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                // 永続化
                await _attendanceRepository.RemoveByEmployeeNumberAndAchievementDateAsync(achievement.EmployeeNumber, achievement.AchievementDate.Value);
                await _attendanceRepository.AddAsync(achievement);
                scope.Complete();
            },
            TaskContinuationOptions.OnlyOnRanToCompletion);
        }
        catch (Exception ex) when (ex is OvertimeWorkTableEmployeeDoseNotFoundException)
        {
            throw new OvertimeWorkTableEmployeeDoseNotFoundApplicationException(ex.Message, ex);
        }
        catch (Exception ex) when (ex is DomainException or EmployeeAggregationException)
        {
            throw new UseCaseException(ex.InnerException.Message, ex.InnerException);
        }
    }

    /// <summary>
    /// 勤務表を開く
    /// </summary>
    /// <param name="attendanceParam"></param>
    /// <returns></returns>
    [Logging]
    private async Task<Stream> OpenAttendanceStreamAsync(AttendanceParam attendanceParam, MemoryStream memoryStream)
    {
        // 設定から勤務表保存フォルダを取得
        var attendanceTableDirectoryPathBase = _configuration["applicationConfiguration:AttendanceTableDirectoryBase"]
            ?? throw new InvalidOperationException("設定情報が取得出来ません(AttendanceTableDirectoryBase) システム担当まで連絡してください");

        var employee = await _employeeRepository.FindByEmployeeNumberAsync(attendanceParam.EmployeeNumber);

        var path = Path.Combine(
            attendanceTableDirectoryPathBase,
            attendanceParam.Department,
            $"{attendanceParam.AchievementDate.Value.FiscalYear()}年度",
            $"{attendanceParam.AchievementDate.Value.FiscalYear()}年度{attendanceParam.AchievementDate.Value.Month:00}月" +
            $"_勤務表_{ReplaceInvalidChar(employee.Name)}.xlsx");

        FileInfo fileInfo = new(path);
        if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
            fileInfo.Directory.Create();

        var fileExists = fileInfo.Exists;
        var fileStream = _streamOpener.OpenOrCreate(path)
            ?? throw new UseCaseException(
                $"ファイルが開けませんでした Path: {path}");

        if (!fileExists)
        {
            AttendanceTableOwner attendanceTableOwner = new(attendanceParam.AchievementDate.Value, attendanceParam.EmployeeNumber);
            await _attendanceTableRepository.CreateAsync(fileStream, attendanceTableOwner);
        }

        fileStream.Seek(0, SeekOrigin.Begin);
        fileStream.CopyTo(memoryStream);
        return fileStream;
    }

    /// <summary>
    /// 使用禁止文字の置換
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    [Logging]
    private static string ReplaceInvalidChar(string source) // TODO: 拡張メソッドにする Windows用として
    {
        string result = source;

        // 禁忌文字を置換する
        Path.GetInvalidFileNameChars()
            .ToList()
            .ForEach(x => result = result.Replace(x, '_'));

        //正規表現を設定
        string pattern = "[\\x00-\\x1f<>:\"/\\\\|?*]|^(CON|PRN|AUX|NUL|COM[0-9]|LPT[0-9]|CLOCK\\$)(\\.|$)|[\\. ]$";
        var regex = new Regex(pattern);
        if (regex.IsMatch(result))
            result = regex.Replace(result, "_");

        return result;
    }

    /// <summary>
    /// 残業実績表を開く
    /// </summary>
    /// <param name="attendanceParam"></param>
    /// <param name="memoryStream"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="UseCaseException"></exception>
    [Logging]
    private async Task<Stream> OpenOvertimeStreamAsync(AttendanceParam attendanceParam, MemoryStream memoryStream)
    {
        OvertimeWorkTableOwner tableOwner = new(attendanceParam.AchievementDate.Value, attendanceParam.Department);
        // 残業実績エクセルの存在確認
        if (!_overtimeWorkTableRepository.OvertimeTableExists(tableOwner))
            await _overtimeWorkTableRepository.CreateAsync(tableOwner);

        // 設定から残業実績エクセルフォルダ取得
        var overtimeTableDirectoryBase = _configuration["applicationConfiguration:OvertimeTableDirectoryBase"]
            ?? throw new InvalidOperationException("設定情報が取得出来ません(OvertimeTableDirectoryBase) システム担当まで連絡してください");
        var path = Path.Combine(
            overtimeTableDirectoryBase,
            $"{attendanceParam.AchievementDate.Value.FiscalYear()}年度",
            $"{attendanceParam.AchievementDate.Value.Year}年{attendanceParam.AchievementDate.Value.Month:00}月",
            $"残業実績表({ReplaceInvalidChar(attendanceParam.Department)}).xlsx");

        var fileStream = _streamOpener.OpenOrCreate(path)
            ?? throw new UseCaseException(
                $"ファイルが開けませんでした Path: {path}");

        fileStream.Seek(0, SeekOrigin.Begin);
        await fileStream.CopyToAsync(memoryStream);
        return fileStream;
    }

    /// <summary>
    /// 日報を開く
    /// </summary>
    /// <param name="attendanceParam"></param>
    /// <param name="memoryStream"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [Logging]
    private async Task<Stream> OpenDailyAchievementStreamAsync(AttendanceParam attendanceParam, MemoryStream memoryStream)
    {
        var dailyAchievementTableBase = _configuration["applicationConfiguration:DailyAchievementTableBase"]
            ?? throw new InvalidOperationException("設定情報が取得出来ません(DailyAchievementTableBase) システム担当まで連絡してください");
        var path = Path.Combine(
            dailyAchievementTableBase,
            attendanceParam.Department,
            $"{attendanceParam.AchievementDate.Value:yyyyMMdd}.xlsx");

        FileInfo fileInfo = new(path);
        if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
            fileInfo.Directory.Create();

        var fileExists = fileInfo.Exists;
        var fileStream = _streamOpener.OpenOrCreate(path);

        if (!fileExists)
        {
            _workedRecordAgentRepository.Create(fileStream);
        }

        fileStream.Seek(0, SeekOrigin.Begin);
        await fileStream.CopyToAsync(memoryStream);
        return fileStream;
    }
}

public record class AttendanceParam(uint EmployeeNumber,
                                    AchievementDateParam AchievementDate,
                                    TimeSpan? StartTime,
                                    DayOffClassificationAttempt DayOffClassification,
                                    string Department,
                                    IEnumerable<AchievementParam> Achievements)
{
    public static AttendanceParam Parse(Attendance attendance)
        => new(attendance.EmployeeNumber,
               AchievementDateParam.Parse(attendance.AchievementDate),
               attendance.StartTime,
               (DayOffClassificationAttempt)attendance.DayOffClassification,
               attendance.Department,
               attendance.Achievements.Select(x => AchievementParam.Parse(x)));

    internal Attendance Convert()
        => new(EmployeeNumber,
               AchievementDate.Convert(),
               StartTime,
               (DayOffClassification)DayOffClassification,
               Department,
               Achievements.Select(x => x.Convert()));

    public uint EmployeeNumber { get; } = EmployeeNumber;
    public AchievementDateParam AchievementDate { get; } = AchievementDate;
    public TimeSpan? StartTime { get; } = StartTime;
    public DayOffClassificationAttempt DayOffClassification { get; } = DayOffClassification;
    public string Department { get; } = Department;
    public IEnumerable<AchievementParam> Achievements { get; } = Achievements;
}

public enum DayOffClassificationAttempt
{
    None,

    /// <summary>
    /// 有給休暇
    /// </summary>
    PaidLeave,

    /// <summary>
    /// 午前有給休暇
    /// </summary>
    AMPaidLeave,

    /// <summary>
    /// 午後有給休暇
    /// </summary>
    PMPaidLeave,

    /// <summary>
    /// 振替休日
    /// </summary>
    SubstitutedHoliday,

    /// <summary>
    /// 振替出勤
    /// </summary>
    TransferedAttendance,

    /// <summary>
    /// 休日出勤
    /// </summary>
    HolidayWorked,

    /// <summary>
    /// 特別休暇(有給)
    /// </summary>
    PaidSpecialLeave,

    /// <summary>
    /// 特別休暇(無給)
    /// </summary>
    UnpaidSpecialLeave,

    /// <summary>
    /// 欠勤
    /// </summary>
    Absence,

    /// <summary>
    /// 遅刻
    /// </summary>
    Lateness,

    /// <summary>
    /// 早退
    /// </summary>
    EarlyLeave,
}

public static class DayOffClassificationExtensions
{
    public static DayOffClassificationAttempt ToDayOffClassificationAttempt(this string value)
        => value switch
        {
            "有休" => DayOffClassificationAttempt.PaidLeave,
            "AM有" => DayOffClassificationAttempt.AMPaidLeave,
            "PM有" => DayOffClassificationAttempt.PMPaidLeave,
            "振休" => DayOffClassificationAttempt.SubstitutedHoliday,
            "振出" => DayOffClassificationAttempt.TransferedAttendance,
            "休出" => DayOffClassificationAttempt.HolidayWorked,
            "欠勤" => DayOffClassificationAttempt.Absence,
            "遅刻" => DayOffClassificationAttempt.Lateness,
            "早退" => DayOffClassificationAttempt.EarlyLeave,
            "特休有給" => DayOffClassificationAttempt.PaidSpecialLeave,
            "特休無給" => DayOffClassificationAttempt.UnpaidSpecialLeave,
            _ => DayOffClassificationAttempt.None,
        };
}

public record class AchievementDateParam(DateTime Value)
{
    internal static AchievementDateParam Parse(AchievementDate achievementDate)
        => new(achievementDate.Value);

    internal AchievementDate Convert()
        => new(Value);

    public DateTime Value { get; } = Value;
}

public class TestAttendanceParamFactory
{
    public static AttendanceParam Create(
        uint employeeNumber = 9999u,
        DateTime? achievementDate = default,
        TimeSpan? startTime = default,
        DayOffClassificationAttempt dayOffClassification = DayOffClassificationAttempt.None,
        string department = "総務部",
        IEnumerable<AchievementParam>? achievements = default)
        => AttendanceParam.Parse(
            TestAttendanceFactory.Create(employeeNumber,
                                         achievementDate == null ? null : new AchievementDate(achievementDate.Value),
                                         startTime,
                                         (DayOffClassification)dayOffClassification,
                                         department,
                                         achievements?.Select(x => x.Convert())));
}

public record class AchievementParam(string WorkingNumber,
                                     string? Det,
                                     string AchievementProcess,
                                     string MajorWorkingClassification,
                                     string? MiddleWorkingClassification,
                                     decimal ManHour,
                                     string? Note)
{
    internal static AchievementParam Parse(Achievement achievement)
        => new(achievement.WorkingNumber.ToString(),
               achievement.Det,
               achievement.AchievementProcess,
               achievement.MajorWorkingClassification,
               achievement.MiddleWorkingClassification,
               achievement.ManHour,
               achievement.Note);

    internal Achievement Convert()
        => new(new WorkingNumber(WorkingNumber),
               Det,
               AchievementProcess,
               MajorWorkingClassification,
               MiddleWorkingClassification,
               ManHour,
               Note);

    string WorkingNumber { get; } = WorkingNumber;
    string? Det { get; } = Det;
    string AchievementProcess { get; } = AchievementProcess;
    string MajorWorkingClassification { get; } = MajorWorkingClassification;
    string? MiddleWorkingClassification { get; } = MiddleWorkingClassification;
    decimal ManHour { get; } = ManHour;
    string? Note { get; } = Note;
}

public class TestAchievementParamFactory
{
    public static AchievementParam Create(string workingNumber = "99Z-001",
                                          string det = "本体",
                                          string achievementProcess = "設計",
                                          string majorWorkingClassification = "大分類甲",
                                          string middleWorkingClassification = "中分類甲-乙",
                                          decimal manHour = 1.25m,
                                          string note = "テストデータ")
        => AchievementParam.Parse(
            TestAchievementFactory.Create(new WorkingNumber(workingNumber),
                                          det,
                                          achievementProcess,
                                          majorWorkingClassification,
                                          middleWorkingClassification,
                                          manHour,
                                          note));
}
