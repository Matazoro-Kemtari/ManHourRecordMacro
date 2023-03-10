using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.Data.OrderManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wada.ManHourRecordService;

namespace Wada.Data.OrderManagement.Tests
{
    [TestClass()]
    public class EmployeeRepositoryTests
    {
        [TestMethod()]
        public async Task FindAllAsyncTest()
        {
            // given
            // when
            IEmployeeRepository repository = new EmployeeRepository();
            var actual = await repository.FindAllAsync();
            
            // then
            Assert.AreEqual(5, actual.Count());
        }
    }
}