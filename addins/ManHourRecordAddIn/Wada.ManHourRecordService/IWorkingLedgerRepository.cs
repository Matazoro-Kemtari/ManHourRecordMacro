using Wada.ManHourRecordService.ValueObjects;
using Wada.ManHourRecordService.WorkingLedgerAggregation;

namespace Wada.ManHourRecordService;

public interface IWorkingLedgerRepository
{
    Task<WorkingLedger> FindByWorkingNumberAsync(WorkingNumber workingNumber);
}
