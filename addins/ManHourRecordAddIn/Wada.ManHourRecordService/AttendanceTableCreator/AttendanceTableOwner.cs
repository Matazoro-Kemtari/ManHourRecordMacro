namespace Wada.ManHourRecordService.AttendanceTableCreator;

public record class AttendanceTableOwner(
    DateTime AttendanceYearMonth,
    uint EmployeeNumber)
{
    public DateTime AttendanceYearMonth { get; } = AttendanceYearMonth;

    public uint EmployeeNumber { get; } = EmployeeNumber;
}