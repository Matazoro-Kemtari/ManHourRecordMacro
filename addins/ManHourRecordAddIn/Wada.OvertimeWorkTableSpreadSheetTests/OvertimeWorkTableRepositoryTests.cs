using ClosedXML.Excel;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Reflection;
using System.Text.RegularExpressions;
using Wada.ManHourRecordService;
using Wada.ManHourRecordService.AttendanceAggregation;
using Wada.ManHourRecordService.EmployeeAggregation;
using Wada.ManHourRecordService.OvertimeWorkTableCreator;
using Wada.ManHourRecordService.OwnCompanyCalendarAggregation;
using Wada.ManHourRecordService.ValueObjects;

namespace Wada.OvertimeWorkTableSpreadSheet.Tests
{
    [TestClass()]
    public class OvertimeWorkTableRepositoryTests
    {
        [TestMethod()]
        public void 正常系_残業実績エクセルの存在が確認できること()
        {
            // given
            // 残業実績エクセルを作っておく
            const string department = "総務部";
            const string testDataFileName = $"残業実績表({department}).xlsx";
            int testYear = 2023;
            int fiscalYear = 2022;
            int testMonth = 1;
            var overtimeTablePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                $"{fiscalYear}年度",
                $"{testYear}年{testMonth:00}月",
                testDataFileName);
            FileInfo fileInfo = new(overtimeTablePath);
            if (!fileInfo.Directory?.Exists ?? false)
                fileInfo.Directory?.Create();
            if (fileInfo.Exists)
                fileInfo.Delete();

            // アセンブリに埋め込まれているリソースを取得する
            var assembly = Assembly.GetExecutingAssembly();
            const string testDataName = "Wada.OvertimeWorkTableSpreadSheetTests.Resources.残業実績表(総務部).xlsx";
            using (var resurceStream = assembly.GetManifestResourceStream(testDataName))
            {
                using var write = File.Create(overtimeTablePath);
                resurceStream?.CopyTo(write);
            }

            // when
            Mock<IConfiguration> mock_conf = new();
            mock_conf.Setup(x => x["applicationConfiguration:OvertimeTableDirectoryBase"])
                     .Returns(AppDomain.CurrentDomain.BaseDirectory);

            Mock<IOwnCompanyHolidayRepository> mock_holiday = new();
            Mock<IEmployeeRepository> mock_employee = new();

            IOvertimeWorkTableRepository repository =
                new OvertimeWorkTableRepository(mock_conf.Object, mock_holiday.Object, mock_employee.Object);

            OvertimeWorkTableOwner overtimeWork = new(new(testYear, testMonth, 20), department);
            var actual = repository.OvertimeTableExists(overtimeWork);

            // then
            mock_conf.Verify(x => x["applicationConfiguration:OvertimeTableDirectoryBase"], Times.Once());
            Assert.IsTrue(actual);

            mock_holiday.Verify(x => x.FindByYearMonthAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
            mock_employee.Verify(x => x.FindAllAsync(), Times.Never());
            mock_employee.Verify(x => x.FindByEmployeeNumberAsync(It.IsAny<uint>()), Times.Never());

            fileInfo.Directory?.Delete(true);
        }

        [TestMethod()]
        public async Task 正常系_残業実績エクセルが作成されること()
        {
            // given
            // 残業実績エクセルが無い状態にする
            const string department = "総務部";
            const string testDataFileName = $"残業実績表({department}).xlsx";
            int testYear = 2023;
            int fiscalYear = 2022;
            int testMonth = 1;
            var overtimeTablePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                $"{fiscalYear}年度",
                $"{testYear}年{testMonth:00}月",
                testDataFileName);
            FileInfo fileInfo = new(overtimeTablePath);
            if (fileInfo.Exists)
                fileInfo.Delete();

            // テンプレートファイルを作る
            var templatePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Template",
                testDataFileName);
            FileInfo templateFileInfo = new FileInfo(templatePath);
            if (templateFileInfo.Exists)
                templateFileInfo.Delete();

