using Wada.Extensions;

namespace Wada.ManHourRecordService.OvertimeWorkTableCreator
{
    public record class OvertimeWorkTableOwner
    {
        public OvertimeWorkTableOwner(DateTime attendanceYearMonth, string department)
        {
            AttendanceYear = attendanceYearMonth.Year;
            AttendanceMonth = attendanceYearMonth.Month;
            Department = department;
            FiscalYear = attendanceYearMonth.FiscalYear();
        }

        public int AttendanceYear { get; }

        public int AttendanceMonth { get; }

        public string Department { get; }

        public int FiscalYear { get; }
    }
}
