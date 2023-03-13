using Wada.AOP.Logging;
using Wada.Extensions;
using Wada.ManHourRecordService.ValueObjects;

namespace Wada.ManHourRecordService.AttendanceAggregation;

/// <summary>
/// 勤務
/// </summary>
public record class Attendance
{
    public Attendance(uint employeeNumber, AchievementDate achievementDate, TimeSpan? startTime, DayOffClassification dayOffClassification, string department, IEnumerable<Achievement> achievements)
    {
        Id = Ulid.NewUlid();
        EmployeeNumber = employeeNumber;
        AchievementDate = achievementDate;
        StartTime = startTime;
        DayOffClassification = dayOffClassification;
        Department = department ?? throw new ArgumentNullException(nameof(department));
        Achievements = achievements ?? throw new ArgumentNullException(nameof(achievements));

        switch (dayOffClassification)
        {
            case DayOffClassification.None:
                if (achievements.Sum(x => x.ManHour) < 8m)
                    // 勤務区分がブランクのとき工数は8時間以上
                    throw new AttendanceAggregationException(
                        $"勤務区分が{dayOffClassification.GetEnumDisplayName()}のときは8時間以上の工数を入力してください");
                break;

            case DayOffClassification.PaidLeave:
            case DayOffClassification.Absence:
            case DayOffClassification.SubstitutedHoliday:
            case DayOffClassification.PaidSpecialLeave:
            case DayOffClassification.UnpaidSpecialLeave:
                if (achievements.Any(x => x.ManHour > 0m))
                    // 有給/振休/欠勤/特休のとき工数は無し
                    throw new AttendanceAggregationException(
                        $"勤務区分が{dayOffClassification.GetEnumDisplayName()}のときは工数を入れないでください");
                break;

            case DayOffClassification.Lateness:
            case DayOffClassification.EarlyLeave:
                if (achievements.Sum(x => x.ManHour) > 8m)
                    // 遅刻 / 早退のとき工数は8時間未満
                    throw new AttendanceAggregationException(
                        $"勤務区分が{dayOffClassification.GetEnumDisplayName()}のときは8時間より少ない工数を入力してください");
                break;

            case DayOffClassification.AMPaidLeave:
            case DayOffClassification.PMPaidLeave:
            case DayOffClassification.TransferedAttendance:
            case DayOffClassification.HolidayWorked:
                if (!achievements.Any() || achievements.Any(x => x.ManHour == 0))
                    throw new AttendanceAggregationException(
                        $"勤務区分が{dayOffClassification.GetEnumDisplayName()}のときは工数を入力してください");
                break;

            default:
                throw new NotImplementedException();
        }

        HasActualOvertime = DetermineActualOvertime(achievementDate, dayOffClassification, achievements);
    }

    private Attendance(Ulid id, uint employeeNumber, AchievementDate achievementDate, TimeSpan? startTime, DayOffClassification dayOffClassification, string department, IEnumerable<Achievement> achievements)
        : this(employeeNumber, achievementDate, startTime, dayOffClassification, department, achievements)
    {
        Id = id;
    }

    /// <summary>
    /// インフラ層専用
    /// </summary>
    /// <param name="id"></param>
    /// <param name="employeeNumber"></param>
    /// <param name="achievementDate"></param>
    /// <param name="startTime"></param>
    /// <param name="dayOffClassification"></param>
    /// <param name="department"></param>
    /// <param name="achievements"></param>
    /// <returns></returns>
    public static Attendance Reconstruct(string id, int employeeNumber, AchievementDate achievementDate, TimeSpan? startTime, DayOffClassification dayOffClassification, string department, IEnumerable<Achievement> achievements)
        => new(Ulid.Parse(id), (uint)employeeNumber, achievementDate, startTime, dayOffClassification, department, achievements);

    [Logging]
    private static bool DetermineActualOvertime(
        AchievementDate achievementDate,
        DayOffClassification dayOffClassification,
        IEnumerable<Achievement> achievements)
    {
        switch (dayOffClassification)
        {
            case DayOffClassification.PaidLeave:
            case DayOffClassification.Absence:
            case DayOffClassification.SubstitutedHoliday:
            case DayOffClassification.PaidSpecialLeave:
            case DayOffClassification.UnpaidSpecialLeave:
                return false;

            case DayOffClassification.HolidayWorked:
                if (achievementDate.Value.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday)
                    // 1日だけを見て残業の判断はできないが 推定残業とする
                    // 日曜勤務は残業と言えるかについても 法解釈より一般感覚で残業と推定する
                    return true;
                else
                    break;
            default:
                break;
        }
        var manHourOfDay = achievements.Sum(x => x.ManHour);
        return manHourOfDay > 8m;
    }

    public Ulid Id { get; }

    /// <summary>
    /// 社員番号
    /// </summary>
    public uint EmployeeNumber { get; }

    /// <summary>
    /// 実績日付
    /// </summary>
    public AchievementDate AchievementDate { get; }

    /// <summary>
    /// 始業時間
    /// </summary>
    public TimeSpan? StartTime { get; }

    /// <summary>
    /// 休日区分
    /// </summary>
    public DayOffClassification DayOffClassification { get; }

    /// <summary>
    /// 所属
    /// </summary>
    public string Department { get; }

