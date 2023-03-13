using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.ManHourRecordService;
using Wada.ManHourRecordService.AttendanceAggregation;

namespace Wada.RecordManHourApplication.Tests
{
    [TestClass()]
    public class ManHourRecordExistsUseCaseTests
    {
        [TestMethod()]
        public async Task 正常系_実績がないときは正常終了すること()
        {
            // given
            // when
            Mock<IAttendanceRepository> mock_attendance = new();
            mock_attendance.Setup(x => x.FindByEmployeeNumberAndAchievementDateAsync(
                It.IsAny<uint>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new AttendanceAggregationException());

            IManHourRecordExistsUseCase useCase =
                new ManHourRecordExistsUseCase(mock_attendance.Object);

            var attendancePram = TestAttendanceParamFactory.Create();
            Task target() => useCase.ExecuteAsync(attendancePram);

            // then
            await target();
        }

        [TestMethod]
        public async Task 異常系_実績があるときは例外を返すこと()
        {
            Mock<IAttendanceRepository> mock_attendance = new();
            mock_attendance.Setup(x => x.FindByEmployeeNumberAndAchievementDateAsync(
                It.IsAny<uint>(), It.IsAny<DateTime>()));

            IManHourRecordExistsUseCase useCase =
                new ManHourRecordExistsUseCase(mock_attendance.Object);

            var attendancePram = TestAttendanceParamFactory.Create();
            Task target() => useCase.ExecuteAsync(attendancePram);

            // then
            var ex = await Assert.ThrowsExceptionAsync<ManHourRecordExistsException>(target);
        }
    }
}