            // アセンブリに埋め込まれているリソースを取得する
            var assembly = Assembly.GetExecutingAssembly();
            const string testDataName = "Wada.OvertimeWorkTableSpreadSheetTests.Resources.残業実績表(総務部).xlsx";
            using (var resurceStream = assembly.GetManifestResourceStream(testDataName))
            {
                if (templateFileInfo.Directory != null && !templateFileInfo.Directory.Exists)
                    templateFileInfo.Directory.Create();

                using var write = templateFileInfo.Create();
                resurceStream?.CopyTo(write);
            }

            // when
            Mock<IConfiguration> mock_conf = new();
            mock_conf.Setup(x => x["applicationConfiguration:OvertimeTableTemplateDirectory"])
                     .Returns(templateFileInfo.DirectoryName);
            mock_conf.Setup(x => x["applicationConfiguration:OvertimeTableDirectoryBase"])
                     .Returns(AppDomain.CurrentDomain.BaseDirectory);

            Mock<IOwnCompanyHolidayRepository> mock_holiday = new();
            mock_holiday.Setup(x => x.FindByYearMonthAsync(testYear, testMonth))
                .ReturnsAsync(new OwnCompanyHoliday[]
                {
                    OwnCompanyHoliday.Reconstruct(new(2023, 1, 1), HolidayClassification.LegalHoliday),
                    OwnCompanyHoliday.Reconstruct(new(2023, 1, 2), HolidayClassification.RegularHoliday),
                    OwnCompanyHoliday.Reconstruct(new(2023, 1, 9), HolidayClassification.RegularHoliday),
                });

            Mock<IEmployeeRepository> mock_employee = new();

            IOvertimeWorkTableRepository repository =
                new OvertimeWorkTableRepository(mock_conf.Object, mock_holiday.Object, mock_employee.Object);

            OvertimeWorkTableOwner overtimeWork = new(new(testYear, testMonth, 20), department);
            await repository.CreateAsync(overtimeWork);

            // then
            fileInfo.Refresh();
            Assert.IsTrue(fileInfo.Exists);
            mock_holiday.Verify(x => x.FindByYearMonthAsync(testYear, testMonth), Times.Once);
            using (var xlBook = new XLWorkbook(fileInfo.FullName))
            {
                xlBook.Worksheets
                    .Where(x => !Regex.IsMatch(x.Name, @"(一覧|三六|祝日)"))
                    .ToList()
                    .ForEach(x =>
                    {
                        Assert.AreEqual(testYear, x.Cell("W2").GetValue<int>(), $"年の検証 シート:{x.Name}");
                        Assert.AreEqual(testMonth, x.Cell("Z2").GetValue<int>(), $"月の検証 シート:{x.Name}");
                    });

                var sheet = xlBook.Worksheet("祝日");
                Assert.IsTrue(sheet.Columns("B").CellsUsed().Skip(7)
                    .Where(x => !x.IsEmpty())
                    .Where(x => x.GetDateTime() == new DateTime(2023, 1, 1))
                    .Count() == 1);
                Assert.IsTrue(sheet.Columns("B").CellsUsed().Skip(7)
                    .Where(x => !x.IsEmpty())
                    .Where(x => x.GetDateTime() == new DateTime(2023, 1, 2))
                    .Count() == 1);
                Assert.IsTrue(sheet.Columns("B").CellsUsed().Skip(7)
                    .Where(x => !x.IsEmpty())
                    .Where(x => x.GetDateTime() == new DateTime(2023, 1, 9))
                    .Count() == 1);
            }

