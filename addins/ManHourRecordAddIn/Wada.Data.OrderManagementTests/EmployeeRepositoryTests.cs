using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Wada.ManHourRecordService;
using Wada.ManHourRecordService.EmployeeAggregation;

namespace Wada.Data.OrderManagement.Tests
{
    [TestClass()]
    public class EmployeeRepositoryTests
    {
        [TestMethod()]
        public async Task 正常系_社員情報が取得できること()
        {
            // given
            // when
            IEmployeeRepository employeeRepository = new EmployeeRepository();
            var employee = await employeeRepository.FindByEmployeeNumberAsync(4001u);

            // then
            Assert.IsNotNull(employee);
            Assert.AreEqual("本社　無人", employee.Name);
            Assert.AreEqual(4, employee.DepartmentId);
        }

        [TestMethod()]
        public async Task 異常系_該当社員がいないとき例外を返すこと()
        {
            // given
            // when
            IEmployeeRepository employeeRepository = new EmployeeRepository();
            Task target()
                => employeeRepository.FindByEmployeeNumberAsync(0u);

            // then
            var ex = await Assert.ThrowsExceptionAsync<EmployeeAggregationException>(target);
            var expected = "社員番号を確認してください 受注管理に登録されていません 社員番号: 0";
            Assert.AreEqual(expected, ex.Message);
        }
    }
}