using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Wada.AOP.Logging;
using Wada.Data.DesignDepartmentDataBse.Entities;
using Wada.Extensions;
using Wada.ManHourRecordService;
using Wada.ManHourRecordService.AttendanceAggregation;
using Wada.ManHourRecordService.ValueObjects;

namespace Wada.Data.DesignDepartmentDataBse
{
    public class AttendanceRepository : IAttendanceRepository
    {
        [Logging]
        public async Task AddAsync(ManHourRecordService.AttendanceAggregation.Attendance attendance)
        {
            var record = new Entities.Attendance()
            {
                Id = attendance.Id.ToString(),
                EmployeeNumber = (int)attendance.EmployeeNumber,
                AchievementDate = attendance.AchievementDate.Value,
                StartTime = attendance.StartTime,
                DayOffClassification = attendance.DayOffClassification.GetEnumDisplayShortName(),
                Department = attendance.Department,
            };
            using (var dbContext = new DesignDepartmentEntities())
            {
                var taskAttendance = dbContext.Attendances.Add(record);
                attendance.Achievements
                    .Select(x => new Entities.Achievement()
                    {
                        Id = x.Id.ToString(),
                        WorkingNumber = x.WorkingNumber.ToString(),
                        Det = x.Det,
                        AchievementProcess = x.AchievementProcess,
                        MajorWorkingClassification = x.MajorWorkingClassification,
                        MiddleWorkingClassification = x.MiddleWorkingClassification,
                        ManHour = x.ManHour,
                        Note = x.Note,
                        AttendanceId = attendance.Id.ToString(),
                    }).ToList().ForEach(x => dbContext.Achievements.Add(x));

                try
                {
                    await dbContext.SaveChangesAsync();

                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    foreach (var errors in ex.EntityValidationErrors)
                    {
                        foreach (var error in errors.ValidationErrors)
                        {
                            // VisualStudioの出力に表示
                            System.Diagnostics.Trace.WriteLine(error.ErrorMessage);
                        }
                    }
                    throw ex;
                }
            }
        }

        [Logging]
        public async Task RemoveByEmployeeNumberAndAchievementDateAsync(uint employeeNumber, DateTime achievementDate)
        {
            using (var dbContext = new DesignDepartmentEntities())
            {
                var attendanceRecord = dbContext.Attendances
                    .Where(x => x.EmployeeNumber == employeeNumber)
                    .Where(x => x.AchievementDate == achievementDate)
                    .Single();
                var achievementRecords = dbContext.Achievements.Where(x => x.AttendanceId == attendanceRecord.Id);
                dbContext.Attendances.Remove(attendanceRecord);
                dbContext.Achievements.RemoveRange(achievementRecords);

                try
                {
                    await dbContext.SaveChangesAsync();

                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    foreach (var errors in ex.EntityValidationErrors)
                    {
                        foreach (var error in errors.ValidationErrors)
                        {
                            // VisualStudioの出力に表示
                            System.Diagnostics.Trace.WriteLine(error.ErrorMessage);
                        }
                    }
                    throw ex;
                }
            }
        }

        [Logging]
        public async Task<ManHourRecordService.AttendanceAggregation.Attendance> FindByIdAsync(string id)
        {
            using (var dbContext = new DesignDepartmentEntities())
            {
                var attendance = await dbContext.Attendances
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync()
                ?? throw new AttendanceAggregationException(
                        $"勤務実績が見つかりませんでした ID: {id}");

                var achievements = await dbContext.Achievements
                    .Where(x => x.AttendanceId == attendance.Id)
                    .ToListAsync();

                return ManHourRecordService.AttendanceAggregation.Attendance.Reconstruct(
                    attendance.Id,
                    attendance.EmployeeNumber,
                    new AchievementDate(attendance.AchievementDate),
                    attendance.StartTime,
                    attendance.DayOffClassification == null
                    ? DayOffClassification.None
                    : attendance.DayOffClassification.ToDayOffClassification(),
                    attendance.Department,
                    achievements.Select(x => ManHourRecordService.AttendanceAggregation.Achievement.Reconstruct(
                        x.Id,
                        x.WorkingNumber,
                        x.Det,
                        x.AchievementProcess,
                        x.MajorWorkingClassification,
                        x.MiddleWorkingClassification,
                        x.ManHour,
                        x.Note)));
            }
        }

        [Logging]
        public async Task<ManHourRecordService.AttendanceAggregation.Attendance> FindByEmployeeNumberAndAchievementDateAsync(uint employeeNumber, DateTime achievementDate)
        {
            using (var dbContext = new DesignDepartmentEntities())
            {
                var attendance = await dbContext.Attendances
                    .Where(x => x.EmployeeNumber == employeeNumber)
                    .Where(x => x.AchievementDate == achievementDate)
                    .FirstOrDefaultAsync()
                    ?? throw new AttendanceAggregationException(
                        $"勤務実績が見つかりませんでした 社員番号: {employeeNumber}, 実績日: {achievementDate:yyyy/MM/dd}");

                var achievements = await dbContext.Achievements
                    .Where(x => x.AttendanceId == attendance.Id)
                    .ToListAsync();

                return ManHourRecordService.AttendanceAggregation.Attendance.Reconstruct(
                    attendance.Id,
                    attendance.EmployeeNumber,
                    new AchievementDate(attendance.AchievementDate),
                    attendance.StartTime,
                    attendance.DayOffClassification == null
                    ? DayOffClassification.None
                    : attendance.DayOffClassification.ToDayOffClassification(),
                    attendance.Department,
                    achievements.Select(x => ManHourRecordService.AttendanceAggregation.Achievement.Reconstruct(
                        x.Id,
                        x.WorkingNumber,
                        x.Det,
                        x.AchievementProcess,
                        x.MajorWorkingClassification,
                        x.MiddleWorkingClassification,
                        x.ManHour,
                        x.Note)));
            }
        }
    }
}
