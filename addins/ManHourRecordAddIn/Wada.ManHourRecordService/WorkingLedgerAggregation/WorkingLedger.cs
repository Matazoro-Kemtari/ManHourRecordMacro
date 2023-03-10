using Wada.ManHourRecordService.ValueObjects;

namespace Wada.ManHourRecordService.WorkingLedgerAggregation;

public record class WorkingLedger(uint OwnCompanyNumber, WorkingNumber WorkingNumber, string? JigCode)
{
    /// <summary>
    /// インフラ層専用
    /// </summary>
    /// <param name="ownCompanyNumber"></param>
    /// <param name="workingNumber"></param>
    /// <param name="jigCode"></param>
    /// <returns></returns>
    public static WorkingLedger Reconstruct(uint ownCompanyNumber, WorkingNumber workingNumber, string? jigCode)
        => new(ownCompanyNumber, workingNumber, jigCode);

    public uint OwnCompanyNumber { get; } = OwnCompanyNumber;
    public WorkingNumber WorkingNumber { get; } = WorkingNumber;
    public string? JigCode { get; } = JigCode;
}

public class TestWorkingLedgerFactory
{
    public static WorkingLedger Create(uint ownCompanyNumber = 2002010040u, WorkingNumber? workingNumber = default, string? jigCode = "11C")
    {
        workingNumber ??= new WorkingNumber("02V-40");
        return WorkingLedger.Reconstruct(ownCompanyNumber, workingNumber, jigCode);
    }
}