    /// <summary>
    /// 実績工数
    /// </summary>
    public IEnumerable<Achievement> Achievements { get; }

    /// <summary>
    /// 実績工数で残業したか
    /// </summary>
    public bool HasActualOvertime { get; }

    /// <summary>
    /// 終業時間
    /// </summary>
    public TimeSpan? EndTime => CalcEndTime();

    private TimeSpan? CalcEndTime()
    {
        if (StartTime == null)
            return null;

        decimal total = Achievements.Sum(x => x.ManHour);
        if (total > 4m)
            // 実績工数が4時間を超えていたら休憩1時間取ったとみなす
            total += 1m;
        else if (total == 0)
            return StartTime;

        return StartTime + new TimeSpan((int)total, (int)(total % 1 * 60), 0);
    }
}

public class TestAttendanceFactory
{
    public static Attendance Create(uint employeeNumber = 9999u,
                                   AchievementDate? achievementDate = default,
                                   TimeSpan? startTime = default,
                                   DayOffClassification dayOffClassification = DayOffClassification.None,
                                   string department = "総務部",
                                   IEnumerable<Achievement>? achievements = default)
    {
        achievementDate ??= new AchievementDate(DateTime.Now.Date);
        startTime ??= TimeSpan.Parse("8:00");
        achievements ??= new Achievement[]
        {
            TestAchievementFactory.Create(workingNumber: new WorkingNumber("99Z-001")),
            TestAchievementFactory.Create(workingNumber: new WorkingNumber("99Z-002")),
            TestAchievementFactory.Create(workingNumber: new WorkingNumber("99Z-003")),
            TestAchievementFactory.Create(workingNumber: new WorkingNumber("99Z-004")),
            TestAchievementFactory.Create(workingNumber: new WorkingNumber("99Z-006"), manHour: 1m),
            TestAchievementFactory.Create(workingNumber: new WorkingNumber("99Z-005"), manHour: 1m),
            TestAchievementFactory.Create(workingNumber: new WorkingNumber("99Z-007"), manHour: 0.5m),
            TestAchievementFactory.Create(workingNumber: new WorkingNumber("99Z-008"), manHour: 0.5m),
        };

        return new(employeeNumber, achievementDate, startTime.Value, dayOffClassification, department, achievements);
    }
}

/// <summary>
/// 実績工数
/// </summary>
public record class Achievement
{
    public Achievement(WorkingNumber workedNumber, string? det, string achievementProcess, string majorWorkingClassification, string? middleWorkingClassification, decimal manHour, string? note)
    {
        Id = Ulid.NewUlid();
        WorkingNumber = workedNumber ?? throw new ArgumentNullException(nameof(workedNumber));
        Det = det;
        AchievementProcess = achievementProcess ?? throw new ArgumentNullException(nameof(achievementProcess));
        MajorWorkingClassification = majorWorkingClassification ?? throw new ArgumentNullException(nameof(majorWorkingClassification));
        MiddleWorkingClassification = middleWorkingClassification;
        ManHour = manHour;
        Note = note;

        if (string.IsNullOrEmpty(achievementProcess))
            throw new AttendanceAggregationException("実績工程は必須項目です");

        if (string.IsNullOrEmpty(majorWorkingClassification))
            throw new AttendanceAggregationException("大分類は必須項目です");

        if (manHour <= 0m)
            throw new AttendanceAggregationException("工数は必須項目です");
    }

    private Achievement(Ulid id, WorkingNumber workedNumber, string? det, string achievementProcess, string majorWorkingClassification, string? middleWorkingClassification, decimal manHour, string? note)
        : this(workedNumber, det, achievementProcess, majorWorkingClassification, middleWorkingClassification, manHour, note)
    {
        Id = id;
    }

    public static Achievement Reconstruct(string id, string workedNumber, string? det, string achievementProcess, string majorWorkingClassification, string? middleWorkingClassification, decimal manHour, string? note)
        => new(Ulid.Parse(id), new WorkingNumber(workedNumber), det, achievementProcess, majorWorkingClassification, middleWorkingClassification, manHour, note);

    public Ulid Id { get; }

    /// <summary>
    /// 作業No
    /// </summary>
    public WorkingNumber WorkingNumber { get; }

    /// <summary>
    /// DET
    /// </summary>
    public string? Det { get; }

    /// <summary>
    /// 実績工程
    /// </summary>
    public string AchievementProcess { get; }

    /// <summary>
    /// 大分類
    /// </summary>
    public string MajorWorkingClassification { get; }

    /// <summary>
    /// 中分類
    /// </summary>
    public string? MiddleWorkingClassification { get; }

    /// <summary>
    /// 工数
    /// </summary>
    public decimal ManHour { get; }

    /// <summary>
    /// 特記事項
    /// </summary>
    public string? Note { get; }
}

public class TestAchievementFactory
{
    public static Achievement Create(WorkingNumber? workingNumber = default,
                                     string det = "本体",
                                     string achievementProcess = "設計",
                                     string majorWorkingClassification = "大分類甲",
                                     string middleWorkingClassification = "中分類甲-乙",
                                     decimal manHour = 1.25m,
                                     string note = "テストデータ")
        => new(workingNumber ?? new WorkingNumber("99Z-001"),
               det,
               achievementProcess,
               majorWorkingClassification,
               middleWorkingClassification,
               manHour,
               note);
}