            fileInfo.Delete();
            templateFileInfo.Refresh();
            templateFileInfo.Directory?.Delete(true);
        }

        [TestMethod()]
        public async Task 異常系_テンプレートに祝日シートがないとき例外を返すこと()
        {
            // given
            // 残業実績エクセルが無い状態にする
            const string department = "総務部";
            const string testDataFileName = $"残業実績表({department}).xlsx";
            int testYear = 2023;
            int fiscalYear = 2022;
            int testMonth = 1;
            var overtimeTablePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                $"{fiscalYear}年度",
                $"{testYear}年{testMonth:00}月",
                testDataFileName);
            FileInfo fileInfo = new(overtimeTablePath);
            if (fileInfo.Exists)
                fileInfo.Delete();

            // テンプレートファイルを作る
            var templatePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Template",
                testDataFileName);
            FileInfo templateFileInfo = new FileInfo(templatePath);
            if (templateFileInfo.Exists)
                templateFileInfo.Delete();

            // アセンブリに埋め込まれているリソースを取得する
            var assembly = Assembly.GetExecutingAssembly();
            const string testDataName = "Wada.OvertimeWorkTableSpreadSheetTests.Resources.残業実績表(総務部).xlsx";
            using (var resurceStream = assembly.GetManifestResourceStream(testDataName))
            {
                if (templateFileInfo.Directory != null && !templateFileInfo.Directory.Exists)
                    templateFileInfo.Directory.Create();

                using var write = templateFileInfo.Create();
                resurceStream?.CopyTo(write);

                using (var xlResurce = new XLWorkbook(write))
                {
                    xlResurce.Worksheet("祝日").Delete();
                    xlResurce.Save();
                }
            }

            // when
            Mock<IConfiguration> mock_conf = new();
            mock_conf.Setup(x => x["applicationConfiguration:OvertimeTableTemplateDirectory"])
                     .Returns(templateFileInfo.DirectoryName);
            mock_conf.Setup(x => x["applicationConfiguration:OvertimeTableDirectoryBase"])
                     .Returns(AppDomain.CurrentDomain.BaseDirectory);

            Mock<IOwnCompanyHolidayRepository> mock_holiday = new();
            Mock<IEmployeeRepository> mock_employee = new();

            IOvertimeWorkTableRepository repository =
                new OvertimeWorkTableRepository(mock_conf.Object, mock_holiday.Object, mock_employee.Object);

            OvertimeWorkTableOwner overtimeWork = new(new(testYear, testMonth, 20), department);
            Task target() => repository.CreateAsync(overtimeWork);

            // then
            var ex = await Assert.ThrowsExceptionAsync<OvertimeWorkTableCreatorException>(target);
            Assert.AreEqual($"テンプレートが壊れています 確認してください 部署: {department}",
                            ex.Message);

            fileInfo.Delete();
            templateFileInfo.Refresh();
            templateFileInfo.Directory?.Delete(true);
        }

        [DataTestMethod()]
        [DataRow(20, 0)]
        [DataRow(21, 6)]
        [DataRow(22, 6)]
        public async Task 正常系_残業実績が追加されること(int day, DayOffClassification dayOffClassification)
        {
            // given
            // when
            Mock<IConfiguration> mock_conf = new();

            Mock<IOwnCompanyHolidayRepository> mock_holiday = new();
            int testYear = 2023;
            int testMonth = 1;
            mock_holiday.Setup(x => x.FindByYearMonthAsync(testYear, testMonth))
                .ReturnsAsync(new List<OwnCompanyHoliday>
                {
                    OwnCompanyHoliday.Reconstruct(new(testYear, testMonth, 21), HolidayClassification.RegularHoliday),
                    OwnCompanyHoliday.Reconstruct(new(testYear, testMonth, 22), HolidayClassification.LegalHoliday),
                });

            Mock<IEmployeeRepository> mock_employee = new();
            mock_employee.Setup(x => x.FindByEmployeeNumberAsync(It.IsAny<uint>()))
                .ReturnsAsync(TestEmployeeFactory.Create());

            IOvertimeWorkTableRepository repository =
                new OvertimeWorkTableRepository(mock_conf.Object, mock_holiday.Object, mock_employee.Object);

            var memoryStream = new MemoryStream();
            // アセンブリに埋め込まれているリソースを取得する
            var assembly = Assembly.GetExecutingAssembly();
            const string testDataName = "Wada.OvertimeWorkTableSpreadSheetTests.Resources.残業実績表(総務部).xlsx";
            using (var resurceStream = assembly.GetManifestResourceStream(testDataName))
            {
                resurceStream?.CopyTo(memoryStream);
            }

            // テスト実績作成 10.5時間分
            var testAttendance = TestAttendanceFactory.Create(
                employeeNumber: 4001u,
                achievementDate: new AchievementDate(new(testYear, testMonth, day)),
                dayOffClassification: dayOffClassification,
                achievements: new List<Achievement>
                {
                    TestAchievementFactory.Create(workingNumber: new WorkingNumber("99Z-001")),
                    TestAchievementFactory.Create(workingNumber: new WorkingNumber("99Z-002")),
                    TestAchievementFactory.Create(workingNumber: new WorkingNumber("99Z-003")),
                    TestAchievementFactory.Create(workingNumber: new WorkingNumber("99Z-004")),
                    TestAchievementFactory.Create(workingNumber: new WorkingNumber("99Z-006"), manHour: 1m),
                    TestAchievementFactory.Create(workingNumber: new WorkingNumber("99Z-005"), manHour: 1m),
                    TestAchievementFactory.Create(workingNumber: new WorkingNumber("99Z-007"), manHour: 0.5m),
                    TestAchievementFactory.Create(workingNumber: new WorkingNumber("99Z-008"), manHour: 0.5m),
                    TestAchievementFactory.Create(workingNumber: new WorkingNumber("99Z-008"), manHour: 2.5m),
                });
            await repository.AddAsync(memoryStream, testAttendance);

            // then
            using var xlBook = new XLWorkbook(memoryStream);
            decimal overtime;
            if (dayOffClassification == DayOffClassification.None)
                overtime = testAttendance.Achievements.Sum(x => x.ManHour) - 8m;
            else
                overtime = testAttendance.Achievements.Sum(x => x.ManHour);
            var sheet = xlBook.Worksheet("生産計画");
            Assert.AreEqual(overtime, sheet.Cell(11, day + 4).GetValue<decimal>());
        }

        [DataTestMethod]
        [DataRow(9, 1, null)]
        [DataRow(9, 2, 4)]
        [DataRow(9, 3, 4)]
        [DataRow(9, 4, null)]
        [DataRow(9, 9, null)]
        public async Task 正常系_勤務区分がある実績が登録できること(int day, DayOffClassification dayOffClassification, int? manHour)
        {
            // given
            // when
            Mock<IConfiguration> mock_conf = new();

            Mock<IOwnCompanyHolidayRepository> mock_holiday = new();
            int testYear = 2023;
            int testMonth = 1;
            mock_holiday.Setup(x => x.FindByYearMonthAsync(testYear, testMonth))
                .ReturnsAsync(new List<OwnCompanyHoliday>{ });

            Mock<IEmployeeRepository> mock_employee = new();
            mock_employee.Setup(x => x.FindByEmployeeNumberAsync(It.IsAny<uint>()))
                .ReturnsAsync(TestEmployeeFactory.Create());

            IOvertimeWorkTableRepository repository =
                new OvertimeWorkTableRepository(mock_conf.Object, mock_holiday.Object, mock_employee.Object);

            var memoryStream = new MemoryStream();
            // アセンブリに埋め込まれているリソースを取得する
            var assembly = Assembly.GetExecutingAssembly();
            const string testDataName = "Wada.OvertimeWorkTableSpreadSheetTests.Resources.残業実績表(総務部).xlsx";
            using (var resurceStream = assembly.GetManifestResourceStream(testDataName))
            {
                resurceStream?.CopyTo(memoryStream);
            }

            // テスト実績作成
            List<Achievement> achievements = new();
            if (manHour != null)
                achievements= new List<Achievement>
                {
                    TestAchievementFactory.Create(workingNumber: new WorkingNumber("99Z-006"), manHour: (decimal)manHour),
                };

            var testAttendance = TestAttendanceFactory.Create(
                employeeNumber: 4001u,
                achievementDate: new AchievementDate(new(testYear, testMonth, day)),
                dayOffClassification: dayOffClassification,
                achievements: achievements);
            await repository.AddAsync(memoryStream, testAttendance);

            // then
            using var xlBook = new XLWorkbook(memoryStream);
            var sheet = xlBook.Worksheet("生産計画");
            Assert.IsTrue(sheet.Cell(11, day + 4).IsEmpty());
        }
    }
}