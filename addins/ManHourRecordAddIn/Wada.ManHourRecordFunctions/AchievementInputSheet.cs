using ExcelDna.Integration;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Wada.AOP.Logging;
using Wada.RecordManHourApplication;
using Wada.SettingValidationRuleApplication;
using Excel = Microsoft.Office.Interop.Excel;

namespace Wada.ManHourRecordFunctions
{
    public interface IAchievementInputSheet
    {
        /// <summary>
        /// 工数入力エクセルのシリアルを確認する
        /// </summary>
        /// <returns></returns>
        bool CheckManHourInputExcelSerial();

        /// <summary>
        /// 所属欄の入力規則を設定する
        /// </summary>
        Task SettingDepartmentValidationRuleAsync();

        /// <summary>
        /// 項目分類の名前の定義を設定する
        /// </summary>
        /// <param name="departmentName"></param>
        /// <returns></returns>
        Task SettingWorkingClassificationValidationRuleAsync();

        /// <summary>
        /// 工数を記録する
        /// </summary>
        /// <returns></returns>
        Task WriteManHourRecordAsync();
    }

    public class AchievementInputSheet : IAchievementInputSheet
    {
        private readonly Excel.Application _application;
        private readonly IConfiguration _configuration;
        private readonly IFetchWorkingClassificationsTableUseCase _fetchWorkingClassificationsTableUseCase;
        private readonly IFetchDepartmentListUseCase _fetchDepartmentListUseCase;
        private readonly IFetchWorkingClassificationListUseCase _fetchWorkingClassificationListUseCase;
        private readonly IRecordManMonthUseCase _recordManMonthUseCase;

        public AchievementInputSheet(IConfiguration configuration, IFetchWorkingClassificationsTableUseCase fetchWorkingClassificationsTableUseCase, IFetchDepartmentListUseCase fetchDepartmentListUseCase, IFetchWorkingClassificationListUseCase fetchWorkingClassificationListUseCase, IRecordManMonthUseCase recordManMonthUseCase)
        {
            _application = (Excel.Application)ExcelDnaUtil.Application;
            _configuration = configuration;
            _fetchWorkingClassificationsTableUseCase = fetchWorkingClassificationsTableUseCase;
            _fetchDepartmentListUseCase = fetchDepartmentListUseCase;
            _fetchWorkingClassificationListUseCase = fetchWorkingClassificationListUseCase;
            _recordManMonthUseCase = recordManMonthUseCase;
        }

