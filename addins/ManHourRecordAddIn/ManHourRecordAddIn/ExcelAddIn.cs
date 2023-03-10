using ExcelDna.Integration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Wada.AttendanceSpreadSheet;
using Wada.Data.DesignDepartmentDataBse;
using Wada.Data.OrderManagement;
using Wada.IO;
using Wada.ManHourRecordFunctions;
using Wada.ManHourRecordService;
using Wada.ManHourRecordService.WorkingClassificationFetcher;
using Wada.OvertimeWorkTableSpreadSheet;
using Wada.RecordManHourApplication;
using Wada.SettingValidationRuleApplication;
using Wada.WorkedRecordAgentSpreadSheet;
using WorkingClassificationsTableSpreadSheet;

namespace ManHourRecordAddIn
{
    public class ExcelAddIn : IExcelAddIn
    {
        internal static IServiceCollection _container = new ServiceCollection();

        public void AutoClose()
        { }

        public void AutoOpen()
        {
            // DI 設定
            _ = _container.AddTransient<IConfiguration>(_ => MyConfigurationBuilder());
            // Logger
            _ = _container.AddSingleton<ILogger, Logger>(_ => LogManager.GetCurrentClassLogger());

            _ = _container.AddTransient<IAchievementInputSheet, AchievementInputSheet>();

            // 入力規則 所属
            _ = _container.AddTransient<IFetchDepartmentListUseCase, FetchDepartmentListUseCase>();
            _ = _container.AddTransient<IDepartmentFetcher, DepartmentFetcher>();

            // 項目分類表インポート
            _ = _container.AddTransient<IFetchWorkingClassificationsTableUseCase, FetchWorkingClassificationsTableUseCase>();
            _ = _container.AddTransient<IWorkingClassificationsTableRepository, WorkingClassificationsTableRepository>();

            // 入力規則 項目分類表
            _ = _container.AddTransient<IFetchWorkingClassificationListUseCase, FetchWorkingClassificationListUseCase>();
            _ = _container.AddTransient<IWorkingClassificationFetcher, WorkingClassificationFetcher>();

            // 登録
            _ = _container.AddTransient<IManHourRecordExistsUseCase, ManHourRecordExistsUseCase>();
            _ = _container.AddTransient<IRecordManMonthUseCase, RecordManMonthUseCase>();
            _ = _container.AddTransient<IFileStreamOpener, FileStreamOpener>();
            _ = _container.AddTransient<IEmployeeRepository, EmployeeRepository>();
            _ = _container.AddTransient<IWorkingLedgerRepository, WorkingLedgerRepository>();
            // 勤務表
            _ = _container.AddTransient<IAttendanceTableRepository, AttendanceTableRepository>();
            // 残業実績表
            _ = _container.AddTransient<IOwnCompanyHolidayRepository, OwnCompanyHolidayRepository>();
            _ = _container.AddTransient<IOvertimeWorkTableRepository, OvertimeWorkTableRepository>();
            // データベース
            _ = _container.AddTransient<IAttendanceRepository, AttendanceRepository>();
            // 日報
            _ = _container.AddTransient<IWorkedRecordAgentRepository, WorkedRecordAgentRepository>();
        }

        // 設定情報ライブラリを作る
        static IConfigurationRoot MyConfigurationBuilder()
        {
            DotNetEnv.Env.Load(".env");
            // NOTE: https://tech-blog.cloud-config.jp/2019-7-11-how-to-configuration-builder/
            return new ConfigurationBuilder()
                .SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(path: "appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
