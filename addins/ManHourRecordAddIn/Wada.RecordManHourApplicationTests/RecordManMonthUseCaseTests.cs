using ClosedXML.Excel;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.ManHourRecordService;
using Wada.ManHourRecordService.AttendanceAggregation;
using Wada.ManHourRecordService.EmployeeAggregation;

namespace Wada.RecordManHourApplication.Tests
{
    [TestClass()]
    public class RecordManMonthUseCaseTests
    {
        [TestMethod()]
        public async Task 正常系_8時間勤務のときリポジトリが実行されること()
        {
            // given
            // when
            Mock<IConfiguration> mock_conf = new();
            mock_conf.Setup(x => x["applicationConfiguration:AttendanceTableDirectoryBase"])
                     .Returns(@"C:\debug");
            mock_conf.Setup(x => x["applicationConfiguration:OvertimeTableDirectoryBase"])
                     .Returns(@"C:\debug");
            mock_conf.Setup(x => x["applicationConfiguration:OvertimeTableTemplateDirectory"])
                     .Returns(@"C:\debug");
            mock_conf.Setup(x => x["applicationConfiguration:DailyAchievementTableBase"])
                     .Returns(@"C:\debug");

            // ダミーブック作成
            MemoryStream dummyBook = new();
            using (var xlBook = new XLWorkbook())
            {
                xlBook.AddWorksheet();
                xlBook.SaveAs(dummyBook);
            }

            Mock<IFileStreamOpener> mock_stream = new();
            mock_stream.Setup(x => x.OpenOrCreate(It.IsAny<string>()))
                .Returns(dummyBook);

            Mock<ManHourRecordService.IEmployeeRepository> mock_emp = new();
            mock_emp.Setup(x => x.FindByEmployeeNumberAsync(It.IsAny<uint>()))
                .ReturnsAsync(TestEmployeeFactory.Create());

            Mock<IAttendanceTableRepository> mock_workedTable = new();
            Mock<IOvertimeWorkTableRepository> mock_overtime = new();
            Mock<IAttendanceRepository> mock_attendance = new();
            Mock<IWorkedRecordAgentRepository> mock_agent = new();

            IRecordManMonthUseCase useCase = new RecordManMonthUseCase(
                mock_conf.Object,
                mock_stream.Object,
                mock_emp.Object,
                mock_workedTable.Object,
                mock_overtime.Object,
                mock_attendance.Object,
                mock_agent.Object);

            var attendance = TestAttendanceParamFactory.Create();
            await useCase.ExecuteAsync(attendance, (message) => true);

            // then
            mock_stream.Verify(x => x.OpenOrCreate(It.IsAny<string>()), Times.Exactly(3));
            mock_workedTable.Verify(x => x.AddWokedDayAsync(It.IsAny<Stream>(), It.IsAny<Attendance>(), It.IsAny<Func<string, bool>>()), Times.Once());
            mock_overtime.Verify(x => x.AddAsync(It.IsAny<Stream>(), It.IsAny<Attendance>()), Times.Once());
            mock_attendance.Verify(x => x.AddAsync(It.IsAny<Attendance>()), Times.Once());
            mock_agent.Verify(x => x.AddAsync(It.IsAny<Stream>(), It.IsAny<Attendance>()), Times.Once());
        }

        [TestMethod()]
        public async Task 正常系_9時間勤務のときリポジトリが実行されること()
        {
            // given
            // when
            Mock<IConfiguration> mock_conf = new();
            mock_conf.Setup(x => x["applicationConfiguration:AttendanceTableDirectoryBase"])
                     .Returns(@"C:\debug");
            mock_conf.Setup(x => x["applicationConfiguration:OvertimeTableDirectoryBase"])
                     .Returns(@"C:\debug");
            mock_conf.Setup(x => x["applicationConfiguration:OvertimeTableTemplateDirectory"])
                     .Returns(@"C:\debug");
            mock_conf.Setup(x => x["applicationConfiguration:DailyAchievementTableBase"])
                     .Returns(@"C:\debug");

            // ダミーブック作成
            MemoryStream dummyBook = new();
            using (var xlBook = new XLWorkbook())
            {
                xlBook.AddWorksheet();
                xlBook.SaveAs(dummyBook);
            }

            Mock<IFileStreamOpener> mock_stream = new();
            mock_stream.Setup(x => x.OpenOrCreate(It.IsAny<string>()))
                .Returns(dummyBook);

            Mock<ManHourRecordService.IEmployeeRepository> mock_emp = new();
            mock_emp.Setup(x => x.FindByEmployeeNumberAsync(It.IsAny<uint>()))
                .ReturnsAsync(TestEmployeeFactory.Create());

            Mock<IAttendanceTableRepository> mock_workedTable = new();
            Mock<IOvertimeWorkTableRepository> mock_overtime = new();
            Mock<IAttendanceRepository> mock_attendance = new();
            Mock<IWorkedRecordAgentRepository> mock_agent = new();

            IRecordManMonthUseCase useCase = new RecordManMonthUseCase(
                mock_conf.Object,
                mock_stream.Object,
                mock_emp.Object,
                mock_workedTable.Object,
                mock_overtime.Object,
                mock_attendance.Object,
                mock_agent.Object);

            var attendance = TestAttendanceParamFactory.Create(
                achievements: new List<AchievementParam>
                {
                    TestAchievementParamFactory.Create(manHour: 3m),
                    TestAchievementParamFactory.Create(manHour: 3m),
                    TestAchievementParamFactory.Create(manHour: 3m),
                });
            await useCase.ExecuteAsync(attendance, (message) => true);

            // then
            mock_stream.Verify(x => x.OpenOrCreate(It.IsAny<string>()), Times.Exactly(3));
            mock_workedTable.Verify(x => x.AddWokedDayAsync(It.IsAny<Stream>(), It.IsAny<Attendance>(), It.IsAny<Func<string, bool>>()), Times.Once());
            mock_overtime.Verify(x => x.AddAsync(It.IsAny<Stream>(), It.IsAny<Attendance>()), Times.Once());
            mock_attendance.Verify(x => x.AddAsync(It.IsAny<Attendance>()), Times.Once());
            mock_agent.Verify(x => x.AddAsync(It.IsAny<Stream>(), It.IsAny<Attendance>()), Times.Once());
        }
    }
}