        [Logging]
        public bool CheckManHourInputExcelSerial()
        {
            var validateSerial = _configuration["applicationConfiguration:ExecutableExcelBoolSerialId"]
                ?? throw new InvalidOperationException("設定情報が取得出来ません(ExecutableExcelBoolSerialId) システム担当まで連絡してください");
            Excel.Sheets sheets = _application.Worksheets;
            Excel.Worksheet sheet = null;
            for (int i = 0; i < sheets.Count; i++)
            {
                sheet = sheets[i + 1];
                if (sheet.Name == "設定")
                    break;
                else
                    sheet = null;
            }
            if (sheet == null)
                throw new InvalidOperationException("設定シートが取得出来ません システム担当まで連絡してください");

            // シリアルナンバー取得
            Excel.Range excelSerialRange = sheet.Cells[2, "B"];
            string excelSerial = excelSerialRange.Value;

            var result = validateSerial.Equals(excelSerial);

            if (!result)
                MessageBox.Show(
                    "お使いの工数入力エクセルファイルは古いマクロです\n" +
                    @"[ N:\04共通\開発・設計受渡し\工数記録システム ]" +
                    "から新しいファイルを受け取ってください",
                    "工数入力",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

            return result;
        }

        [Logging]
        public async Task SettingDepartmentValidationRuleAsync()
        {
            try
            {
                DroStop();

                IEnumerable<IEnumerable<object>> items = ImportWorkingClassifications();

                // 入力規則
                // 所属一覧取得
                var departments = await _fetchDepartmentListUseCase.ExecuteAsync(items);
                SettingDepartmentValidationRule(departments);

                // 項目分類表取得
                await SettingWorkingClassificationValidationRuleAsync();
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show(ex.Message, "工数入力", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"致命的なエラーが発生しました システム担当まで連絡してください\n{ex.Message}\n{ex}", "工数入力", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                DrowStart();
            }
        }

        [Logging]
        public async Task SettingWorkingClassificationValidationRuleAsync()
        {
            try
            {
                DroStop();

                Excel.Sheets sheets = _application.Worksheets;

                // シート項目分類表の値を取得

                Excel.Worksheet classSheet = sheets["項目分類表"];
                var usedRange = classSheet.UsedRange;
                object[,] usedValues = usedRange.Value;

                IEnumerable<IEnumerable<object>> items = ConvertTwoDimensionalArrayToJaggedArray(usedValues);

                // 所属取得
                Excel.Worksheet classInput = sheets["実績入力シート"];
                var departmentRange = classInput.Range["F2"];
                string departmentName = departmentRange.Value
                    ?? throw new NullReferenceException("所属は必須項目です");

                // 入力規則
                // 項目分類表取得
                var workingClasses = await _fetchWorkingClassificationListUseCase.ExecuteAsync(items, departmentName);
                SettingWorkingClassificationNameDefinition(workingClasses);
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show(ex.Message, "工数入力", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"致命的なエラーが発生しました システム担当まで連絡してください\n{ex.Message}\n{ex}", "工数入力", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                DrowStart();
            }
        }

        [Logging]
        public async Task WriteManHourRecordAsync()
        {
            try
            {
                DroStop();

                Excel.Sheets sheets = _application.Worksheets;

                Excel.Worksheet classInput = sheets["実績入力シート"];
                // 社員No
                var employeeNumberRange = classInput.Range["F1"];
                uint employeeNumber = default;
                if (employeeNumberRange.Value == null
                    || !uint.TryParse(employeeNumberRange.Value.ToString(), out employeeNumber))
                    throw new NullReferenceException("社員Noは必須項目です");

                // 実績日
                var achievementDateRange = classInput.Range["A3"];
                DateTime _achievementDate = new DateTime();
                if (achievementDateRange.Value == null
                    || !DateTime.TryParse(achievementDateRange.Value.ToString(), out _achievementDate))
                    throw new NullReferenceException("日付は必須項目です");
                AchievementDateParam achievementDate = new AchievementDateParam(_achievementDate);

                // 始業時間
                var startTimeRange = classInput.Range["B3"];
                if (!TimeSpan.TryParse(startTimeRange.Text, out TimeSpan startTime))
                    throw new NullReferenceException("始業は必須項目です");

                // 勤務
                var dayOffClassificationRange = classInput.Range["D3"];
                DayOffClassificationAttempt dayOffClassification = (dayOffClassificationRange.Value as string)?.ToDayOffClassificationAttempt()
                    ?? DayOffClassificationAttempt.None;

                // 所属
                var departmentRange = classInput.Range["F2"];
                string department = departmentRange.Value?.ToString()
                    ?? throw new NullReferenceException("所属は必須項目です");

                // 実績
                IEnumerable<AchievementParam> achievementParams = FetchAchievements(classInput);

                AttendanceParam attendance = new AttendanceParam(employeeNumber,
                                                 achievementDate,
                                                 startTime,
                                                 dayOffClassification,
                                                 department,
                                                 achievementParams);
                await _recordManMonthUseCase.ExecuteAsync(attendance, ConfirmAttendanceTableOverwriting);

                var result = MessageBox.Show("データを更新しました\n入力した内容をそのまま残しますか",
                                             "工数入力",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Question,
                                             MessageBoxResult.No);
                if (result != MessageBoxResult.Yes)
                    InitializeInputSheet(classInput);
            }
            catch (Exception ex) when (ex is NullReferenceException
                                       || ex is RecordManHourApplicationException
                                       || ex is OvertimeWorkTableEmployeeDoseNotFoundApplicationException)
            {
                MessageBox.Show(ex.Message, "工数入力", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"致命的なエラーが発生しました システム担当まで連絡してください\n" +
                    $"{ex.Message}\n{ex}", "工数入力", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                DrowStart();
            }
        }

        /// <summary>
        /// 項目分類表を取り込む
        /// </summary>
        /// <returns></returns>
        [Logging]
        private IEnumerable<IEnumerable<object>> ImportWorkingClassifications()
        {
            IEnumerable<IEnumerable<object>> classCells = _fetchWorkingClassificationsTableUseCase.Execute();

            // ジャグ配列を2次元配列にする
            var multiArray = ConvertJaggedArrayToTwoDimensionalArray(classCells);

            Excel.Sheets sheets = _application.Worksheets;
            Excel.Worksheet sheet = sheets["項目分類表"];
            var rows = sheet.Rows;
            rows.Clear();

            Excel.Range range = sheet.Range[sheet.Cells[1, 1], sheet.Cells[multiArray.GetLength(0), multiArray.GetLength(1)]];
            range.Value = multiArray;
            return classCells;
        }

        [Logging]
        private void InitializeInputSheet(Excel.Worksheet classInput)
        {
            Excel.Range date = classInput.Cells[3, "A"];
            date.ClearContents();
            Excel.Range workingClass = classInput.Cells[3, "D"];
            workingClass.ClearContents();
            Excel.Range detail = classInput.Range["A7:F30,H7:H30"];
            detail.ClearContents();
        }

        [Logging]
        private bool ConfirmAttendanceTableOverwriting(string message)
            => MessageBox.Show($"勤務表を上書きしてもよろしいですか?\n{message}",
                               "工数入力",
                               MessageBoxButton.YesNo,
                               MessageBoxImage.Question,
                               MessageBoxResult.No) == MessageBoxResult.Yes;

        private void DrowStart()
        {
            // 再計算を行う
            _application.Calculate();
            // 画面描画再開
            _application.EnableEvents = true;
            _application.ScreenUpdating = true;
            _application.DisplayAlerts = true;
            // 計算処理再開
            _application.Calculation = Excel.XlCalculation.xlCalculationAutomatic;
        }

        private void DroStop()
        {
            // 描画処理ストップ
            _application.ScreenUpdating = false;
            _application.EnableEvents = false;
            _application.DisplayAlerts = false;
            // 計算処理ストップ
            _application.Calculation = Excel.XlCalculation.xlCalculationManual;
            // 再計算を行う
            _application.Calculate();
        }

        [Logging]
        private static IEnumerable<AchievementParam> FetchAchievements(Excel.Worksheet classInput)
        {
            List<AchievementParam> result = new List<AchievementParam>();
            for (int i = 7; i < 31; i++)
            {
                Excel.Range manHourRange = classInput.Cells[i, "F"];
                decimal manHour = default;
                if (manHourRange.Value == null
                    || !decimal.TryParse(manHourRange.Value.ToString(), out manHour))
                    continue;

                Excel.Range workingNumberRange = classInput.Cells[i, "A"];
                string workingNumber = workingNumberRange.Value?.ToString()
                    ?? throw new NullReferenceException("作業Noは必須項目です");

                Excel.Range detRange = classInput.Cells[i, "B"];
                string det = detRange.Value?.ToString();

                Excel.Range processRange = classInput.Cells[i, "C"];
                string process = processRange.Value?.ToString()
                    ?? throw new NullReferenceException("実績工程は必須項目です");

                Excel.Range majorClassRange = classInput.Cells[i, "D"];
                string majorClass = majorClassRange.Value?.ToString()
                    ?? throw new NullReferenceException("大分類は必須項目です");

                Excel.Range middleClassRange = classInput.Cells[i, "E"];
                string middleClass = middleClassRange.Value?.ToString();

                Excel.Range noteRange = classInput.Cells[i, "H"];
                string note = noteRange.Value?.ToString();

                result.Add(new AchievementParam(workingNumber, det, process, majorClass, middleClass, manHour, note));
            }
            return result;
        }

        [Logging]
        private void SettingWorkingClassificationNameDefinition(WorkingClassificationDto workingClasses)
        {
            // 大分類リストの参照を設定する
            SettingMajorWorkingClassificationNameDefinition(workingClasses.MajorRange);

            // 中分類リストの参照を設定する
            SettingMiddleWorkingClassificationNameDefinition(workingClasses.MiddleClassification);
        }

        [Logging]
        private void SettingMiddleWorkingClassificationNameDefinition(Dictionary<string, ClassificationRangeDto> middleClassification)
        {
            Excel.Sheets sheets = _application.Worksheets;
            Excel.Worksheet InputSheet = sheets["実績入力シート"];
            Excel.Workbook book = InputSheet.Parent;
            var definitionNames = book.Names;
            // シート項目分類表を参照している定義は削除する
            foreach (Excel.Name nameDefinition in definitionNames)
            {
                System.Diagnostics.Debug.WriteLine(nameDefinition.Name);
                System.Diagnostics.Debug.WriteLine(nameDefinition.Visible);
                System.Diagnostics.Debug.WriteLine(nameDefinition.MacroType);
                System.Diagnostics.Debug.WriteLine(nameDefinition.Value);
                if (nameDefinition.Visible
                    && nameDefinition.Name != "大分類"
                    && Regex.IsMatch(nameDefinition.RefersTo, @"項目分類表\!\$?[A-Z]+\$?\d+:\$?[A-Z]+\$?\d+"))
                    nameDefinition.Delete();
            }
            middleClassification.ToList().ForEach(x =>
                SettingNameDefinition(x.Key, x.Value, definitionNames));
        }

        [Logging]
        private void SettingMajorWorkingClassificationNameDefinition(ClassificationRangeDto majorRange)
        {
            Excel.Sheets sheets = _application.Worksheets;
            Excel.Worksheet InputSheet = sheets["実績入力シート"];
            Excel.Workbook book = InputSheet.Parent;
            var definitionNames = book.Names;

            SettingNameDefinition("大分類", majorRange, definitionNames);
        }

        [Logging]
        private void SettingNameDefinition(string definableName, ClassificationRangeDto classesRange, Excel.Names definitionNames)
        {
            // 名前の定義があるか探す
            var hasMajorClassification = false;
            foreach (Excel.Name nameDefinition in definitionNames)
            {
                if (nameDefinition.Name == definableName)
                {
                    hasMajorClassification = true;
                    break;
                }
            }

            // 名前を定義する
            if (hasMajorClassification)
                definitionNames.Item(definableName).RefersTo = MakeMajorClassificationReference(classesRange);
            else
                definitionNames.Add(definableName, RefersTo: MakeMajorClassificationReference(classesRange));
        }

        [Logging]
        private string MakeMajorClassificationReference(ClassificationRangeDto majorRange)
        {
            Excel.Sheets sheets = _application.Worksheets;
            Excel.Worksheet sheet = sheets["項目分類表"];
            Excel.Range range = sheet.Range[sheet.Cells[majorRange.Bigen.Row + 1, majorRange.Bigen.Column + 1],
                                            sheet.Cells[majorRange.Finish.Row + 1, majorRange.Finish.Column + 1]];
            return $"={range.Address[External: true]}";
        }

        [Logging]
        private static bool DepartmentExistsToList(IEnumerable<string> departments, object value)
            => departments.Any(x => x == value?.ToString());

        [Logging]
        private void SettingDepartmentValidationRule(IEnumerable<string> departments)
        {
            Excel.Sheets sheets = _application.Worksheets;
            Excel.Worksheet InputSheet = sheets["実績入力シート"];
            var departmentRange = InputSheet.Range["F2"];
            departmentRange.Validation.Delete();
            departmentRange.Validation.Add(
                Type: Excel.XlDVType.xlValidateList,
                Formula1: string.Join(",", departments));
        }

        /// <summary>
        /// 2次元配列をジャグ配列に変換
        /// </summary>
        /// <param name="dimensionValues"></param>
        /// <returns></returns>
        [Logging]
        private static IEnumerable<IEnumerable<object>> ConvertTwoDimensionalArrayToJaggedArray(object[,] dimensionValues)
        {
            // 2次元配列をジャグ配列に変換
            // REFERENCE: https://qiita.com/tricogimmick/items/de52dc6a166aebc33b5d のコメント欄
            // Rangeの2次元配列の添え字は1からスタート
            var rows = Enumerable.Range(1, dimensionValues.GetLength(0));
            var columns = Enumerable.Range(1, dimensionValues.GetLength(1));
            // この添え字はLinqの機能で0スタート
            var items = rows.Select(r => columns.Select(c => dimensionValues[r, c]));
            return items;
        }

        [Logging]
        private static object[,] ConvertJaggedArrayToTwoDimensionalArray(IEnumerable<IEnumerable<object>> jaggedValues)
        {
            var rowCount = jaggedValues.Count();
            var columnCount = jaggedValues.Select(x => x.Count()).First();
            object[,] result = new object[rowCount, columnCount];
            jaggedValues.Select((x, i) =>
            new
            {
                rowIndex = i,
                rowValues = x.Select((y, j) =>
                new
                {
                    cellIndex = j,
                    cellValue = y
                })
            }).ToList().ForEach(
                x => x.rowValues.ToList().ForEach(
                    y => result[x.rowIndex, y.cellIndex] = y.cellValue));
            return result;
        }
    }
}
