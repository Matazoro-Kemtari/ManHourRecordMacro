using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.ManHourRecordService.WorkingClassificationFetcher;

namespace Wada.SettingValidationRuleApplication.Tests
{
    [TestClass()]
    public class FetchWorkingClassificationListUseCaseTests
    {
        [TestMethod()]
        public async Task 正常系_ドメインサービスが呼ばれること()
        {
            // given
            // when
            Mock<IWorkingClassificationFetcher> mock_fetcher = new();
            mock_fetcher.Setup(x => x.FetchAsync(It.IsAny<IEnumerable<IEnumerable<object>>>(), It.IsAny<string>()))
                .ReturnsAsync(TestWorkingClassificationRecordFactory.Create());

            IFetchWorkingClassificationListUseCase useCase = new FetchWorkingClassificationListUseCase(mock_fetcher.Object);
            object[][] cellValues = new object[1][];
            _ = await useCase.ExecuteAsync(cellValues, "foo");

            // then
            mock_fetcher.Verify(x => x.FetchAsync(It.IsAny<IEnumerable<IEnumerable<object>>>(), It.IsAny<string>()), Times.Once);
        }
    }
}