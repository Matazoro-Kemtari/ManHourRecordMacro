using Wada.AOP.Logging;
using Wada.ManHourRecordService;
using Wada.ManHourRecordService.AttendanceAggregation;

namespace Wada.RecordManHourApplication
{
    public interface IManHourRecordExistsUseCase
    {
        /// <summary>
        /// 実績があるか確認する
        /// </summary>
        /// <param name="attendancePram"></param>
        /// <returns></returns>
        /// <exception cref="ManHourRecordExistsException"/>
        Task ExecuteAsync(AttendanceParam attendancePram);
    }

    public class ManHourRecordExistsUseCase : IManHourRecordExistsUseCase
    {
        private readonly IAttendanceRepository _attendanceRepository;

        public ManHourRecordExistsUseCase(IAttendanceRepository attendanceRepository)
        {
            _attendanceRepository = attendanceRepository;
        }

        [Logging]
        public async Task ExecuteAsync(AttendanceParam attendancePram)
        {
            try
            {
                _ = await _attendanceRepository.FindByEmployeeNumberAndAchievementDateAsync(
                    attendancePram.EmployeeNumber, attendancePram.AchievementDate.Value);
            }
            catch (AttendanceAggregationException)
            {
                // レコードが存在しない
                return;
            }
            throw new ManHourRecordExistsException();
        }
    }
}
