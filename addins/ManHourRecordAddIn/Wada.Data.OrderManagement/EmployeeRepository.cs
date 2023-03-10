using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Wada.AOP.Logging;
using Wada.Data.OrderManagement.Entities;
using Wada.ManHourRecordService;
using Wada.ManHourRecordService.EmployeeAggregation;

namespace Wada.Data.OrderManagement
{

    public class EmployeeRepository : IEmployeeRepository
    {
        [Logging]
        public async Task<IEnumerable<Employee>> FindAllAsync()
        {
            using (var dbContext = new OrderManagementEntities())
            {
                var employees = await dbContext.S社員
                    .ToListAsync();
                return employees.Select(x => new Employee((uint)x.社員NO, x.氏名, x.部署ID));

            }
        }

        [Logging]
        public async Task<Employee> FindByEmployeeNumberAsync(uint employeeNumber)
        {
            using (var dbContext = new OrderManagementEntities())
            {
                try
                {
                    var employee = await dbContext.S社員.Where(x => x.社員NO == employeeNumber)
                                                        .FirstAsync();
                    return new Employee((uint)employee.社員NO, employee.氏名, employee.部署ID);
                }
                catch (InvalidOperationException ex)
                {
                    throw new EmployeeAggregationException(
                        $"社員番号を確認してください 受注管理に登録されていません 社員番号: {employeeNumber}", ex);
                }
            }
        }
    }
}