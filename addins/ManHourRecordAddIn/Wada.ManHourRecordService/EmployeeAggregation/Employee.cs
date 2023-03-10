namespace Wada.ManHourRecordService.EmployeeAggregation;

public record class Employee
{
    public Employee(uint employeeNumber, string name, int? departmentID)
    {
        EmployeeNumber = employeeNumber;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DepartmentId = departmentID;
    }

    /// <summary>
    /// 社員番号
    /// </summary>
    public uint EmployeeNumber { get; }

    /// <summary>
    /// 氏名
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 部署ID
    /// </summary>
    public int? DepartmentId { get; }
}

public class TestEmployeeFactory
{
    public static Employee Create(
        uint employeeNumber = 4001u,
        string name = "本社　無人",
        int? departmentID = 4)
        => new(employeeNumber, name, departmentID);
}
