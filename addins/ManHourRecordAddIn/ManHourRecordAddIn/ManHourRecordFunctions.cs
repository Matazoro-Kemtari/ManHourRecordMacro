using ExcelDna.Integration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Wada.AOP.Logging;
using Wada.ManHourRecordFunctions;

namespace ManHourRecordAddIn
{
    public class ManHourRecordFunctions
    {
        [ExcelFunction(Description = "工数入力エクセルのシリアルを確認する")]
        [Logging]
        public static bool CheckManHourInputExcelSerial()
        {
            using (var provider = ExcelAddIn._container.BuildServiceProvider())
            {
                var achievementInputSheet = provider.GetService<IAchievementInputSheet>()
                    ?? throw new InvalidOperationException(
                        $"{nameof(IAchievementInputSheet)}が取得できませんでした");

                return achievementInputSheet.CheckManHourInputExcelSerial();
            }
        }

        [ExcelCommand(Description = "所属の入力規則を設定する")]
        [Logging]
        public static void SettingDepartmentValidationRule()
        {
            using (var provider = ExcelAddIn._container.BuildServiceProvider())
            {
                var achievementInputSheet = provider.GetService<IAchievementInputSheet>()
                    ?? throw new InvalidOperationException(
                        $"{nameof(IAchievementInputSheet)}が取得できませんでした");
                
                achievementInputSheet.SettingDepartmentValidationRuleAsync().Wait();
            }
        }

        [ExcelCommand(Description = "分類の入力規則を設定する")]
        [Logging]
        public static void SettingWorkingClassification()
        {
            using (var provider = ExcelAddIn._container.BuildServiceProvider())
            {
                var achievementInputSheet = provider.GetService<IAchievementInputSheet>()
                    ?? throw new InvalidOperationException(
                        $"{nameof(IAchievementInputSheet)}が取得できませんでした");

                achievementInputSheet.SettingWorkingClassificationValidationRuleAsync().Wait();
            }
        }

        [ExcelFunction(Description = "入力漏れをチェックする")]
        [Logging]
        public static bool CheckExistsInputLeak()
        {
            throw new NotImplementedException();
        }

        [ExcelCommand(Description = "入力漏れを補う")]
        [Logging]
        public static void CompensateInputLeak()
        {
            throw new NotImplementedException();
        }

        [ExcelCommand(Description = "実績を登録する")]
        [Logging]
        public static void WriteAttendance()
        {
            using (var provider = ExcelAddIn._container.BuildServiceProvider())
            {
                var achievementInputSheet = provider.GetService<IAchievementInputSheet>()
                    ?? throw new InvalidOperationException(
                        $"{nameof(IAchievementInputSheet)}が取得できませんでした");
                ExcelAsyncUtil.QueueAsMacro(
                    () => achievementInputSheet.WriteManHourRecordAsync());
            }
        }

        [ExcelCommand(Description = "履歴を取得する")]
        [Logging]
        public static void FetchAttendance()
        {
            using (var provider = ExcelAddIn._container.BuildServiceProvider())
            {
                var achievementInputSheet = provider.GetService<IAchievementInputSheet>()
                    ?? throw new InvalidOperationException(
                        $"{nameof(IAchievementInputSheet)}が取得できませんでした");
                ExcelAsyncUtil.QueueAsMacro(
                    () => achievementInputSheet.FetchManHourRecordAsync());
            }
        }

        [ExcelCommand(Description = "入力クリアする")]
        [Logging]
        public static void ClearAttendance()
        {
            using (var provider = ExcelAddIn._container.BuildServiceProvider())
            {
                var achievementInputSheet = provider.GetService<IAchievementInputSheet>()
                    ?? throw new InvalidOperationException(
                        $"{nameof(IAchievementInputSheet)}が取得できませんでした");
                achievementInputSheet.ClearInputedManHour();
            }
        }
    }
}
