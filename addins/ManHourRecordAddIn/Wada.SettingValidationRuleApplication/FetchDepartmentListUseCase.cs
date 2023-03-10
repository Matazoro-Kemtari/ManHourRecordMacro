using Wada.AOP.Logging;
using Wada.ManHourRecordService.WorkingClassificationFetcher;

namespace Wada.SettingValidationRuleApplication
{
    public interface IFetchDepartmentListUseCase
    {
        /// <summary>
        /// 所属一覧を取得する
        /// </summary>
        /// <param name="cellValues"></param>
        /// <returns></returns>
        Task<IEnumerable<string>> ExecuteAsync(IEnumerable<IEnumerable<object?>> cellValues);
    }

    public class FetchDepartmentListUseCase : IFetchDepartmentListUseCase
    {
        private readonly IDepartmentFetcher _departmentFetcher;

        public FetchDepartmentListUseCase(IDepartmentFetcher departmentFetcher)
        {
            _departmentFetcher = departmentFetcher;
        }

        [Logging]

        public Task<IEnumerable<string>> ExecuteAsync(IEnumerable<IEnumerable<object?>> cellValues)
            => Task.Run(() => _departmentFetcher.Fetch(cellValues));
    }
}