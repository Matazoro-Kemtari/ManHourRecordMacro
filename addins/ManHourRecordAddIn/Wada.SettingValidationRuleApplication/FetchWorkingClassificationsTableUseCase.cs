using Microsoft.Extensions.Configuration;
using Wada.AOP.Logging;
using Wada.ManHourRecordService;

namespace Wada.SettingValidationRuleApplication
{
    public interface IFetchWorkingClassificationsTableUseCase
    {
        /// <summary>
        /// 項目分類表を取り込む
        /// </summary>
        IEnumerable<IEnumerable<object?>> Execute();
    }

    public class FetchWorkingClassificationsTableUseCase : IFetchWorkingClassificationsTableUseCase
    {
        private readonly IConfiguration _configuration;
        private readonly IFileStreamOpener _streamOpener;
        private readonly IWorkingClassificationsTableRepository _workingClassificationsTableRepository;

        public FetchWorkingClassificationsTableUseCase(IConfiguration configuration, IFileStreamOpener streamOpener, IWorkingClassificationsTableRepository workingClassificationsTableRepository)
        {
            _configuration = configuration;
            _streamOpener = streamOpener;
            _workingClassificationsTableRepository = workingClassificationsTableRepository;
        }

        [Logging]
        public IEnumerable<IEnumerable<object?>> Execute()
        {
            // 設定ファイルから項目分類表パスを取得
            var workingClassTablePath = _configuration["applicationConfiguration:WorkingClassificationTablePath"]
                ?? throw new InvalidOperationException("設定情報が取得出来ません(WorkingClassificationTablePath) システム担当まで連絡してください");
            
            if (!File.Exists(workingClassTablePath))
                throw new InvalidOperationException("項目分類表ファイルが見つかりません システム担当まで連絡してください");

            var fileStream = _streamOpener.OpenOrCreate(workingClassTablePath);
            return _workingClassificationsTableRepository.FetchAll(fileStream);
        }
    }
}
