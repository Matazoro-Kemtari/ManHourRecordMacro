using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Wada.ManHourRecordService.WorkingClassificationFetcher;

namespace Wada.SettingValidationRuleApplication.Tests
{
    [TestClass()]
    public class FetchDepartmentListUseCaseTests
    {
        [TestMethod()]
        public async Task 正常系_ドメインサービスが呼ばれること()
        {
            // given
            // when
            Mock<IDepartmentFetcher> mock_fetcher = new();
            var expected = new string[]
            {
                "A部署","B部署","C部署","D部署","E部署",
            };
            mock_fetcher.Setup(x=>x.Fetch(It.IsAny<IEnumerable<IEnumerable<object?>>>()))
                .Returns(expected);

            IFetchDepartmentListUseCase useCase = new FetchDepartmentListUseCase(mock_fetcher.Object);
            object?[][] cellValues = new object?[5][];
            _ = await useCase.ExecuteAsync(cellValues);

            // then
            mock_fetcher.Verify(x => x.Fetch(It.IsAny<IEnumerable<IEnumerable<object?>>>()), Times.Once);
        }
    }
}