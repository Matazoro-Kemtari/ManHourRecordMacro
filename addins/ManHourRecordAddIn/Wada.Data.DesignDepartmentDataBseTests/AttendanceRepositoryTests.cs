using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Wada.ManHourRecordService;
using Wada.ManHourRecordService.AttendanceAggregation;

namespace Wada.Data.DesignDepartmentDataBse.Tests
{
    [TestClass()]
    public class AttendanceRepositoryTests
    {
        [TestMethod()]
        public async Task 正常系_勤務実績が追加できること()
        {
            // given
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // when
                IAttendanceRepository repository = new AttendanceRepository();
                var source = TestAttendanceFactory.Create();
                await repository.AddAsync(source);

                var attendance = await repository.FindByIdAsync(source.Id.ToString());
                Assert.AreEqual(source.Id, attendance.Id);
                Assert.AreEqual(source.EmployeeNumber, attendance.EmployeeNumber);
                Assert.AreEqual(source.AchievementDate, attendance.AchievementDate);
                Assert.AreEqual(source.StartTime, attendance.StartTime);
                Assert.AreEqual(source.DayOffClassification, attendance.DayOffClassification);
                Assert.AreEqual(source.Department, attendance.Department);
                CollectionAssert.AreEquivalent(source.Achievements.ToList(), attendance.Achievements.ToList());
            }
        }

        [TestMethod()]
        public async Task 正常系_IDで勤務実績が取得できること()
        {
            // given
            // when
            IAttendanceRepository repository = new AttendanceRepository();
            var actual = await repository.FindByIdAsync("01GSRV12EZZRNS9ER29NK4KF9G");

            // then
            Assert.AreEqual("01GSRV12EZZRNS9ER29NK4KF9G", actual.Id.ToString());
            Assert.AreEqual(5000u, actual.EmployeeNumber);
            Assert.AreEqual(DateTime.Parse("2023/2/15"), actual.AchievementDate.Value);
            Assert.AreEqual(TimeSpan.Parse("8:00"), actual.StartTime);
            Assert.AreEqual(ManHourRecordService.ValueObjects.DayOffClassification.None, actual.DayOffClassification);
            Assert.AreEqual("総務部", actual.Department);
        }

        [TestMethod()]
        public async Task 異常系_IDが無いとき例外を返すこと()
        {
            // given
            // when
            IAttendanceRepository repository = new AttendanceRepository();
            var id = "FOO";
            Task target()
                => repository.FindByIdAsync(id);

            // then
            var ex = await Assert.ThrowsExceptionAsync<AttendanceAggregationException>(target);
            Assert.AreEqual($"勤務実績が見つかりませんでした ID: {id}", ex.Message);
        }

        [TestMethod()]
        public async Task 正常系_社員番号と実績日で勤務実績が取得できること()
        {
            // given
            // when
            IAttendanceRepository repository = new AttendanceRepository();
            var employeeNumber = 9999u;
            var achievementDate = DateTime.Parse("2023/2/15");
            var actual = await repository.FindByEmployeeNumberAndAchievementDateAsync(
                employeeNumber, achievementDate);

            // then
            Assert.AreEqual("01GSA12MG0F9WDGXXFCH3E2WJZ", actual.Id.ToString());
            Assert.AreEqual(employeeNumber, actual.EmployeeNumber);
            Assert.AreEqual(achievementDate, actual.AchievementDate.Value);
            Assert.AreEqual(TimeSpan.Parse("8:00"), actual.StartTime);
            Assert.AreEqual(ManHourRecordService.ValueObjects.DayOffClassification.None, actual.DayOffClassification);
            Assert.AreEqual("総務部", actual.Department);
            Assert.AreEqual(8, actual.Achievements.Count());
        }

        [TestMethod()]
        public async Task 異常系_社員番号と実績日が無いとき例外を返すこと()
        {
            // given
            // when
            IAttendanceRepository repository = new AttendanceRepository();
            var employeeNumber = 9999u;
            var achievementDate = DateTime.Parse("2023/2/1");
            Task target()
                => repository.FindByEmployeeNumberAndAchievementDateAsync(
                    employeeNumber, achievementDate);

            // then
            var ex = await Assert.ThrowsExceptionAsync<AttendanceAggregationException>(target);
            Assert.AreEqual(
                $"勤務実績が見つかりませんでした 社員番号: {employeeNumber}, 実績日: {achievementDate:yyyy/MM/dd}",
                ex.Message);
        }
    }
}