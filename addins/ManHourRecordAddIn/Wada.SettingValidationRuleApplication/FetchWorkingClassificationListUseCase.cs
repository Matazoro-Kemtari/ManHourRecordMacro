using Wada.AOP.Logging;
using Wada.ManHourRecordService.WorkingClassificationFetcher;

namespace Wada.SettingValidationRuleApplication
{
    public interface IFetchWorkingClassificationListUseCase
    {
        /// <summary>
        /// 項目分類から大・中分類を取得する
        /// </summary>
        /// <param name="cellValues"></param>
        /// <param name="departmentName"></param>
        /// <returns></returns>
        Task<WorkingClassificationDto> ExecuteAsync(IEnumerable<IEnumerable<object>> cellValues, string departmentName);
    }

    public class FetchWorkingClassificationListUseCase : IFetchWorkingClassificationListUseCase
    {
        private readonly IWorkingClassificationFetcher _workingClassificationFetcher;

        public FetchWorkingClassificationListUseCase(IWorkingClassificationFetcher workingClassificationFetcher)
        {
            _workingClassificationFetcher = workingClassificationFetcher;
        }

        [Logging]
        public async Task<WorkingClassificationDto> ExecuteAsync(IEnumerable<IEnumerable<object>> cellValues, string departmentName)
        {
            // 項目分類を取得する
            var workingClass = await _workingClassificationFetcher.FetchAsync(cellValues, departmentName);

            return WorkingClassificationDto.Parse(workingClass);
        }
    }

    public record class WorkingClassificationDto(
        ClassificationRangeDto MajorRange,
        Dictionary<string, ClassificationRangeDto> MiddleClassification)
    {
        internal static WorkingClassificationDto Parse(WorkingClassificationRecord workingClass)
            => new(ClassificationRangeDto.Parse(workingClass.MajorRange),
                   workingClass.MiddleClassification
                               .Select(x => new { x.Key, Value = ClassificationRangeDto.Parse(x.Value) })
                               .ToDictionary(x => x.Key, x => x.Value));

        public ClassificationRangeDto MajorRange { get; } = MajorRange;

        public Dictionary<string, ClassificationRangeDto> MiddleClassification { get; } = MiddleClassification;
    }

    public record class ClassificationRangeDto(ClassificationPositionDto Bigen, ClassificationPositionDto Finish)
    {
        internal static ClassificationRangeDto Parse(ClassificationRangeRecord classRange)
            => new(ClassificationPositionDto.Parse(classRange.Bigen),
                   ClassificationPositionDto.Parse(classRange.Finish));

        public ClassificationPositionDto Bigen { get; } = Bigen;

        public ClassificationPositionDto Finish { get; } = Finish;
    }

    public record class ClassificationPositionDto(int Row, int Column)
    {
        internal static ClassificationPositionDto Parse(ClassificationPositionRecord position)
            => new(position.Row, position.Column);

        public int Row { get; } = Row;

        public int Column { get; } = Column;
    }
}
