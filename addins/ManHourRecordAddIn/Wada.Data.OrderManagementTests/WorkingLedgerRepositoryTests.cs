using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Wada.ManHourRecordService;
using Wada.ManHourRecordService.ValueObjects;

namespace Wada.Data.OrderManagement.Tests
{
    [TestClass()]
    public class WorkingLedgerRepositoryTests
    {
        [DataTestMethod()]
        [DataRow("02V-111", "12C")]
        [DataRow("02V-782", null)]
        public async Task 正常系_作業台帳が取得できること(string workingNumber, string expected)
        {
            // given
            // when
            IWorkingLedgerRepository repository = new WorkingLedgerRepository();
            var workingLedger = await repository.FindByWorkingNumberAsync(new WorkingNumber(workingNumber));

            // then
            Assert.IsNotNull(workingLedger);
            Assert.AreEqual(expected, workingLedger.JigCode);
        }
    }
}