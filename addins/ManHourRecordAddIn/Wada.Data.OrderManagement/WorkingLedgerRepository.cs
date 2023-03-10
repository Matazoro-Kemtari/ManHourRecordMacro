using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Wada.AOP.Logging;
using Wada.Data.OrderManagement.Entities;
using Wada.ManHourRecordService;
using Wada.ManHourRecordService.ValueObjects;
using Wada.ManHourRecordService.WorkingLedgerAggregation;
using Wada.Wada.ManHourRecordService.WorkingLedgerAggregation;

namespace Wada.Data.OrderManagement
{

    public class WorkingLedgerRepository : IWorkingLedgerRepository
    {
        [Logging]
        public async Task<WorkingLedger> FindByWorkingNumberAsync(WorkingNumber workingNumber)
        {
            using (var dbContext = new OrderManagementEntities())
            {
                try
                {
                    var _workingNumber = workingNumber.ToString();
                    var workingLedger = await dbContext.M作業台帳
                        .Where(x => x.作業NO == _workingNumber)
                        .FirstAsync();
                    return new WorkingLedger((uint)workingLedger.自社NO,
                                             new WorkingNumber(workingLedger.作業NO),
                                             workingLedger.コード);

                }
                catch (InvalidOperationException ex)
                {
                    throw new WorkingLedgerAggregationException(
                        $"作業Noを確認してください 受注管理に登録されていません 作業No: {workingNumber}", ex);
                }
            }
        }
    }
}